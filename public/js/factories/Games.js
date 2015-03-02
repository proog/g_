angular.module('games').factory('Games', ['$resource', 'upload', '$http', function($resource, upload, $http) {
    var Games = $resource('api/users/:userId/games/:id', {
        id: '@id',
        userId: '@user_id'
    }, {
        query: {
            method: 'GET',
            isArray: true,
            transformResponse: function(data) {
                var games = angular.fromJson(data);

                angular.forEach(games, function(game) {
                    initializeGame(game);
                });

                return games;
            }
        },
        get: {
            method: 'GET',
            isArray: false,
            transformResponse: function(data) {
                var game = angular.fromJson(data);
                return initializeGame(game);
            }
        },
        save: {
            method: 'POST',
            transformResponse: function(data) {
                var game = angular.fromJson(data);
                return initializeGame(game);
            }
        },
        update: {
            method: 'PUT',
            transformResponse: function(data) {
                var game = angular.fromJson(data);
                return initializeGame(game);
            }
        }
    });

    var initializeGame = function(game) {
        game.decachedImage = game.image;

        if(game.title) {
            var splitter = ': ';
            var splitterPos = game.title.indexOf(splitter);
            if(splitterPos > -1) {
                game.mainTitle = game.title.substr(0, splitterPos);
                game.subTitle = game.title.substr(splitterPos + splitter.length);
            }
            else game.mainTitle = game.title;
        }

        return game;
    };

    Games.prototype.uploadImage = function(image) {
        var self = this;
        return upload({
            url: 'api/users/' + self.user_id + '/games/' + self.id + '/image',
            method: 'POST',
            data: {
                image: image
            }
        }).then(function(response) {
            var data = response.data;
            self.image = data.image;
            self.decachedImage = self.image + '?q=' + new Date().getTime();
        });
    };

    Games.prototype.uploadExternalImage = function(url) {
        var self = this;
        return $http.post('api/users/' + self.user_id + '/games/' + self.id + '/image', { image_url: url }).then(function(response) {
            var data = response.data;
            self.image = data.image;
            self.decachedImage = self.image + '?q=' + new Date().getTime();
        });
    };

    Games.prototype.deleteImage = function() {
        var self = this;
        return $http.delete('api/users/' + self.user_id + '/games/' + self.id + '/image').success(function() {
            self.image = null;
            self.decachedImage = null;
        });
    };

    Games.prototype.playtimeAsInt = function() {
        if(!this.hasPlaytime())
            return 0;

        return parseInt(this.playtime.split(':').join(''));
    };

    Games.prototype.hasPlaytime = function() {
        return this.playtime && this.playtime != '00:00:00';
    };

    Games.prototype.hasRating = function() {
        return this.rating != null;
    };

    Games.prototype.isOnWishlist = function() {
        return this.wishlist_position !== null;
    };

    Games.prototype.isInQueue = function() {
        return this.queue_position !== null;
    };

    Games.prototype.getPlaytimeDisplay = function() {
        if(!this.playtime)
            return '';

        var split = this.playtime.split(':');
        var hours = split[0];
        var mins = split[1];

        if(hours.indexOf('00') == 0)
            hours = '';
        else if(hours.indexOf('0') == 0)
            hours = hours.substr(1) + ' hours';
        else
            hours = hours + ' hours';

        if(mins.indexOf('00') == 0)
            mins = '';
        else if(mins.indexOf('0') == 0)
            mins = mins.substr(1) + ' mins';
        else
            mins = mins + ' mins';

        return hours + ' ' + mins;
    };

    return Games;
}]);
