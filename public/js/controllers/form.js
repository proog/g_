angular.module('games').controller('formCtrl', ['$scope', '$modalInstance', 'game', 'genres', 'platforms', 'tags', 'games', 'gameService', function($scope, $modalInstance, game, genres, platforms, tags, games, gameService) {
    $scope.initialize = function() {
        $scope.genres = genres;
        $scope.platforms = platforms;
        $scope.tags = tags;
        $scope.games = games;
        $scope.gameService = gameService;

        $scope.STATE_DEFAULT = 0;
        $scope.STATE_ON_WISHLIST = 1;
        $scope.STATE_IN_QUEUE = 2;
        $scope.STATE_CURRENTLY_PLAYING = 3;

        if(game) {
            $scope.isNew = false;
            $scope.model = angular.copy(game);

            if($scope.model.currently_playing)
                $scope.state = $scope.STATE_CURRENTLY_PLAYING;
            else if($scope.model.queue_position != null)
                $scope.state = $scope.STATE_IN_QUEUE;
            else if($scope.model.wishlist_position != null)
                $scope.state = $scope.STATE_ON_WISHLIST;
            else
                $scope.state = $scope.STATE_DEFAULT;
        }
        else {
            $scope.isNew = true;
            $scope.state = $scope.STATE_DEFAULT;
            $scope.model = {
                title: null,
                developer: null,
                publisher: null,
                year: null,
                comment: null,
                sort_as: null,
                finished: gameService.NOT_FINISHED,
                playtime: null,
                rating: null,
                currently_playing: false,
                queue_position: null,
                hidden: false,
                wishlist_position: null,
                genre_ids: [],
                platform_ids: [],
                tag_ids: []
            };
        }

        var maxPos;

        // set up the queue position to toggle between this and null when queue checkbox is checked
        if($scope.model.queue_position != null) {
            $scope.originalQueuePosition = $scope.model.queue_position;
        }
        else {
            // find the potential queue position
            maxPos = $scope.games.reduce(function(ret, game) {
                return game.queue_position != null ? Math.max(game.queue_position, ret) : ret;
            }, -1);
            $scope.originalQueuePosition = maxPos + 1;
        }

        // set up the wishlist position to toggle between this and null when wishlist checkbox is checked
        if($scope.model.wishlist_position != null) {
            $scope.originalWishlistPosition = $scope.model.wishlist_position;
        }
        else {
            // find the potential wishlist position
            maxPos = $scope.games.reduce(function(ret, game) {
                return game.wishlist_position != null ? Math.max(game.wishlist_position, ret) : ret;
            }, -1);
            $scope.originalWishlistPosition = maxPos + 1;
        }
    };

    $scope.genreClick = function(genreId) {
        var pos = $scope.model.genre_ids.indexOf(genreId);
        if(pos > -1)
            $scope.model.genre_ids.splice(pos, 1);
        else
            $scope.model.genre_ids.push(genreId);
    };

    $scope.platformClick = function(platformId) {
        var pos = $scope.model.platform_ids.indexOf(platformId);
        if(pos > -1)
            $scope.model.platform_ids.splice(pos, 1);
        else
            $scope.model.platform_ids.push(platformId);
    };

    $scope.tagClick = function(tagId) {
        var pos = $scope.model.tag_ids.indexOf(tagId);
        if(pos > -1)
            $scope.model.tag_ids.splice(pos, 1);
        else
            $scope.model.tag_ids.push(tagId);
    };

    $scope.clearImageInput = function() {
        for(var i = 0; i < $scope.image.length; i++) {
            try{
                $scope.image[i].value = '';
                if($scope.image[i].value){
                    $scope.image[i].type = "text";
                    $scope.image[i].type = "file";
                }
            } catch(e){}
        }
    };

    $scope.titleChanged = function() {
        $scope.model.sort_as = $scope.model.title.replace(/^The\s|A\s|An\s/, '');
    };

    $scope.currentlyPlayingClicked = function() {
        $scope.model.currently_playing = true;
        $scope.model.queue_position = null;
        $scope.model.wishlist_position = null;
    };

    $scope.inQueueClicked = function() {
        $scope.model.currently_playing = false;
        $scope.model.queue_position = $scope.originalQueuePosition;
        $scope.model.wishlist_position = null;
    };

    $scope.onWishlistClicked = function() {
        $scope.model.currently_playing = false;
        $scope.model.queue_position = null;
        $scope.model.wishlist_position = $scope.originalWishlistPosition;

        $scope.model.finished = gameService.NOT_FINISHED;
        $scope.model.rating = null;
        $scope.model.playtime = null;
    };

    $scope.defaultClicked = function() {
        $scope.model.currently_playing = false;
        $scope.model.queue_position = null;
        $scope.model.wishlist_position = null;
    };

    $scope.formSubmit = function() {
        if($scope.isNew)
            $scope.addClick();
        else if(!$scope.isNew)
            $scope.saveClick();
    };

    $scope.addClick = function() {
        var result = {
            model: $scope.model,
            image: $scope.image,
            delete: false,
            isNew: true
        };
        $modalInstance.close(result);
    };

    $scope.saveClick = function() {
        var result = {
            model: $scope.model,
            image: $scope.image,
            delete: false,
            isNew: false
        };
        $modalInstance.close(result);
    };

    $scope.deleteClick = function() {
        var dialog = confirm('Are you sure you want to delete ' + $scope.model.title + '?');
        if(!dialog)
            return;

        var result = {
            model: $scope.model,
            image: $scope.image,
            delete: true,
            isNew: false
        };
        $modalInstance.close(result);
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.initialize();
}]);
