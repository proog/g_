angular.module('games')
.factory('Genres', ['$resource', function($resource) {
    var Genres = $resource('api/users/:userId/genres/:id', {
        id: '@id',
        userId: '@user_id'
    }, {
        update: {
            method: 'PUT'
        }
    });

    Genres.prototype.type = 'genre';
    Genres.prototype.ids = 'genre_ids';

    return Genres;
}])
.factory('Platforms', ['$resource', function($resource) {
    var Platforms = $resource('api/users/:userId/platforms/:id', {
        id: '@id',
        userId: '@user_id'
    }, {
        update: {
            method: 'PUT'
        }
    });

    Platforms.prototype.type = 'platform';
    Platforms.prototype.ids = 'platform_ids';

    return Platforms;
}])
.factory('Tags', ['$resource', function($resource) {
    var Tags = $resource('api/users/:userId/tags/:id', {
        id: '@id',
        userId: '@user_id'
    }, {
        update: {
            method: 'PUT'
        }
    });

    Tags.prototype.type = 'tag';
    Tags.prototype.ids = 'tag_ids';

    return Tags;
}])
.factory('Config', ['$resource', function($resource) {
    var Config = $resource('api/config');

    return Config;
}])
.factory('Settings', ['$resource', function($resource) {
    var Settings = $resource('api/settings', {}, {
        update: {
            method: 'PUT'
        }
    });

    return Settings;
}]);
