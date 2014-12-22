<?php

function listPlatforms($userId) {
    $user = User::findOrFail($userId);
    $platforms = $user->platforms;
    echo $platforms->toJson(JSON_NUMERIC_CHECK);
}

function getPlatform($userId, $id) {
    $user = User::findOrFail($userId);
    $platform = $user->findOrFail($id);
    echo $platform->toJson(JSON_NUMERIC_CHECK);
}

function addPlatform($userId) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $json = decodeJsonOrFail($app->request->getBody());
    $platform = new Platform($json);
    $platform->user()->associate($user);
    $platform->save();
    
    $app->response->setStatus(201);
    echo $platform->toJson(JSON_NUMERIC_CHECK);
}

function updatePlatform($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $platform = $user->platforms()->findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $platform->update($json);

    echo $platform->toJson(JSON_NUMERIC_CHECK);
}

function deletePlatform($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $platform = $user->platforms()->findOrFail($id);
    $platform->delete();
    $app->response->setStatus(204);
}
