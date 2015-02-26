<?php

function listTags($userId) {
    $user = User::findOrFail($userId);
    $tags = $user->tags;
    echo $tags->toJson();
}

function getTag($userId, $id) {
    $user = User::findOrFail($userId);
    $tag = $user->findOrFail($id);
    echo $tag->toJson();
}

function addTag($userId) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $json = decodeJsonOrFail($app->request->getBody());
    $tag = new Tag($json);
    $tag->user()->associate($user);
    $tag->validOrThrow();
    $tag->save();

    $app->response->setStatus(201);
    echo $tag->toJson();
}

function updateTag($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $tag = $user->tags()->findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $tag->fill($json);
    $tag->validOrThrow();
    $tag->save();

    echo $tag->toJson();
}

function deleteTag($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $tag = $user->tags()->findOrFail($id);
    $tag->delete();
    $app->response->setStatus(204);
}
