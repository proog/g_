angular.module('games').controller('loginFormCtrl', ['$scope', '$modalInstance', '$http', 'loginUrl', function($scope, $modalInstance, $http, loginUrl) {
    $scope.loginClick = function() {
        $scope.loginError = false;
        $http.post($scope.loginUrl, $scope.loginForm)
            .success(function(data) {
                $scope.resetForm();
                $modalInstance.close(data);
            }).error(function() {
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

    $scope.loginUrl = loginUrl;
    $scope.resetForm();
}]);
