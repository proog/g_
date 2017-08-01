angular.module('games').factory('Users', ['$resource', function($resource) {
    var Users = $resource('api/users/:id', {
        id: '@id'
    }, {
        update: {
            method: 'PUT'
        }
    });

    return Users;
}]);