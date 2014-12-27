angular.module('games').controller('queueFormCtrl', ['$scope', '$modalInstance', '$filter', 'games', function($scope, $modalInstance, $filter, games) {
    $scope.initialize = function() {
        $scope.games = angular.copy(games);
        $scope.rewriteQueue();
    };

    $scope.rewriteQueue = function() {
        // just in case of duplicate or missing positions, set all queue positions starting from 0 at the first item
        $scope.games = $filter('orderBy')($scope.games, ['queue_position', 'sort_as']);
        for(var i = 0; i < $scope.games.length; i++)
            $scope.games[i].queue_position = i;
    };

    $scope.getMaxPosition = function() {
        return $scope.games.reduce(function(ret, item) {
            return item.queue_position == null ? ret : ret + 1;
        }, 0) - 1;
    };

    $scope.move = function(game, position) {
        angular.forEach($scope.games, function(item) {
            if(item == game || item.queue_position == null)
                return;

            if(item.queue_position > game.queue_position)
                item.queue_position--;

            if(item.queue_position >= position)
                item.queue_position++;
        });

        game.queue_position = position;
    };

    $scope.moveUpClick = function(game) {
        if(game.queue_position == 0)
            $scope.move(game, $scope.getMaxPosition());
        else
            $scope.move(game, game.queue_position - 1);
    };

    $scope.moveDownClick = function(game) {
        if(game.queue_position == $scope.getMaxPosition())
            $scope.move(game, 0);
        else
            $scope.move(game, game.queue_position + 1);
    };

    $scope.removeClick = function(game) {
        // move following items one up
        angular.forEach($scope.games, function(item) {
            if(item != game && item.queue_position >= game.queue_position)
                item.queue_position--;
        });

        game.queue_position = null;
    };

    $scope.saveClick = function() {
        $scope.rewriteQueue();
        $modalInstance.close($scope.games);
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.initialize();
}]);
