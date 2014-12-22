angular.module('games').controller('formCtrl', ['$scope', '$modalInstance', 'game', 'genres', 'platforms', 'tags', 'games', function($scope, $modalInstance, game, genres, platforms, tags, games) {
    $scope.initialize = function() {
        $scope.genres = genres;
        $scope.platforms = platforms;
        $scope.tags = tags;
        $scope.games = games;

        if(game) {
            $scope.isNew = false;
            $scope.form = angular.copy(game);
        }
        else {
            $scope.isNew = true;
            $scope.form = {
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
    };

    $scope.genreClick = function(genreId) {
        var pos = $scope.form.genre_ids.indexOf(genreId);
        if(pos > -1)
            $scope.form.genre_ids.splice(pos, 1);
        else
            $scope.form.genre_ids.push(genreId);
    };

    $scope.platformClick = function(platformId) {
        var pos = $scope.form.platform_ids.indexOf(platformId);
        if(pos > -1)
            $scope.form.platform_ids.splice(pos, 1);
        else
            $scope.form.platform_ids.push(platformId);
    };

    $scope.tagClick = function(tagId) {
        var pos = $scope.form.tag_ids.indexOf(tagId);
        if(pos > -1)
            $scope.form.tag_ids.splice(pos, 1);
        else
            $scope.form.tag_ids.push(tagId);
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
        if(!$scope.isNew)
            return;

        $scope.form.sort_as = $scope.form.title.replace(/^The\s|A\s/, '');
    };

    $scope.inQueueClicked = function() {
        if($scope.form.queue_position == null) {
            // set the queue position to the largest queue position + 1
            var maxPos = -1;
            angular.forEach($scope.games, function(game) {
                if(game.queue_position != null && game.queue_position > maxPos)
                    maxPos = game.queue_position;
            });

            $scope.form.queue_position = maxPos + 1;
        }
        else $scope.form.queue_position = null;
    };

    $scope.formSubmit = function() {
        if($scope.isNew)
            $scope.addClick();
        else if(!$scope.isNew)
            $scope.saveClick();
    };

    $scope.addClick = function() {
        var result = {
            form: $scope.form,
            image: $scope.image,
            delete: false,
            isNew: true
        };
        $modalInstance.close(result);
    };

    $scope.saveClick = function() {
        var result = {
            form: $scope.form,
            image: $scope.image,
            delete: false,
            isNew: false
        };
        $modalInstance.close(result);
    };

    $scope.deleteClick = function() {
        var dialog = confirm('Are you sure you want to delete ' + $scope.form.title + '?');
        if(!dialog)
            return;

        var result = {
            form: $scope.form,
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
