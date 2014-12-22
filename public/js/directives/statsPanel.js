angular.module('games').directive('statsPanel', function() {
    return {
        templateUrl: 'stats.html',
        scope: {
            panelTitle: '@',
            key: '@',
            value: '@',
            items: '=',
            filterBy: '=',
            orderBy: '=',
            descending: '@',
            onClick: '&'
        },
        link: function($scope, element, attributes) {
            $scope.itemsPerPage = 10;
            $scope.pageChanged = function() {
                $scope.offset = $scope.currentPage * $scope.itemsPerPage - $scope.itemsPerPage;
            };

            $scope.itemClicked = function(item) {
                $scope.onClick({item: item});
            }
        }
    };
});
