angular.module('games').controller('formCtrl', ['$scope', '$modalInstance', 'game', 'genres', 'platforms', 'tags', 'games', function($scope, $modalInstance, game, genres, platforms, tags, games) {
    $scope.initialize = function() {
        $scope.genres = genres;
        $scope.platforms = platforms;
        $scope.tags = tags;
        $scope.games = games;

        if(game) {
            $scope.isNew = false;
            $scope.model = angular.copy(game);
        }
        else {
            $scope.isNew = true;
            $scope.model = {
                title: null,
                developer: null,
                publisher: null,
                year: null,
                comment: null,
                sort_as: null,
                finished: 0,
                playtime: null,
                rating: null,
                currently_playing: false,
                queue_position: null,
                genre_ids: [],
                platform_ids: [],
                tag_ids: []
            };
        }

        // set up the queue position to toggle between this and null when queue checkbox is checked
        if($scope.model.queue_position != null) {
            $scope.originalQueuePosition = $scope.model.queue_position;
        }
        else {
            // find the potential queue position
            var maxPos = -1;
            angular.forEach($scope.games, function(game) {
                if(game.queue_position != null && game.queue_position > maxPos)
                    maxPos = game.queue_position;
            });
            $scope.originalQueuePosition = maxPos + 1;
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

    $scope.inQueueClicked = function() {
        if($scope.model.queue_position == null)
            $scope.model.queue_position = $scope.originalQueuePosition;
        else
            $scope.model.queue_position = null;
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
