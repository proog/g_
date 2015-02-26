<?php

function listPlatforms($userId) {
    $user = User::findOrFail($userId);
    $platforms = $user->platforms;
    echo $platforms->toJson();
}

function getPlatform($userId, $id) {
    $user = User::findOrFail($userId);
    $platform = $user->findOrFail($id);
    echo $platform->toJson();
}

function addPlatform($userId) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $json = decodeJsonOrFail($app->request->getBody());
    $platform = new Platform($json);
    $platform->user()->associate($user);
    $platform->validOrThrow();
    $platform->save();
    
    $app->response->setStatus(201);
    echo $platform->toJson();
}

function updatePlatform($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $platform = $user->platforms()->findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $platform->fill($json);
    $platform->validOrThrow();
    $platform->save();

    echo $platform->toJson();
}

function deletePlatform($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $platform = $user->platforms()->findOrFail($id);
    $platform->delete();
    $app->response->setStatus(204);
}
