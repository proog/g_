<?php
require '../config/require.php';

session_cache_limiter(false);
session_start();

// load db settings
$settings = decodeJsonOrFail(file_get_contents('../config/db.json'));

// set up eloquent
use Illuminate\Database\Capsule\Manager as Capsule;
$capsule = new Capsule();
$capsule->addConnection($settings);
$capsule->setAsGlobal();
$capsule->bootEloquent();

// set up slim
$app = new Slim\Slim(['debug' => false]);
$app->response->headers->set('Content-Type', 'application/json');

// public api
$app->get('/users/:userId/games', 'listGames');
$app->get('/users/:userId/games/:id', 'getGame');

$app->get('/users/:userId/genres', 'listGenres');
$app->get('/users/:userId/genres/:id', 'getGenre');

$app->get('/users/:userId/platforms', 'listPlatforms');
$app->get('/users/:userId/platforms/:id', 'getPlatform');

$app->get('/users/:userId/tags', 'listTags');
$app->get('/users/:userId/tags/:id', 'getTag');

$app->get('/users', 'listUsers');
$app->get('/users/:id', 'getUser');
$app->get('/config', 'getConfig');

$app->post('/login', 'logIn');
$app->post('/logout', 'logOut');
$app->get('/login', 'checkLogin');
$app->get('/setup', 'showSetup');
$app->post('/setup', 'doSetup');

// restricted api
$app->post('/users/:userId/games', 'authenticate', 'addGame');
$app->put('/users/:userId/games/:id', 'authenticate', 'updateGame');
$app->delete('/users/:userId/games/:id', 'authenticate', 'deleteGame');
$app->post('/users/:userId/games/:id/image', 'authenticate', 'uploadImage');

$app->post('/users/:userId/genres', 'authenticate', 'addGenre');
$app->put('/users/:userId/genres/:id', 'authenticate', 'updateGenre');
$app->delete('/users/:userId/genres/:id', 'authenticate', 'deleteGenre');

$app->post('/users/:userId/platforms', 'authenticate', 'addPlatform');
$app->put('/users/:userId/platforms/:id', 'authenticate', 'updatePlatform');
$app->delete('/users/:userId/platforms/:id', 'authenticate', 'deletePlatform');

$app->post('/users/:userId/tags', 'authenticate', 'addTag');
$app->put('/users/:userId/tags/:id', 'authenticate', 'updateTag');
$app->delete('/users/:userId/tags/:id', 'authenticate', 'deleteTag');

$app->put('/users/:userId', 'authenticate', 'updateUser');

$app->notFound('showNotFound');
$app->error('showError');
$app->run();
