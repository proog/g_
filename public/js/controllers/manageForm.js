angular.module('games').controller('manageFormCtrl', ['$scope', '$modalInstance', '$filter', 'Entities', 'items', 'gameService', '$q', function($scope, $modalInstance, $filter, Entities, items, gameService, $q) {
    $scope.initialize = function() {
        $scope.form = { };
        $scope.originalItems = items;
        $scope.items = angular.copy(items);
        $scope.type = Entities.prototype.type;
        $scope.error = null;
        $scope.STATE_UNCHANGED = 1;
        $scope.STATE_ADDED = 2;
        $scope.STATE_UPDATED = 3;
        $scope.STATE_DELETED = 4;

        angular.forEach($scope.items, function(item) {
            item.state = $scope.STATE_UNCHANGED;
        });

        // one time sort to make user experience better
        $scope.items = $filter('orderBy')($scope.items, 'name');
    };

    $scope.addClick = function() {
        var item = new Entities($scope.form);
        item.state = $scope.STATE_ADDED;

        $scope.items.push(item);
        $scope.form = { };
    };

    $scope.deleteClick = function(item) {
        // if new, just remove from list, otherwise mark as deleted
        if(item.state == $scope.STATE_ADDED)
            $scope.items.splice($scope.items.indexOf(item));
        else
            item.state = $scope.STATE_DELETED;
    };

    $scope.nameChange = function(item) {
        // if not new, mark as updated
        if(item.state != $scope.STATE_ADDED)
            item.state = $scope.STATE_UPDATED;

        item.short_name = item.name;
    };

    $scope.shortNameChange = function(item) {
        // if not new, mark as updated
        if(item.state != $scope.STATE_ADDED)
            item.state = $scope.STATE_UPDATED;
    };

    $scope.saveClick = function() {
        var requests = [];

        // handle modified genre/platform/tag items
        angular.forEach($scope.items, function(item) {
            switch (item.state) {
                case $scope.STATE_ADDED:
                    // add new item to original array
                    requests.push(item.$save({userId: gameService.authenticatedUser.id}, function(data) {
                        $scope.originalItems.push(data);
                    }));
                    break;
                case $scope.STATE_UPDATED:
                    requests.push(item.$update(function(data) {
                        // copy updated data back into original item
                        angular.forEach($scope.originalItems, function(originalItem) {
                            if(originalItem.id == item.id)
                                angular.copy(data, originalItem);
                        });
                    }));
                    break;
                case $scope.STATE_DELETED:
                    requests.push(item.$delete(function() {
                        // remove item from original array
                        var pos = $scope.originalItems.reduce(function(ret, originalItem, index) {
                            return originalItem.id == item.id ? index : ret;
                        }, -1);

                        if(pos > -1)
                            $scope.originalItems.splice(pos, 1);

                        // remove deleted item id from games that reference it
                        var ids = Entities.prototype.ids;
                        angular.forEach(gameService.games, function(game) {
                            var pos = game[ids].reduce(function(ret, id, index) {
                                return item.id == id ? index : ret;
                            }, -1);

                            if(pos > -1)
                                game[ids].splice(pos, 1);
                        });
                    }));
                    break;
            }
        });

        $q.all(requests).then(function() {
            $modalInstance.close();
        }, function() {
            $scope.error = 'Could not save all items. Please check that all fields are filled out.';
        });
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

    $scope.closeAlert = function() {
        $scope.error = null;
    };

    $scope.initialize();
}]);
