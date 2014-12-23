angular.module('games').controller('gamesCtrl', ['$scope', '$routeParams', '$route', '$http', '$q', '$location', 'upload', '$modal', '$filter', 'Games', 'Genres', 'Platforms', 'Tags', 'Users', function($scope, $routeParams, $route, $http, $q, $location, upload, $modal, $filter, Games, Genres, Platforms, Tags, Users) {
    var self = this;

    self.refreshGames = function() {
        return Games.query({userId: self.userId}).$promise.then(function(data) {
            self.games = data;
        });
    };
    self.refreshGenres = function() {
        return Genres.query({userId: self.userId}).$promise.then(function(data) {
            self.genres = data;
        });
    };
    self.refreshPlatforms = function() {
        return Platforms.query({userId: self.userId}).$promise.then(function(data) {
            self.platforms = data;
        });
    };
    self.refreshTags = function() {
        return Tags.query({userId: self.userId}).$promise.then(function(data) {
            self.tags = data;
        });
    };
    self.refreshUsers = function() {
        return Users.query().$promise.then(function(data) {
            self.users = data;
        });
    };

    self.getYears = function() {
        var years = [];
        angular.forEach(self.games, function(game) {
            if(game.year && years.indexOf(game.year) < 0)
                years.push(game.year);
        });

        return years;
    };

    self.countFinished = function() {
        return $filter('filter')(self.games, {finished: 1}).length;
    };
    self.countFinishedPct = function() {
        return Math.round(self.countFinished()/self.games.length*100);
    };

    self.countGenre = function(genre) {
        genre.count = $filter('filter')(self.games, function(game) {
            return game.genre_ids.indexOf(genre.id) > -1;
        }).length;
        return genre.count;
    };
    self.countPlatform = function(platform) {
        platform.count = $filter('filter')(self.games, function(game) {
            return game.platform_ids.indexOf(platform.id) > -1;
        }).length;
        return platform.count;
    };
    self.countTag = function(tag) {
        tag.count = $filter('filter')(self.games, function(game) {
            return game.tag_ids.indexOf(tag.id) > -1;
        }).length;
        return tag.count;
    };

    self.addClick = function() {
        self.openGameForm(null);
    };
    self.editClick = function(game) {
        self.openGameForm(game);
    };

    self.openGameForm = function(game) {
        var modalInstance = $modal.open({
            templateUrl: 'form.html',
            controller: 'formCtrl',
            size: 'lg',
            resolve: {
                game: function() { return game; },
                genres: function() { return self.genres; },
                platforms: function() { return self.platforms; },
                tags: function() { return self.tags; },
                games: function() { return self.games; }
            }
        });
        modalInstance.result.then(function(result) {
            self.formSubmit(game, result.form, result.image, result.delete, result.isNew);
        });
    };

    self.formSubmit = function(game, form, image, del, isNew) {
        if(!form.rating || form.rating.length == 0)
            form.rating = null;

        if(del && form.id) {
            // delete game
            game.$delete({userId: self.userId}, function() {
                var pos = self.games.indexOf(game);
                if(pos > -1)
                    self.games.splice(pos, 1);
            });
        }
        else if(isNew) {
            // add game
            game = new Games(form);
            game.$save({userId: self.userId}, function(data) {
                self.games.push(game);
                if(image)
                    game.uploadImage(self.userId, image);
            });
        }
        else {
            // update game
            var g = new Games(form);
            g.$update({userId: self.userId}, function(data) {
                angular.copy(data, game);
                if(image)
                    game.uploadImage(self.userId, image);
            });
        }
    };

    self.manageGenresClick = function() {
        self.openManageForm(Genres, self.genres, 'genre');
    };

    self.managePlatformsClick = function() {
        self.openManageForm(Platforms, self.platforms, 'platform');
    };

    self.manageTagsClick = function() {
        self.openManageForm(Tags, self.tags, 'tag');
    };

    self.openManageForm = function(Entities, items) {
        var modalInstance = $modal.open({
            templateUrl: 'manage.html',
            controller: 'manageFormCtrl',
            resolve: {
                Entities: function() { return Entities; },
                items: function() { return items; }
            }
        });
        modalInstance.result.then(function(result) {
            var requests = [];

            // handle modified genre/platform/tag list
            angular.forEach(result, function(item) {
                if(item.deleted && item.id) {
                    // delete request
                    requests.push(item.$delete({userId: self.userId}, function() {
                        var pos = -1;
                        for(var i = 0; i < items.length; i++) {
                            var originalItem = items[i];
                            if(originalItem.id == item.id) {
                                pos = i;
                                break;
                            }
                        }

                        if(pos > -1)
                            items.splice(pos, 1);
                    }));
                }
                else if(item.updated && item.id) {
                    // update request
                    requests.push(item.$update({userId: self.userId}, function(data) {
                        var pos = -1;
                        for(var i = 0; i < items.length; i++) {
                            var originalItem = items[i];
                            if(originalItem.id == item.id) {
                                angular.copy(data, originalItem);
                                break;
                            }
                        }
                    }));
                }
                else if(item.added && !item.id) {
                    // add request
                    requests.push(item.$save({userId: self.userId}, function(data) {
                        items.push(data);
                    }));
                }
            });

            // remove deleted genres/platforms/tags from games that reference them
            $q.all(requests).then(function() {
                var ids = Entities.prototype.ids;

                angular.forEach(self.games, function (game) {
                    for(var i = 0; i < game[ids].length; i++) {
                        var id = game[ids][i];
                        var found = false;

                        angular.forEach(items, function(item) {
                            if(id == item.id)
                                found = true;
                        });

                        // if not found in updated list, delete id from game's list
                        if(!found)
                            game[ids].splice(i, 1);
                    }
                });
            });
        });
    };

    self.manageQueueClick = function() {
        var modalInstance = $modal.open({
            templateUrl: 'queue.html',
            controller: 'queueFormCtrl',
            size: 'md',
            resolve: {
                games: function() {
                    return $filter('filter')(self.games, { queue_position: '!!' });
                }
            }
        });
        modalInstance.result.then(function(result) {
            // handle game queue list
            angular.forEach(result, function(game) {
                for(var i = 0; i < self.games.length; i++) {
                    var original = self.games[i];

                    if(original.id == game.id) {
                        original.queue_position = game.queue_position;
                        original.$update({userId: self.userId});
                        break;
                    }
                }
            });
        });
    };

    self.pageChanged = function() {
        self.offset = self.currentPage * self.itemsPerPage - self.itemsPerPage;
    };

    self.loginClick = function() {
        var modalInstance = $modal.open({
            templateUrl: 'login.html',
            controller: 'loginFormCtrl',
            size: 'sm',
            resolve: {
                loginUrl: function() { return 'api/login' }
            }
        });
        modalInstance.result.then(function(result) {
            self.authenticated = true;
            self.authenticated_id = result.id;
        });
    };

    self.logoutClick = function() {
        $http.post('api/logout').success(function() {
            self.authenticated = false;
            self.authenticated_id = false;
        });
    };

    self.aboutClick = function() {
        $modal.open({
            templateUrl: 'about.html',
            size: 'sm'
        });
    };

    self.viewChanged = function(view) {
        if(view != self.GRID_VIEW && view != self.LIST_VIEW)
            return;

        self.view = view;

        if(self.authenticated) {
            angular.forEach(self.users, function(user) {
                if(user.id == self.authenticated_id) {
                    user.view = view;
                    user.$update();
                }
            });
        }
    };

    self.gameSelected = function(game) {
        self.selectedGame = game;

        if(self.view == self.GRID_VIEW)
            self.query = { title: game.title };
    };

    self.openLinkDialog = function(game) {
        $modal.open({
            templateUrl: 'link.html',
            controller: 'linkCtrl',
            size: 'sm',
            resolve: {
                url: function() {
                    var protocol = $location.protocol() + '://';
                    var host = $location.host();
                    var port = ($location.port() != 80 ? ':' + $location.port() : '');
                    var path = window.location.pathname;
                    var userId = '#/' + self.userId;
                    var gameId = '/' + game.id;
                    return protocol + host + port + path + userId + gameId;
                }
            }
        });
    };

    self.hasPlaytime = function(game) {
        return game.hasPlaytime();
    };

    self.refreshAll = function() {
        return $q.all([
            self.refreshGames(),
            self.refreshGenres(),
            self.refreshPlatforms(),
            self.refreshTags(),
            self.refreshUsers()
        ]);
    };

    self.init = function() {
        self.finishedOptions = [
            { name: 'Completed', value: 1 },
            { name: 'Not completed', value: 0 },
            { name: 'N/A', value: 2 }
        ];
        self.offset = 0;
        self.itemsPerPage = 18;
        self.currentPage = 0;
        self.games = [];
        self.genres = [];
        self.platforms = [];
        self.tags = [];
        self.users = [];
        self.selectedGame = null;
        self.GRID_VIEW = 1;
        self.LIST_VIEW = 2;
        self.view = self.LIST_VIEW;

        if($routeParams.userId) {
            self.userId = $routeParams.userId;
            var promise = self.refreshAll();

            if($routeParams.gameId) {
                promise.then(function() {
                    angular.forEach(self.games, function(game) {
                        if(game.id == $routeParams.gameId)
                            self.gameSelected(game);
                    });
                });
            }
        }
        else {
            // no user id specified, load default user and redirect
            $http.get('api/config').success(function(data) {
                self.userId = data.default_user.id;
                $location.path('/' + self.userId).replace();
            });

            return;
        }

        $http.get('api/login').success(function(data) {
            self.authenticated = true;
            self.authenticated_id = data.id;
            self.loginError = false;
            self.loginForm = {
                username: '',
                password: ''
            };

            if(data.view)
                self.view = data.view;
        });
    };

    self.init();
}]);
