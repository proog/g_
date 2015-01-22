<?php
use Illuminate\Database\Schema\Blueprint;

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

    try {
        createTables();
    } catch(Exception $e) {
        $app->flashNow('db', 'Could not create tables for g_. The most likely reason for this is an incorrect database login or insufficient permissions for the database user.');
        $app = Slim\Slim::getInstance();
        $app->response->headers->set('Content-Type', 'text/html');
        $app->render('setup.php');
        return;
    }

    // create default user
    $user = new User();
    $user->username = $username;
    $user->password = hash('sha256', $password);
    $user->save();

    $config = new Config();
    $config->default_user = $user->id;
    $config->save();

    $app->redirect('../');
}

function createTables()
{
    $schema = Illuminate\Database\Capsule\Manager::schema();

    $schema->create('users', function (Blueprint $table) {
        $table->increments('id');
        $table->string('username');
        $table->string('password');
        $table->integer('view')->nullable()->default(null);
        $table->timestamps();
    });

    $schema->create('config', function (Blueprint $table) {
        $table->increments('id');
        $table->integer('default_user')->unsigned();
        $table->foreign('default_user')->references('id')->on('users');
    });

    $schema->create('games', function (Blueprint $table) {
        $table->increments('id');
        $table->string('title');
        $table->string('developer')->nullable()->default(null);
        $table->string('publisher')->nullable()->default(null);
        $table->integer('year')->nullable()->default(null);
        $table->string('image')->nullable()->default(null);
        $table->tinyInteger('finished')->default(0);
        $table->text('comment')->nullable()->default(null);
        $table->string('sort_as')->nullable()->default(null);
        $table->text('private_comment')->nullable()->default(null);
        $table->time('playtime')->nullable()->default(null);
        $table->tinyInteger('rating')->nullable()->default(null);
        $table->boolean('currently_playing')->default(false);
        $table->integer('queue_position')->nullable()->default(null);
        $table->integer('user_id')->unsigned();
        $table->foreign('user_id')->references('id')->on('users')->onDelete('cascade');
        $table->timestamps();
    });

    $entities = [
        [
            'table' => 'genres',
            'junctionTable' => 'game_genre',
            'junctionColumn' => 'genre_id'
        ], [
            'table' => 'platforms',
            'junctionTable' => 'game_platform',
            'junctionColumn' => 'platform_id'
        ], [
            'table' => 'tags',
            'junctionTable' => 'game_tag',
            'junctionColumn' => 'tag_id'
        ]
    ];

    foreach ($entities as $entity) {
        $schema->create($entity['table'], function (Blueprint $table) {
            $table->increments('id');
            $table->integer('user_id')->unsigned();
            $table->foreign('user_id')->references('id')->on('users')->onDelete('cascade');
            $table->string('name');
            $table->string('short_name');
            $table->timestamps();
        });

        $schema->create($entity['junctionTable'], function (Blueprint $table) use ($entity) {
            $table->integer('game_id')->unsigned();
            $table->foreign('game_id')->references('id')->on('games')->onDelete('cascade');
            $table->integer($entity['junctionColumn'])->unsigned();
            $table->foreign($entity['junctionColumn'])->references('id')->on('games')->onDelete('cascade');
        });
    }
}