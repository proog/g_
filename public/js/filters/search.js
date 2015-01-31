angular.module('games').filter('search', ['gameService', function(gameService) {
    return function(games, query) {
        // global search string
        var search = query.search ? query.search.toLowerCase() : false;
        var filtered = [];

        angular.forEach(games, function(game) {
            // hidden games only match if the authenticated user is the game's owner
            if(game.hidden && (!gameService.authenticated || gameService.authenticated && game.user_id != gameService.authenticatedUser.id))
                return;

            if(search) {
                var title = game.title && game.title.toLowerCase().indexOf(search) >= 0;
                var sort = game.sort_as && game.sort_as.toLowerCase().indexOf(search) >= 0;
                var dev = game.developer && game.developer.toLowerCase().indexOf(search) >= 0;
                var pub = game.publisher && game.publisher.toLowerCase().indexOf(search) >= 0;
                var comm = game.comment && game.comment.toLowerCase().indexOf(search) >= 0;

                if(!title && !sort && !dev && !pub && !comm)
                    return;
            }

            var matches = true;
            angular.forEach(query.parameters, function(param) {
                // if the previous iteration ended with non-match, terminate early
                if(!matches || !param.type || param.value === null || param.value === '')
                    return;

                if(param.type == gameService.FILTER_PLATFORM && game.platform_ids.indexOf(param.value) < 0) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_GENRE && game.genre_ids.indexOf(param.value) < 0) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_TAG && game.tag_ids.indexOf(param.value) < 0) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_YEAR && (game.year == null || game.year != param.value)) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_YEAR_MIN && (game.year == null || game.year < param.value)) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_YEAR_MAX && (game.year == null || game.year > param.value)) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_COMPLETION && game.finished != param.value) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_RATING && (game.rating == null || game.rating != param.value)) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_RATING_MIN && (game.rating == null || game.rating < param.value)) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_RATING_MAX && (game.rating == null || game.rating > param.value)) {
                    matches = false;
                }
                else if(param.type == gameService.FILTER_TITLE && game.title.toLowerCase().indexOf(param.value) < 0) {
                    matches = false;
                }

                if(param.negate)
                    matches = !matches;
            });

            if(matches)
                filtered.push(game);
        });
        return filtered;
    };
}]);
