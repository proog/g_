angular.module('games').controller('gamesCtrl', ['$scope', '$routeParams', '$route', '$http', '$q', '$location', 'upload', '$modal', '$filter', 'Games', 'Genres', 'Platforms', 'Tags', 'Users', 'gameService', '$cookies', 'searchFilter', '$timeout', function($scope, $routeParams, $route, $http, $q, $location, upload, $modal, $filter, Games, Genres, Platforms, Tags, Users, gameService, $cookies, searchFilter, $timeout) {
    "use strict";
    var self = this;

    self.countFinished = function() {
        return gameService.countFinished();
    };
    self.countFinishedPct = function() {
        return gameService.countFinishedPct();
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
            game.$save({userId: self.userId}, function() {
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
                loginUrl: function() { return 'api/login'; }
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

        $timeout(function() {
            self.updateChart();
        }, 500); // hack :(
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
        // uncollapse all_games section
        if(self.sections.all_games)
            self.collapseSection('all_games');

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

                        var body = $('body,html');
                        body.animate({scrollTop: target.offset().top - 50 - body.height()/2 + target.height()/2});
                    });

                    break;
                }
            }
        }
        else if(self.view == self.LIST_VIEW) {
            // scroll to the game
            $timeout(function() {
                var target = $('#game'+game.id);
                if(!target.length)
                    return;

                var list = $('#game-list');
                list.animate({scrollTop: list.scrollTop() + target.position().top + 50 - list.height()/2 + target.height()/2});
            });
        }
    };

    self.collapseSection = function(sectionKey) {
        var collapsed = !self.sections[sectionKey];
        self.sections[sectionKey] = collapsed;
        $cookies[sectionKey] = (collapsed ? 1 : 0);

        // fix for when loading the page with statistics collapsed, the chart wouldn't display
        if(sectionKey == 'statistics' && !collapsed) {
            $timeout(function() {
                self.chart.instance.resize(self.chart.instance.render, true);
            });
        }
    };

    self.changeTheme = function() {
        $scope.changeStyle();
        self.updateChart();
    };

    self.updateChart = function() {
        var stats = self.chart.orderBy(self.chart.xAxis(self.chart.yAxis));
        var labels = [];
        var values = [];
        angular.forEach(stats, function(dataPoint) {
            labels.push(dataPoint.label);
            values.push(dataPoint.value);
        });

        var data = {
            labels: labels,
            datasets: [
                {
                    fillColor: 'rgba(151,187,205,0.2)',
                    strokeColor: 'rgba(151,187,205,1)',
                    pointColor: 'rgba(151,187,205,1)',
                    pointStrokeColor: '#fff',
                    pointHighlightFill: '#fff',
                    pointHighlightStroke: 'rgba(151,187,205,1)',
                    data: values
                }
            ]
        };

        var ctx = document.getElementById('stats-chart').getContext('2d');
        var options = {
            animation: false,
            responsive: true,
            scaleBeginsAtZero: true,
            pointHitDetectionRadius: 5,
            scaleFontColor: $scope.globalOptions.darkStyle ? '#fff' : '#333'
        };

        if(self.chart.instance)
            self.chart.instance.destroy();

        self.chart.instance = new Chart(ctx).Line(data, options);
    };

    self.initChart = function() {
        var getPlaytimes = function(games) {
            var playtimes = [];
            angular.forEach(games, function(game) {
                if(game.hasPlaytime())
                    playtimes.push(moment.duration(game.playtime).asHours());
            });
            return playtimes;
        };

        var getRatings = function(games) {
            var ratings = [];
            angular.forEach(games, function(game) {
                if(game.hasRating())
                    ratings.push(game.rating);
            });
            return ratings;
        };

        self.chart = {
            instance: null,
            xAxis: null,
            yAxis: null,
            orderBy: null,
            xOptions: [
                {
                    name: 'Genre',
                    value: function(valueFn) {
                        var stats = [];
                        angular.forEach(gameService.genres, function(genre) {
                            var gamesInCategory = $filter('filter')(gameService.games, function(game) {
                                return game.genre_ids.indexOf(genre.id) > -1;
                            });

                            stats.push({
                                label: genre.name,
                                value: valueFn(gamesInCategory)
                            });
                        });
                        return stats;
                    }
                },
                {
                    name: 'Platform',
                    value: function(valueFn) {
                        var stats = [];
                        angular.forEach(gameService.platforms, function(platform) {
                            var gamesInCategory = $filter('filter')(gameService.games, function(game) {
                                return game.platform_ids.indexOf(platform.id) > -1;
                            });

                            stats.push({
                                label: platform.name,
                                value: valueFn(gamesInCategory)
                            });
                        });
                        return stats;
                    }
                },
                {
                    name: 'Tag',
                    value: function(valueFn) {
                        var stats = [];
                        angular.forEach(gameService.tags, function(tag) {
                            var gamesInCategory = $filter('filter')(gameService.games, function(game) {
                                return game.tag_ids.indexOf(tag.id) > -1;
                            });

                            stats.push({
                                label: tag.name,
                                value: valueFn(gamesInCategory)
                            });
                        });
                        return stats;
                    }
                },
                {
                    name: 'Year',
                    value: function(valueFn) {
                        var stats = [];
                        angular.forEach(gameService.getYears(), function(year) {
                            var gamesInCategory = $filter('filter')(gameService.games, function(game) {
                                return game.year == year;
                            });

                            stats.push({
                                label: year,
                                value: valueFn(gamesInCategory)
                            });
                        });
                        return stats;
                    }
                }
            ],
            yOptions: [
                {
                    name: 'Games',
                    value: function(games) {
                        return games.length;
                    }
                },
                {
                    name: 'Completed games',
                    value: function(games) {
                        return games.reduce(function(count, game) {
                            return game.finished == 1 ? count + 1 : count;
                        }, 0);
                    }
                },
                {
                    name: 'Average playtime',
                    value: function(games) {
                        var playtimes = getPlaytimes(games);

                        if(!playtimes.length)
                            return 0;

                        return playtimes.reduce(function(sum, playtime) {
                            return sum + playtime
                        }, 0) / playtimes.length;
                    }
                },
                {
                    name: 'Median playtime',
                    value: function(games) {
                        var playtimes = getPlaytimes(games);

                        if(!playtimes.length)
                            return 0;

                        var sorted = $filter('orderBy')(playtimes);
                        var half = Math.floor(sorted.length/2);
                        return (sorted.length % 2) ? sorted[half] : (sorted[half-1] + sorted[half]) / 2;
                    }
                },
                {
                    name: 'Maximum playtime',
                    value: function(games) {
                        return getPlaytimes(games).reduce(function(max, playtime) {
                            return Math.max(playtime, max);
                        }, 0);
                    }
                },
                {
                    name: 'Average rating',
                    value: function(games) {
                        var ratings = getRatings(games);

                        if(!ratings.length)
                            return 0;

                        return ratings.reduce(function(sum, rating) {
                            return sum + rating;
                        }, 0) / ratings.length;
                    }
                },
                {
                    name: 'Maximum rating',
                    value: function(games) {
                        return getRatings(games).reduce(function(max, rating) {
                            return Math.max(rating, max);
                        }, 0);
                    }
                }
            ],
            orderOptions: [
                {
                    name: 'Category',
                    value: function(stats) {
                        return $filter('orderBy')(stats, 'label');
                    }
                },
                {
                    name: 'Value',
                    value: function(stats) {
                        return $filter('orderBy')(stats, 'value');
                    }
                }
            ]
        };

        self.chart.xAxis = self.chart.xOptions[0].value;
        self.chart.yAxis = self.chart.yOptions[0].value;
        self.chart.orderBy = self.chart.orderOptions[0].value;
    };

    self.init = function() {
        self.finishedOptions = [
            { name: 'Completed', value: 1 },
            { name: 'Not completed', value: 0 },
            { name: 'N/A', value: 2 }
        ];
        self.sortOptions = [
            { name: 'Sort by title, asc.', value: 'sort_as' },
            { name: 'Sort by title, desc.', value: '-sort_as' },
            { name: 'Sort by year, asc.', value: ['year', 'sort_as'] },
            { name: 'Sort by year, desc.', value: [function(game) {
                return game.year != null ? -game.year : Number.MAX_VALUE; // empty year at end
            }, 'sort_as'] },
            { name: 'Sort by rating, asc.', value: ['rating', 'sort_as'] },
            { name: 'Sort by rating, desc.', value: [function(game) {
                return game.hasRating() ? -game.rating : Number.MAX_VALUE; // empty rating at end
            }, 'sort_as'] },
            { name: 'Sort by playtime, asc.', value: [function(game) {
                return game.hasPlaytime() ? game.playtimeAsInt() : Number.MAX_VALUE; // empty playtime at end
            }, 'sort_as'] },
            { name: 'Sort by playtime, desc.', value: [function(game) {
                return game.hasPlaytime() ? -game.playtimeAsInt() : Number.MAX_VALUE; // empty playtime at end
            }, 'sort_as'] }
        ];
        self.sections = {
            currently_playing: $cookies.currently_playing == 1,
            queue: $cookies.queue == 1,
            all_games: $cookies.all_games == 1,
            statistics: $cookies.statistics == 1
        };

        self.offset = 0;
        self.itemsPerPage = 18;
        self.currentPage = 1;
        self.selectedGame = null;
        self.GRID_VIEW = 1;
        self.LIST_VIEW = 2;
        self.view = $cookies.view ? $cookies.view : self.LIST_VIEW;
        self.gameService = gameService;
        self.resetFilter();
        self.initChart();

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

        var dataLoaded = function() {
            // select game if specified
            if($routeParams.gameId) {
                angular.forEach(gameService.games, function(game) {
                    if(game.id == $routeParams.gameId)
                        self.gameSelected(game);
                });
            }

            $timeout(function() {
                self.updateChart();
            });
        };

        if(!gameService.initialized) {
            // check if session is active
            gameService.checkLogin();

            // data not fetched yet, refresh all
            var promise = gameService.refreshAll(self.userId);
            promise.then(dataLoaded);
        }
        else {
            dataLoaded();
        }

        // for programmatic page changes to work, instead of ng-change
        $scope.$watch(function() { return self.currentPage; }, function() {
            self.pageChanged();
        });
    };

    self.init();
}]);
