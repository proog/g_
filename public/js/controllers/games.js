angular.module('games').controller('gamesCtrl', ['$scope', '$routeParams', '$route', '$http', '$q', '$location', 'upload', '$modal', '$filter', 'Games', 'Genres', 'Platforms', 'Tags', 'Users', 'gameService', '$cookies', 'searchFilter', '$timeout', function($scope, $routeParams, $route, $http, $q, $location, upload, $modal, $filter, Games, Genres, Platforms, Tags, Users, gameService, $cookies, searchFilter, $timeout) {
    var self = this;

    self.getYears = function() {
        var years = [];
        angular.forEach(gameService.games, function(game) {
            if(game.year && years.indexOf(game.year) < 0)
                years.push(game.year);
        });

        return years;
    };

    self.countFinished = function() {
        return $filter('filter')(gameService.games, {finished: 1}).length;
    };
    self.countFinishedPct = function() {
        return Math.round(self.countFinished()/gameService.games.length*100);
    };

    self.countGenre = function(genre) {
        genre.count = $filter('filter')(gameService.games, function(game) {
            return game.genre_ids.indexOf(genre.id) > -1;
        }).length;
        return genre.count;
    };
    self.countPlatform = function(platform) {
        platform.count = $filter('filter')(gameService.games, function(game) {
            return game.platform_ids.indexOf(platform.id) > -1;
        }).length;
        return platform.count;
    };
    self.countTag = function(tag) {
        tag.count = $filter('filter')(gameService.games, function(game) {
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
                genres: function() { return gameService.genres; },
                platforms: function() { return gameService.platforms; },
                tags: function() { return gameService.tags; },
                games: function() { return gameService.games; }
            }
        });
        modalInstance.result.then(function(result) {
            self.formSubmit(game, result.model, result.image, result.delete, result.isNew);
        });
    };

    self.formSubmit = function(game, model, image, del, isNew) {
        if(!model.rating || model.rating.length == 0)
            model.rating = null;

        if(del && model.id) {
            // delete game
            game.$delete({userId: self.userId}, function() {
                var pos = gameService.games.indexOf(game);
                if(pos > -1)
                    gameService.games.splice(pos, 1);
            });
        }
        else if(isNew) {
            // add game
            game = new Games(model);
            game.$save({userId: self.userId}, function(data) {
                gameService.games.push(game);
                if(image && image[0].files.length > 0)
                    game.uploadImage(self.userId, image);
            });
        }
        else {
            // update game
            var g = new Games(model);
            g.$update({userId: self.userId}, function(data) {
                angular.copy(data, game);
                if(image && image[0].files.length > 0)
                    game.uploadImage(self.userId, image);
            });
        }
    };

    self.manageGenresClick = function() {
        self.openManageForm(Genres, gameService.genres, 'genre');
    };

    self.managePlatformsClick = function() {
        self.openManageForm(Platforms, gameService.platforms, 'platform');
    };

    self.manageTagsClick = function() {
        self.openManageForm(Tags, gameService.tags, 'tag');
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

                angular.forEach(gameService.games, function (game) {
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
            size: 'sm',
            resolve: {
                games: function() {
                    return $filter('filter')(gameService.games, { queue_position: '!!' });
                }
            }
        });
        modalInstance.result.then(function(result) {
            // handle game queue list
            angular.forEach(result, function(game) {
                for(var i = 0; i < gameService.games.length; i++) {
                    var original = gameService.games[i];

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
            gameService.authenticated = true;
            gameService.authenticatedUser = result;
        });
    };

    self.logoutClick = function() {
        $http.post('api/logout').success(function() {
            gameService.authenticated = false;
            gameService.authenticatedUser = null;
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
        $cookies.view = view;
    };

    self.gameSelected = function(game, scroll) {
        self.selectedGame = game;

        if(scroll)
            self.scrollToGame(game);
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

    self.changeUser = function(user) {
        gameService.resetAll();
        $location.path('/' + user.id);
    };

    self.hasPlaytime = function(game) {
        return game.hasPlaytime();
    };

    self.canShowAuthenticatedMenuItems = function() {
        return gameService.authenticated && gameService.authenticatedUser.id == self.userId && gameService.initialized;
    };

    self.resetFilter = function() {
        self.query = {};
        self.sorting = self.sortOptions[0].value;
    };

    self.scrollToGame = function(game) {
        if(self.view == self.GRID_VIEW) {
            // find the page of the game, change to it, and scroll to the game
            for(var i = 0; i < self.filtered.length; i++) {
                var item = self.filtered[i];
                if(item == game) {
                    self.currentPage = parseInt(i/self.itemsPerPage)+1;

                    $timeout(function() {
                        var target = $('#game'+game.id);
                        if(!target.length)
                            return;

                        var body = $('body');
                        body.animate({scrollTop: target.offset().top - 50 - body.height()/2 + target.height()/2});
                    });

                    break;
                }
            }
        }
        else if(self.view == self.LIST_VIEW) {
            // scroll to the game
            var target = $('#game'+game.id);
            if(!target.length)
                return;

            var list = $('#game-list');
            list.animate({scrollTop: list.scrollTop() + target.position().top + 50 - list.height()/2 + target.height()/2});
        }
    };

    self.collapseSection = function(sectionKey) {
        var collapsed = !self.sections[sectionKey];
        self.sections[sectionKey] = collapsed;
        $cookies[sectionKey] = (collapsed ? 1 : 0);
    };

    self.init = function() {
        self.finishedOptions = [
            { name: 'Completed', value: 1 },
            { name: 'Not completed', value: 0 },
            { name: 'N/A', value: 2 }
        ];
        self.sortOptions = [
            { name: 'Sort by title', value: 'sort_as' },
            { name: 'Sort by year', value: ['year', 'sort_as'] },
            { name: 'Sort by rating', value: ['rating', 'sort_as'] }
        ];
        self.sections = {
            currently_playing: $cookies.currently_playing == 1,
            queue: $cookies.queue == 1,
            all_games: $cookies.all_games == 1,
            statistics: $cookies.statistics == 1
        };

        self.sorting = self.sortOptions[0].value;
        self.offset = 0;
        self.itemsPerPage = 18;
        self.currentPage = 1;
        self.selectedGame = null;
        self.GRID_VIEW = 1;
        self.LIST_VIEW = 2;
        self.view = $cookies.view ? $cookies.view : self.LIST_VIEW;
        self.gameService = gameService;

        if(!$routeParams.userId) {
            // no user id specified, load default user and redirect
            $http.get('api/config').success(function(data) {
                self.userId = data.default_user.id;
                $location.path('/' + self.userId).replace();
            });
            return;
        }

        // user id specified, load data and linked game
        self.userId = $routeParams.userId;

        var findGameFn = function() {
            angular.forEach(gameService.games, function(game) {
                if(game.id == $routeParams.gameId)
                    self.gameSelected(game);
            });
        };

        if(!gameService.initialized) {
            // check if session is active
            gameService.checkLogin();

            // data not fetched yet, refresh all
            var promise = gameService.refreshAll(self.userId);
            if($routeParams.gameId)
                promise.then(findGameFn);
        }
        else {
            findGameFn();
        }

        // for programmatic page changes to work, instead of ng-change
        $scope.$watch(function() { return self.currentPage; }, function() {
            self.pageChanged();
        });
    };

    self.init();
}]);
