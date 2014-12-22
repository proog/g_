<?php

function listGenres($userId) {
    $user = User::findOrFail($userId);
    $genres = $user->genres;
    echo $genres->toJson(JSON_NUMERIC_CHECK);
}

function getGenre($userId, $id) {
    $user = User::findOrFail($userId);
    $genre = $user->findOrFail($id);
    echo $genre->toJson(JSON_NUMERIC_CHECK);
}

function addGenre($userId) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $json = decodeJsonOrFail($app->request->getBody());
    $genre = new Genre($json);
    $genre->user()->associate($user);
    $genre->save();

    $app->response->setStatus(201);
    echo $genre->toJson(JSON_NUMERIC_CHECK);
}

function updateGenre($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $genre = $user->genres()->findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $genre->update($json);

    echo $genre->toJson(JSON_NUMERIC_CHECK);
}

function deleteGenre($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $genre = $user->genres()->findOrFail($id);
    $genre->delete();
    $app->response->setStatus(204);
}
