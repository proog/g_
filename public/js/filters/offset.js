angular.module('games').filter('offset', function() {
    return function(items, start) {
        return items.slice(start);
    };
});
