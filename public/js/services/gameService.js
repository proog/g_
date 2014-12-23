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

    self.games = [];
    self.genres = [];
    self.platforms = [];
    self.tags = [];
    self.users = [];
    self.GRID_VIEW = 1;
    self.LIST_VIEW = 2;
    self.view = self.LIST_VIEW;
    self.initialized = false;

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
}]);