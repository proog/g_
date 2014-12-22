angular.module('games').directive('fileUpload', function() {
    return {
        restrict: 'A',
        scope: {
            inputElement: '='
        },
        link: function(scope, elem) {
            scope.inputElement = elem;
        }
    };
});
