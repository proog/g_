<?php

function logIn() {
    $app = Slim\Slim::getInstance();
    $json = decodeJsonOrFail($app->request->getBody());
    $user = User::query()->where('username', '=', $json['username'])->where('password', '=', hash('sha256', $json['password']))->firstOrFail();
    
    $_SESSION['authenticated'] = true;
    $_SESSION['user_id'] = $user->id;
    echo $user->toJson(JSON_NUMERIC_CHECK);
}

function checkLogin() {
    $user = User::findOrFail($_SESSION['user_id']);
    echo $user->toJson(JSON_NUMERIC_CHECK);
}

function logOut() {
    $_SESSION['authenticated'] = false;
    $_SESSION['user_id'] = false;
}

function authenticate(\Slim\Route $route) {
    $id = $route->getParam('userId');
    if(!isAuthenticated($id))
        throw new NotAuthenticatedException();
}

function isAuthenticated($id) {
    return $_SESSION['authenticated']
        && $id
        && $id == $_SESSION['user_id']
        && User::find($id);
}

class NotAuthenticatedException extends Exception {
    public function __construct($message = 'Not authenticated', $code = 0, $previous = null) {
        parent::__construct($message, $code, $previous);
    }
}
