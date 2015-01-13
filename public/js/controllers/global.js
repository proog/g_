angular.module('games').controller('globalCtrl', ['$scope', '$cookies', function($scope, $cookies) {
    $scope.globalOptions = {
        darkStyle: true
    };

    $scope.changeStyle = function() {
        $scope.globalOptions.darkStyle = !$scope.globalOptions.darkStyle;
        $cookies.darkStyle = $scope.globalOptions.darkStyle ? 1 : 0;
    };

    this.init = function() {
        $scope.globalOptions.darkStyle = ($cookies.darkStyle == 1);
    };
    this.init();
}]);