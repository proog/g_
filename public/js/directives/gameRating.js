angular.module('games').directive('gameRating', function() {
    return {
        templateUrl: 'rating.html',
        scope: {
            game: '='
        }
    };
});
