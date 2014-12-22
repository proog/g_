angular.module('games').directive('game', function() {
    return {
        restrict: 'E',
        templateUrl: 'game.html',
        scope: {
            game: '=',
            genres: '=',
            platforms: '=',
            tags: '=',
            showEdit: '=',
            editCallback: '&',
            linkCallback: '&',
            height: '@'
        },
        link: function($scope, element, attributes) {
            $scope.heightStyle = $scope.height ? {height: $scope.height} : {};

            $scope.getGenre = function(id) {
                for(var i = 0; i < $scope.genres.length; i++) {
                    var genre = $scope.genres[i];
                    if(id == genre.id)
                        return genre;
                }
            };

            $scope.getPlatform = function(id) {
                for(var i = 0; i < $scope.platforms.length; i++) {
                    var platform = $scope.platforms[i];
                    if(id == platform.id)
                        return platform;
                }
            };

            $scope.getTag = function(id) {
                for(var i = 0; i < $scope.tags.length; i++) {
                    var tag = $scope.tags[i];
                    if(id == tag.id)
                        return tag;
                }
            };
        }
    };
});
