angular.module('games').controller('queueFormCtrl', ['$scope', '$modalInstance', '$filter', 'games', function($scope, $modalInstance, $filter, games) {
    $scope.initialize = function() {
        $scope.games = angular.copy(games);

        // set the queue position relative to the first element
        $scope.games = $filter('orderBy')($scope.games, ['queue_position', 'sort_as']);
        for(var i = 0; i < $scope.games.length; i++)
            $scope.games[i].queue_position = i;
    };

    $scope.moveUpClick = function(game) {
        if(game.queue_position == 0)
            return;

        game.queue_position--;

        // swap position with the previous item
        angular.forEach($scope.games, function(item) {
            if(item.id != game.id && item.queue_position == game.queue_position) {
                item.queue_position++;
            }
        });
    };

    $scope.moveDownClick = function(game) {
        if(game.queue_position == $scope.games.length - 1)
            return;

        game.queue_position++;

        // swap position with the next item
        angular.forEach($scope.games, function(item) {
            if(item.id != game.id && item.queue_position == game.queue_position) {
                item.queue_position--;
            }
        });
    };

    $scope.saveClick = function() {
        var result = $scope.games;
        $modalInstance.close(result);
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.initialize();
}]);
