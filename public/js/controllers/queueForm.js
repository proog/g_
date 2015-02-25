angular.module('games').controller('queueFormCtrl', ['$scope', '$modalInstance', '$filter', 'games', 'property', 'gameService', '$q', function($scope, $modalInstance, $filter, games, property, gameService, $q) {
    $scope.initialize = function() {
        $scope.games = angular.copy(games);
        $scope.property = property;
        $scope.filterObject = {};
        $scope.filterObject[property] = '!!';
        $scope.rewriteQueue();
    };

    $scope.rewriteQueue = function() {
        // just in case of duplicate or missing positions, set all positions starting from 0 at the first item, skipping nulled positions
        $scope.games = $filter('orderBy')($scope.games, [$scope.property, 'sort_as']);

        var position = 0;
        angular.forEach($scope.games, function(game) {
            if(game[property] != null)
                game[property] = position++;
        });
    };

    $scope.getMaxPosition = function() {
        return $scope.games.reduce(function(ret, item) {
            return item[property] == null ? ret : ret + 1;
        }, 0) - 1;
    };

    $scope.move = function(game, position) {
        angular.forEach($scope.games, function(item) {
            if(item == game || item[property] == null)
                return;

            if(item[property] > game[property])
                item[property]--;

            if(item[property] >= position)
                item[property]++;
        });

        game[property] = position;
    };

    $scope.moveUpClick = function(game) {
        if(game[property] == 0)
            $scope.move(game, $scope.getMaxPosition());
        else
            $scope.move(game, game[property] - 1);
    };

    $scope.moveDownClick = function(game) {
        if(game[property] == $scope.getMaxPosition())
            $scope.move(game, 0);
        else
            $scope.move(game, game[property] + 1);
    };

    $scope.removeClick = function(game) {
        // move subsequent items one position up
        angular.forEach($scope.games, function(item) {
            if(item != game && item[property] != null && item[property] >= game[property])
                item[property]--;
        });

        game[property] = null;
    };

    $scope.saveClick = function() {
        $scope.rewriteQueue();

        // update original games
        var promises = [];
        angular.forEach($scope.games, function(game) {
            angular.forEach(gameService.games, function(originalGame) {
                if(originalGame.id == game.id) {
                    originalGame[property] = game[property];
                    promises.push(originalGame.$update());
                }
            });
        });

        $q.all(promises).then(function() {
            $modalInstance.close();
        });
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.initialize();
}]);
