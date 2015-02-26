<?php

function listUsers() {
    $users = User::all();
    echo $users->toJson();
}

function getUser($id) {
    $user = User::findOrFail($id);
    echo $user->toJson();
}

function updateUser($id) {
    $app = Slim\Slim::getInstance();
    $user = User::findOrFail($id);
    $json = decodeJsonOrFail($app->request->getBody());
    $user->fill($json);
    $user->validOrThrow();
    $user->save();

    echo $user->toJson();
}