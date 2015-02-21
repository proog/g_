angular.module('games').service('gameService', ['Games', 'Genres', 'Platforms', 'Tags', 'Users', '$q', '$routeParams', '$http', '$filter', function(Games, Genres, Platforms, Tags, Users, $q, $routeParams, $http, $filter) {
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
        return $http.get('api/users/'+userId+'/suggestions').success(function(data) {
            self.suggestions = data;
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
            self.refreshUsers()
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
    };

    self.checkLogin = function() {
        return $http.get('api/login').success(function(data) {
            self.authenticated = true;
            self.authenticatedUser = data;
        });
    };

    self.getVisibleGames = function() {
        return $filter('filter')(self.games, function(game) {
            if(game.hidden)
                return self.authenticated && self.authenticatedUser.id == game.user_id;
            return true;
        });
    };
    self.getFinishedGames = function() {
        return $filter('filter')(self.getVisibleGames(), function(game) {
            return game.finished == self.FINISHED;
        });
    };
    self.getUnfinishedGames = function() {
        return $filter('filter')(self.getVisibleGames(), function(game) {
            return game.finished != self.FINISHED;
        });
    };
    self.getYears = function() {
        var years = [];
        angular.forEach(self.getVisibleGames(), function(game) {
            if(game.year && years.indexOf(game.year) < 0)
                years.push(game.year);
        });

        return years;
    };
    self.countFinished = function() {
        return self.getVisibleGames().reduce(function(count, game) {
            return game.finished == self.FINISHED ? count + 1 : count;
        }, 0);
    };
    self.countFinishedPct = function() {
        return Math.round(self.countFinished()/self.getVisibleGames().length*100);
    };
    self.countGenre = function(genre) {
        return self.getVisibleGames().reduce(function(count, game) {
            return game.genre_ids.indexOf(genre.id) > -1 ? count + 1 : count;
        }, 0);
    };
    self.countPlatform = function(platform) {
        return self.getVisibleGames().reduce(function(count, game) {
            return game.platform_ids.indexOf(platform.id) > -1 ? count + 1 : count;
        }, 0);
    };
    self.countTag = function(tag) {
        return self.getVisibleGames().reduce(function(count, game) {
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