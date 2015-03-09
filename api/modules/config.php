<?php

function getConfig() {
    $config = Config::with('defaultUser')->firstOrFail();
    echo $config->toJson();
}

function getSettings() {
    $config = Config::with('defaultUser')->firstOrFail();

    echo json_encode([
        'default_user_id' => $config->defaultUser->id,
        'giant_bomb_api_key' => $config->giant_bomb_api_key
    ]);
}

function updateSettings() {
    $app = Slim\Slim::getInstance();
    $config = Config::query()->firstOrFail();
    $json = decodeJsonOrFail($app->request->getBody());

    // if the supplied password is incorrect, abort
    $user = getLoggedInUserOrFail();
    if($user->password != hash('sha256', $json['old_password'])) {
        throw new NotAuthenticatedException();
    }

    // update config
    /**
     * @var $config Config
     */
    $defaultUser = User::findOrFail($json['default_user_id']);
    $config->defaultUser()->associate($defaultUser);
    $config->giant_bomb_api_key = $json['giant_bomb_api_key'];
    $config->validOrThrow();
    $config->save();

    // change password
    if($json['new_password']) {
        $user->password = hash('sha256', $json['new_password']);
        $user->validOrThrow();
        $user->save();
    }

    // send back updated settings
    getSettings();
}