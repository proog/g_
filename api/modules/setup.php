<?php
// check if g_ has already been set up, redirect if it has
function checkAlreadyConfigured() {
    $app = Slim\Slim::getInstance();
    if(\Illuminate\Database\Capsule\Manager::schema()->hasTable('users')) {
        $app->redirect('../');
    }
}

function showSetup() {
    checkAlreadyConfigured();

    $app = Slim\Slim::getInstance();
    $app->response->headers->set('Content-Type', 'text/html');
    $app->render('setup.php');
}

function doSetup() {
    checkAlreadyConfigured();

    $app = Slim\Slim::getInstance();
    $post = $app->request->post();
    $username = $post['username'];
    $password = $post['password'];

    if(!$username || !$password) {
        $app->flashNow('user', 'Username or password invalid. Please enter a valid username and password.');
        showSetup();
        return;
    }

    if($password === '1234') {
        $app->flashNow('user', 'I said <em>NOT 1234</em>.');
        showSetup();
        return;
    }

    $settings = decodeJsonOrFail(file_get_contents('../config/db.json'));
    $dsn = 'mysql:dbname='.$settings['database'].';host='.$settings['host'].';port='.$settings['port'];
    $createTables = file_get_contents('../config/create_tables.sql');
    try {
        $con = new PDO($dsn, $settings['username'], $settings['password']);
        if($con->exec($createTables) === false)
            throw new Exception(implode(', ', $con->errorInfo()));

        // create default user
        $user = new User();
        $user->username = $username;
        $user->password = hash('sha256', $password);
        $user->save();

        $config = new Config();
        $config->default_user = $user->id;
        $config->save();
    } catch(PDOException $e) {
        $app->flashNow('db', 'Could not create tables for g_. The most likely reason for this is an incorrect database login or insufficient permissions for the database user.');
        showSetup();
        return;
    }

    $app->redirect('../');
}