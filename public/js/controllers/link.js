angular.module('games').controller('linkCtrl', ['$scope', '$uibModalInstance', 'url', function($scope, $modalInstance, url) {
    $scope.url = url;
}]);
