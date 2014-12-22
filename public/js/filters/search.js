angular.module('games').filter('search', function() {
    return function(games, query) {
        if(!query || query == {})
            return games;

        var filtered = [];
        angular.forEach(games, function(game) {
            var matches = true;

            if(query.title != null && game.title.toLowerCase().indexOf(query.title.toLowerCase()) < 0)
                matches = false;
            if(query.year != null && game.year != query.year)
                matches = false;
            if(query.platform != null && game.platform_ids.indexOf(query.platform) < 0)
                matches = false;
            if(query.genre != null && game.genre_ids.indexOf(query.genre) < 0)
                matches = false;
            if(query.tag != null && game.tag_ids.indexOf(query.tag) < 0)
                matches = false;
            if(query.rating != null && game.rating != query.rating)
                matches = false;
            if(query.finished != null && game.finished != query.finished)
                matches = false;

            if(matches)
                filtered.push(game);
        });
        return filtered;
    };
});
