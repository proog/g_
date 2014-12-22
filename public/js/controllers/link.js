angular.module('games').controller('linkCtrl', ['$scope', '$modalInstance', 'url', function($scope, $modalInstance, url) {
    $scope.url = url;
}]);
