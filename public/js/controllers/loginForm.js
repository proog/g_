angular.module('games').controller('loginFormCtrl', ['$scope', '$modalInstance', 'gameService', function($scope, $modalInstance, gameService) {
    $scope.loginClick = function() {
        $scope.loginError = false;
        gameService.logIn($scope.loginForm.username, $scope.loginForm.password)
            .then(function() {
                $scope.resetForm();
                $modalInstance.close();
            }, function() {
                $scope.loginError = true;
            });
    };

    $scope.cancelClick = function() {
        $scope.resetForm();
        $modalInstance.dismiss();
    };

    $scope.resetForm = function() {
        $scope.loginError = false;
        $scope.loginForm = {
            username: '',
            password: ''
        };
    };

    $scope.resetForm();
}]);
