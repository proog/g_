angular.module('games').controller('globalCtrl', ['$scope', '$cookies', function($scope, $cookies) {
    $scope.globalOptions = {
        darkStyle: true
    };

    $scope.changeStyle = function() {
        $scope.globalOptions.darkStyle = !$scope.globalOptions.darkStyle;
        $cookies.put('darkStyle', $scope.globalOptions.darkStyle ? 1 : 0);
    };

    this.init = function() {
        $scope.globalOptions.darkStyle = $cookies.get('darkStyle') == 1;
    };
    this.init();
}]);