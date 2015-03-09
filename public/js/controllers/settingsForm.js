angular.module('games').controller('settingsFormCtrl', ['$scope', '$modalInstance', 'gameService', 'Settings', function($scope, $modalInstance, gameService, Settings) {
    $scope.initialize = function() {
        $scope.gameService = gameService;
        $scope.model = Settings.get();
    };

    $scope.saveClick = function() {
        $scope.model.$update().then(function() {
            // refresh config to reflect changes to e.g. assisted game creation
            gameService.refreshConfig().then(function() {
                $modalInstance.close();
            });
        }, function() {
            $scope.error = true;
        });
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.closeAlert = function() {
        $scope.error = false;
    };

    $scope.initialize();
}]);
