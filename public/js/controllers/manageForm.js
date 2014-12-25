angular.module('games').controller('manageFormCtrl', ['$scope', '$modalInstance', 'Entities', 'items', function($scope, $modalInstance, Entities, items) {
    $scope.initialize = function() {
        $scope.form = { };
        $scope.items = angular.copy(items);
        $scope.type = Entities.prototype.type;

        angular.forEach($scope.items, function(item) {
            item.added = false;
            item.deleted = false;
            item.updated = false;
        });
    };

    $scope.addClick = function() {
        var item = new Entities($scope.form);

        item.added = true;
        item.updated = false;
        item.deleted = false;

        $scope.items.push(item);
        $scope.form = { };
    };

    $scope.deleteClick = function(item) {
        item.added = false;
        item.deleted = true;
        item.updated = false;
    };

    $scope.nameChange = function(item) {
        if(!item.added) {
            item.updated = true;
        }

        item.short_name = item.name;
    };

    $scope.shortNameChange = function(item) {
        if(!item.added) {
            item.updated = true;
        }
    };

    $scope.saveClick = function() {
        var result = $scope.items;
        $modalInstance.close(result);
    };

    $scope.cancelClick = function() {
        $modalInstance.dismiss();
    };

    $scope.canSave = function() {
        for(var i = 0; i < $scope.items.length; i++) {
            var item = $scope.items[i];
            if(!item.name || !item.short_name)
                return false;
        }
        return true;
    };

    $scope.initialize();
}]);
