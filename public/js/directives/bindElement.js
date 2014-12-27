angular.module('games').directive('bindElement', function() {
    return {
        restrict: 'A',
        scope: {
            bindElement: '='
        },
        link: function(scope, elem) {
            scope.bindElement = elem;
        }
    };
});
