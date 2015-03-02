<?php

function logIn() {
    $app = Slim\Slim::getInstance();
    $json = decodeJsonOrFail($app->request->getBody());
    $user = User::query()->where('username', '=', $json['username'])->where('password', '=', hash('sha256', $json['password']))->firstOrFail();
    
    $_SESSION['authenticated'] = true;
    $_SESSION['user_id'] = $user->id;
    echo $user->toJson();
}

function checkLogin() {
    echo getLoggedInUserOrFail()->toJson();
}

function getLoggedInUserOrFail() {
    return User::findOrFail($_SESSION['user_id']);
}

function logOut() {
    $_SESSION['authenticated'] = false;
    $_SESSION['user_id'] = false;
}

function authenticate(\Slim\Route $route) {
    $params = $route->getParams();
    $id = $params['userId'];

    // general authentication
    if(!isAuthenticated())
        throw new NotAuthenticatedException();

    // authentication for user-specific route, e.g. someone's collection
    if($id && $id != $_SESSION['user_id'])
        throw new NotAuthenticatedException();
}

function isAuthenticated() {
    return $_SESSION['authenticated']
        && $_SESSION['user_id']
        && User::find($_SESSION['user_id']);
}

class NotAuthenticatedException extends Exception {
    public function __construct($message = 'Not authenticated', $code = 0, $previous = null) {
        parent::__construct($message, $code, $previous);
    }
}
