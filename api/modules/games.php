<?php

function listGames($userId) {
    $user = User::findOrFail($userId);
    $games = $user->games()->with('genres', 'platforms', 'tags')->get();
    echo $games->toJson(JSON_NUMERIC_CHECK);
}

function getGame($userId, $id) {
    $user = User::findOrFail($userId);
    $game = $user->games()->with('genres', 'platforms', 'tags')->findOrFail($id);
    echo $game->toJson(JSON_NUMERIC_CHECK);
}

function addGame($userId) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $metadata = decodeJsonOrFail($app->request->getBody());
    $game = new Game($metadata);
    $game->user()->associate($user);
    $game->save();
    
    foreach($metadata['genre_ids'] as $gid) {
        $genre = $user->genres()->findOrFail($gid);
        $game->genres()->attach($genre);
    }
    
    foreach($metadata['platform_ids'] as $pid) {
        $platform = $user->platforms()->findOrFail($pid);
        $game->platforms()->attach($platform);
    }
    
    foreach($metadata['tag_ids'] as $tid) {
        $tag = $user->tags()->findOrFail($tid);
        $game->tags()->attach($tag);
    }
    
    echo $game->toJson(JSON_NUMERIC_CHECK);
}

function updateGame($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $game = $user->games()->findOrFail($id);
    $metadata = decodeJsonOrFail($app->request->getBody());
    
    $game->update($metadata);
    $game->genres()->detach();
    $game->platforms()->detach();
    $game->tags()->detach();
    
    foreach($metadata['genre_ids'] as $gid) {
        $genre = $user->genres()->findOrFail($gid);
        $game->genres()->attach($genre);
    }
    
    foreach($metadata['platform_ids'] as $pid) {
        $platform = $user->platforms()->findOrFail($pid);
        $game->platforms()->attach($platform);
    }
    
    foreach($metadata['tag_ids'] as $tid) {
        $tag = $user->tags()->findOrFail($tid);
        $game->tags()->attach($tag);
    }
    
    echo $game->toJson(JSON_NUMERIC_CHECK);
}

function deleteGame($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $game = $user->games()->findOrFail($id);
    $game->deleteImage();
    $game->delete();
    $app->response->setStatus(204);
}

function uploadImage($userId, $id) {
    $user = User::findOrFail($userId);
    $game = $user->games()->findOrFail($id);
    $image = $_FILES['image'];
    $game->addImage($image);
    echo $game->toJson(JSON_NUMERIC_CHECK);
}

function listSuggestions($userId) {
    $user = User::findOrFail($userId);
    $applicableGames = $user->games()
        ->with('genres', 'platforms', 'tags')
        ->where('finished', 0)
        ->where('rating', null)
        ->where('playtime', null)
        ->where('queue_position', null)
        ->where('currently_playing', false)
        ->get();
    $topGames = $user->games()
        ->with('genres', 'platforms', 'tags')
        ->orderBy('rating', 'desc')
        ->orderBy('playtime', 'desc')
        ->take($user->games()->count() / 10) // top 10% of all games
        ->get();

    // collect genres from top 10 and the occurrences of each
    $topGenres = [];
    foreach($topGames as $topGame) {
        foreach($topGame->genres as $genre) {
            if(!$topGenres[$genre->id])
                $topGenres[$genre->id] = 0;
            $topGenres[$genre->id]++;
        }
    }

    $suggestions = [];
    foreach($applicableGames as $game) {
        $score = 0;

        // how similar are the genres to top 10's genres?
        foreach($game->genres as $genre) {
            if(array_key_exists($genre->id, $topGenres))
                $score += $topGenres[$genre->id];
        }

        // how similar is the title on average to top 10
        $titleSimilarity = 0;
        foreach($topGames as $topGame) {
            $pct = 0;
            similar_text(strtolower($game->title), strtolower($topGame->title), $pct);
            $titleSimilarity += $pct;
        }
        $score += (int)($titleSimilarity/count($topGames));

        // 30% chance of getting score boosted by 33%
        $rand = rand(0, 10);
        if($rand > 7)
            $score += (int) ($score/3);

        $suggestions[] = [
            'game_id' => $game->id,
            'score' => $score
        ];
    }

    usort($suggestions, function($a, $b) {
        if($a['score'] < $b['score'])
            return 1;
        if($a['score'] > $b['score'])
            return -1;
        return 0;
    });
    echo json_encode(array_slice($suggestions, 0, 5), JSON_NUMERIC_CHECK);
}
