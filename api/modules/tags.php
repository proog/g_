<?php

function listTags($userId) {
    $user = User::findOrFail($userId);
    $tags = $user->tags;
    echo $tags->toJson(JSON_NUMERIC_CHECK);
}

function getTag($userId, $id) {
    $user = User::findOrFail($userId);
    $tag = $user->findOrFail($id);
    echo $tag->toJson(JSON_NUMERIC_CHECK);
}

function addTag($userId) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $json = decodeJsonOrFail($app->request->getBody());
    $tag = new Tag($json);
    $tag->user()->associate($user);
    $tag->save();

    $app->response->setStatus(201);
    echo $tag->toJson(JSON_NUMERIC_CHECK);
}

function updateTag($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $tag = $user->tags()->findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $tag->update($json);

    echo $tag->toJson(JSON_NUMERIC_CHECK);
}

function deleteTag($userId, $id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($userId);
    $tag = $user->tags()->findOrFail($id);
    $tag->delete();
    $app->response->setStatus(204);
}
