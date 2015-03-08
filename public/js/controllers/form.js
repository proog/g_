angular.module('games').controller('formCtrl', ['$scope', '$modalInstance', 'game', 'gameService', 'Games', '$http', function($scope, $modalInstance, game, gameService, Games, $http) {
    $scope.initialize = function() {
        $scope.genres = gameService.genres;
        $scope.platforms = gameService.platforms;
        $scope.tags = gameService.tags;
        $scope.gameService = gameService;
        $scope.error = null;
        $scope.externalImageUrl = null;

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
            maxPos = gameService.games.reduce(function(ret, game) {
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
            maxPos = gameService.games.reduce(function(ret, game) {
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
        if($scope.image && $scope.image[0].files.length > 0 || $scope.externalImageUrl) {
            for(var i = 0; i < $scope.image.length; i++) {
                try{
                    $scope.image[i].value = '';
                    if($scope.image[i].value){
                        $scope.image[i].type = "text";
                        $scope.image[i].type = "file";
                    }
                } catch(e){}
            }

            $scope.filename = null;
            $scope.externalImageUrl = null;
        }
        else if(!$scope.isNew && $scope.model.image) {
            var dialog = confirm('Do you want to deleted the image for ' + $scope.model.title + '? This cannot be undone.');
            if(!dialog)
                return;

            // delete associated cover image
            game.deleteImage();
        }
    };

    $scope.titleChanged = function() {
        if($scope.model.title)
            $scope.model.sort_as = $scope.model.title.replace(/^The\s|A\s|An\s/, '');
        else
            $scope.model.sort_as = null;
    };

    $scope.yearButtonClicked = function() {
        $scope.model.year = new Date().getFullYear();
    };

    $scope.fileChanged = function() {
        $scope.$apply(function() {
            if($scope.image && $scope.image[0] && $scope.image[0].files && $scope.image[0].files.length > 0) {
                $scope.filename = $scope.image[0].files[0].name;
            }
            else {
                $scope.filename = null;
            }
        });
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
        var handleImage = function() {
            if($scope.image && $scope.image[0].files.length > 0) {
                // image from upload form
                game.uploadImage($scope.image).then(function() {
                    $modalInstance.close();
                }, function(response) {
                    $scope.error = response.data.message;
                });
            }
            else if($scope.externalImageUrl) {
                // image from suggestion api
                game.uploadExternalImage($scope.externalImageUrl).then(function() {
                    $modalInstance.close();
                }, function(response) {
                    $scope.error = response.data.message;
                });
            }
            else {
                // no image specified
                $modalInstance.close();
            }
        };

        if(!$scope.model.rating || $scope.model.rating.length == 0)
            $scope.model.rating = null;

        if($scope.isNew) {
            game = new Games($scope.model);
            game.$save({userId: gameService.authenticatedUser.id}, function() {
                // add saved game to games list
                gameService.games.push(game);
                handleImage();
            }, function(response) {
                $scope.error = response.data.message;
            });
        }
        else {
            var g = new Games($scope.model);
            g.$update(function(data) {
                // copy data back to original game
                angular.copy(data, game);
                handleImage();
            }, function(response) {
                $scope.error = response.data.message;
            });
        }
    };

    $scope.deleteClick = function() {
        var dialog = confirm('Are you sure you want to delete ' + $scope.model.title + '?');
        if(!dialog)
            return;

        game.$delete(function() {
            var pos = gameService.games.indexOf(game);
            if(pos > -1)
                gameService.games.splice(pos, 1);

            $modalInstance.close();
        }, function(response) {
            $scope.error = response.data.message;
        });
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.closeAlert = function() {
        $scope.error = null;
    };

    $scope.getSuggestions = function(search) {
        return gameService.assistedCreationSearch(search);
    };

    $scope.suggestionSelected = function(id) {
        gameService.assistedCreationGetGame(id).then(function(data) {
            $scope.model.title = data.title;
            $scope.titleChanged();

            $scope.model.developer = data.developer;
            $scope.model.publisher = data.publisher;
            $scope.model.year = data.year;

            if(data.genre_ids.length > 0) {
                $scope.model.genre_ids = data.genre_ids;
            }

            if(data.platform_ids.length > 0) {
                $scope.model.platform_ids = data.platform_ids;
            }

            if(data.image_url) {
                $scope.externalImageUrl = data.image_url;
                $scope.filename = data.image_url;
            }
        });
    };

    $scope.initialize();
}]);
