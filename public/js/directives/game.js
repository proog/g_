angular.module('games').directive('game', ['gameService', function(gameService) {
    return {
        restrict: 'E',
        templateUrl: 'game.html',
        scope: {
            game: '=',
            editCallback: '&',
            linkCallback: '&',
            height: '@'
        },
        link: function($scope, element, attributes) {
            $scope.heightStyle = $scope.height ? {height: $scope.height} : {};
            $scope.gameService = gameService;

            $scope.getGenre = function(id) {
                for(var i = 0; i < gameService.genres.length; i++) {
                    var genre = gameService.genres[i];
                    if(id == genre.id)
                        return genre;
                }
            };

            $scope.getPlatform = function(id) {
                for(var i = 0; i < gameService.platforms.length; i++) {
                    var platform = gameService.platforms[i];
                    if(id == platform.id)
                        return platform;
                }
            };

            $scope.getTag = function(id) {
                for(var i = 0; i < gameService.tags.length; i++) {
                    var tag = gameService.tags[i];
                    if(id == tag.id)
                        return tag;
                }
            };
        }
    };
}]);
