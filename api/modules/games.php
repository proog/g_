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
