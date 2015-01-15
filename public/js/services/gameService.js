angular.module('games').service('gameService', ['Games', 'Genres', 'Platforms', 'Tags', 'Users', '$q', '$routeParams', '$http', function(Games, Genres, Platforms, Tags, Users, $q, $routeParams, $http) {
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
    self.refreshAll = function(userId) {
        self.resetAll();

        return $q.all([
            self.refreshGames(userId),
            self.refreshGenres(userId),
            self.refreshPlatforms(userId),
            self.refreshTags(userId),
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
        self.users = [];
    };

    self.checkLogin = function() {
        return $http.get('api/login').success(function(data) {
            self.authenticated = true;
            self.authenticatedUser = data;
        });
    };

    self.getFinishedGames = function() {
        return $filter('filter')(self.games, function(game) {
            return game.finished == 1;
        });
    };
    self.getUnfinishedGames = function() {
        return $filter('filter')(self.games, function(game) {
            return game.finished != 1;
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
        return self.games.reduce(function(count, game) {
            return game.finished == 1 ? count + 1 : count;
        }, 0);
    };
    self.countFinishedPct = function() {
        return Math.round(self.countFinished()/self.games.length*100);
    };
    self.countGenre = function(genre) {
        genre.count = self.games.reduce(function(count, game) {
            return game.genre_ids.indexOf(genre.id) > -1 ? count + 1 : count;
        }, 0);
        return genre.count;
    };
    self.countPlatform = function(platform) {
        platform.count = self.games.reduce(function(count, game) {
            return game.platform_ids.indexOf(platform.id) > -1 ? count + 1 : count;
        }, 0);
        return platform.count;
    };
    self.countTag = function(tag) {
        tag.count = self.games.reduce(function(count, game) {
            return game.tag_ids.indexOf(tag.id) > -1 ? count + 1 : count;
        }, 0);
        return tag.count;
    };

    self.FILTER_TITLE = 1;
    self.FILTER_PLATFORM = 2;
    self.FILTER_GENRE = 3;
    self.FILTER_TAG = 4;
    self.FILTER_YEAR = 5;
    self.FILTER_RATING = 6;
    self.FILTER_COMPLETION = 7;

    self.authenticated = false;
    self.authenticatedUser = null;
    self.resetAll();
}]);