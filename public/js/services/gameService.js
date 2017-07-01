angular.module('games').service('gameService', ['Games', 'Genres', 'Platforms', 'Tags', 'Users', '$q', '$routeParams', '$http', '$filter', 'Config', '$httpParamSerializerJQLike', function(Games, Genres, Platforms, Tags, Users, $q, $routeParams, $http, $filter, Config, $httpParamSerializerJQLike) {
    var self = this;

    self.refreshGames = function(userId) {
        return Games.query({userId: userId}).$promise.then(function(data) {
            self.games = data;
        });
    };
    self.refreshGenres = function(userId) {
        return Genres.query({userId: userId}).$promise.then(function(data) {
            self.genres = data;
        });
    };
    self.refreshPlatforms = function(userId) {
        return Platforms.query({userId: userId}).$promise.then(function(data) {
            self.platforms = data;
        });
    };
    self.refreshTags = function(userId) {
        return Tags.query({userId: userId}).$promise.then(function(data) {
            self.tags = data;
        });
    };
    self.refreshUsers = function() {
        return Users.query().$promise.then(function(data) {
            self.users = data;
        });
    };
    self.refreshSuggestions = function(userId) {
        return $http.get('api/users/'+userId+'/suggestions').then(function(response) {
            self.suggestions = response.data;
        });
    };
    self.refreshConfig = function() {
        return Config.get().$promise.then(function(data) {
            self.config = data;
        });
    };
    self.refreshAll = function(userId) {
        self.resetAll();

        return $q.all([
            self.refreshGames(userId),
            self.refreshGenres(userId),
            self.refreshPlatforms(userId),
            self.refreshTags(userId),
            self.refreshSuggestions(userId),
            self.refreshUsers(),
            self.refreshConfig()
        ]).then(function() {
            self.initialized = true;
        });
    };
    self.resetAll = function() {
        self.initialized = false;
        self.games = [];
        self.genres = [];
        self.platforms = [];
        self.tags = [];
        self.suggestions = [];
        self.users = [];
        self.config = null;
    };

    // if username is not specified, use a get to see if we have an active session
    self.logIn = function(username, password) {
        if(!username && !password) {
            return self.checkLogin();
        }

        return $http({
            url: 'api/token',
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            data: $httpParamSerializerJQLike({
                grant_type: 'password',
                username: username,
                password: password
            })
        }).then(function(response) {
            window.sessionStorage.setItem('token', response.data.access_token);
        }).then(self.checkLogin);
    };

    self.checkLogin = function() {
        return $http.get('api/login').then(function(response) {
            var data = response.data;
            self.authenticated = true;
            self.authenticatedUser = data;
            return data;
        });
    };

    self.logOut = function() {
        return $http.post('api/logout').then(function() {
            self.authenticated = false;
            self.authenticatedUser = null;
        });
    };

    self.assistedCreationSearch = function(search) {
        return $http.get('api/assisted/search/'+encodeURIComponent(search)).then(function(response) {
            return response.data;
        });
    };

    self.assistedCreationGetGame = function(id) {
        return $http.get('api/assisted/game/'+id).then(function(response) {
            return response.data;
        });
    };

    self.isGameVisible = function(game) {
        return !game.hidden || (self.authenticated && self.authenticatedUser.id == game.user_id);
    };
    self.getOwnedGames = function() {
        return $filter('filter')(self.games, function(game) {
            if(game.isOnWishlist())
                return false;

            return self.isGameVisible(game);
        });
    };
    self.getWishlistGames = function() {
        return $filter('filter')(self.games, function(game) {
            if(!game.isOnWishlist())
                return false;

            return self.isGameVisible(game);
        });
    };
    self.getQueuedGames = function() {
        return $filter('filter')(self.getOwnedGames(), function(game) {
            return game.isInQueue();
        });
    };
    self.getFinishedGames = function() {
        return $filter('filter')(self.getOwnedGames(), function(game) {
            return game.finished == self.FINISHED;
        });
    };
    self.getUnfinishedGames = function() {
        return $filter('filter')(self.getOwnedGames(), function(game) {
            return game.finished != self.FINISHED;
        });
    };
    self.getYears = function() {
        var years = [];
        angular.forEach(self.getOwnedGames(), function(game) {
            if(game.year && years.indexOf(game.year) < 0)
                years.push(game.year);
        });

        return years;
    };
    self.countFinished = function() {
        return self.getOwnedGames().reduce(function(count, game) {
            return game.finished == self.FINISHED ? count + 1 : count;
        }, 0);
    };
    self.countFinishedPct = function() {
        return Math.round(self.countFinished()/self.getOwnedGames().length*100);
    };
    self.countGenre = function(genre) {
        return self.getOwnedGames().reduce(function(count, game) {
            return game.genre_ids.indexOf(genre.id) > -1 ? count + 1 : count;
        }, 0);
    };
    self.countPlatform = function(platform) {
        return self.getOwnedGames().reduce(function(count, game) {
            return game.platform_ids.indexOf(platform.id) > -1 ? count + 1 : count;
        }, 0);
    };
    self.countTag = function(tag) {
        return self.getOwnedGames().reduce(function(count, game) {
            return game.tag_ids.indexOf(tag.id) > -1 ? count + 1 : count;
        }, 0);
    };
    self.isInSuggestions = function(game) {
        return self.suggestions.reduce(function(ret, suggestion) {
            return ret || suggestion.game_id == game.id;
        }, false);
    };

    self.FILTER_TITLE = 1;
    self.FILTER_PLATFORM = 2;
    self.FILTER_GENRE = 3;
    self.FILTER_TAG = 4;
    self.FILTER_YEAR = 5;
    self.FILTER_RATING = 6;
    self.FILTER_COMPLETION = 7;
    self.FILTER_TITLE = 8;
    self.FILTER_YEAR_MIN = 9;
    self.FILTER_YEAR_MAX = 10;
    self.FILTER_RATING_MIN = 11;
    self.FILTER_RATING_MAX = 12;

    self.NOT_FINISHED = 0;
    self.FINISHED = 1;
    self.FINISHED_NA = 2;
    self.SHELVED = 3;

    self.authenticated = false;
    self.authenticatedUser = null;
    self.resetAll();
}]);