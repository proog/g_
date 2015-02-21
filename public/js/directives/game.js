angular.module('games').directive('game', ['gameService', '$filter', function(gameService, $filter) {
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

            $scope.getGenres = function() {
                return $filter('filter')(gameService.genres, function(genre) {
                    return $scope.game.genre_ids.indexOf(genre.id) > -1;
                });
            };

            $scope.getPlatforms = function() {
                return $filter('filter')(gameService.platforms, function(platform) {
                    return $scope.game.platform_ids.indexOf(platform.id) > -1;
                });
            };

            $scope.getTags = function() {
                return $filter('filter')(gameService.tags, function(tag) {
                    return $scope.game.tag_ids.indexOf(tag.id) > -1;
                });
            };
        }
    };
}]);
