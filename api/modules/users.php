<?php

function listUsers() {
    $users = User::all();
    echo $users->toJson(JSON_NUMERIC_CHECK);
}

function getUser($id) {
    $user = User::findOrFail($id);
    echo $user->toJson(JSON_NUMERIC_CHECK);
}

function updateUser($id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $user->update($json);

    echo $user->toJson(JSON_NUMERIC_CHECK);
}