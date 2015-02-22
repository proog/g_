<?php
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Capsule\Manager as Capsule;

function renderSetup($success = false) {
    $app = \Slim\Slim::getInstance();
    $app->response->headers->set('Content-Type', 'text/html');
    $app->render('setup.php', ['post' => $app->request->post(), 'success' => $success]);
    $app->stop();
}

function showSetup() {
    if(isConfigured())
        renderSetup(true);

    renderSetup();
}

function doSetup() {
    if(isConfigured())
        renderSetup(true);

    $app = \Slim\Slim::getInstance();
    $post = $app->request->post();
    $username = $post['username'];
    $password = $post['password'];
    $dbname = $post['dbname'];
    $dbuser = $post['dbuser'];
    $dbpass = $post['dbpass'];
    $dbprefix = $post['dbprefix'] ?: 'g_';
    $dbhost = $post['dbhost'] ?: 'localhost';
    $dbport = $post['dbport'] ?: '3306';

    if(!$dbname || !$dbuser || !$dbpass) {
        $app->flashNow('db', 'You must specify a database name, user, and password.');
        renderSetup();
    }

    if(!$username || !$password) {
        $app->flashNow('user', 'Username or password invalid. Please enter a valid username and password.');
        renderSetup();
    }

    if($password === '1234') {
        $app->flashNow('user', 'I said <em>NOT 1234</em>.');
        renderSetup();
    }

    $settings = [
        'host' => $dbhost,
        'port' => $dbport,
        'database' => $dbname,
        'username' => $dbuser,
        'password' => $dbpass,
        'prefix' => $dbprefix,
        'driver' => 'mysql',
        'collation' => 'utf8_general_ci',
        'charset' => 'utf8'
    ];

    // test db connection
    try {
        $capsule = new Capsule();
        $capsule->addConnection($settings);
        $capsule->setAsGlobal();
        $capsule->connection();
        $capsule->bootEloquent();
    } catch(Exception $e) {
        $app->flashNow('db', 'Could not connect to the database. Suggestion: Make sure the database name, username and password is correct.');
        renderSetup();
    }

    // create tables
    try {
        createTables();
    } catch(Exception $e) {
        $app->flashNow('db', 'Could not create tables for g_. Suggestion: Make sure the database user has permissions to create tables.');
        renderSetup();
    }

    // create default user
    try {
        $user = new User();
        $user->username = $username;
        $user->password = hash('sha256', $password);
        $user->save();

        $config = new Config();
        $config->default_user = $user->id;
        $config->save();
    } catch(Exception $e) {
        $app->flashNow('db', 'Could not create the default user. Suggestion: Make sure the database user has permissions to create records.');
        renderSetup();
    }

    // write config file
    $saved = file_put_contents('../config/db.json', json_encode($settings));
    if(!$saved) {
        $app->flashNow('db', 'Could not create configuration file. Suggestion: Make sure that PHP has permissions to write to the file system.');
        renderSetup();
    }

    // successfully set up
    renderSetup(true);
}

function createTables() {
    $schema = Illuminate\Database\Capsule\Manager::schema();

    $schema->create('users', function (Blueprint $table) {
        $table->engine = 'InnoDB';
        $table->increments('id');
        $table->string('username');
        $table->string('password');
        $table->integer('view')->nullable()->default(null);
        $table->timestamps();
    });

    $schema->create('config', function (Blueprint $table) {
        $table->engine = 'InnoDB';
        $table->increments('id');
        $table->integer('default_user')->unsigned();
        $table->foreign('default_user')->references('id')->on('users');
    });

    $schema->create('games', function (Blueprint $table) {
        $table->engine = 'InnoDB';
        $table->increments('id');
        $table->string('title');
        $table->string('developer')->nullable()->default(null);
        $table->string('publisher')->nullable()->default(null);
        $table->integer('year')->nullable()->default(null);
        $table->string('image')->nullable()->default(null);
        $table->tinyInteger('finished')->default(Game::NOT_FINISHED);
        $table->text('comment')->nullable()->default(null);
        $table->string('sort_as')->nullable()->default(null);
        $table->text('private_comment')->nullable()->default(null);
        $table->time('playtime')->nullable()->default(null);
        $table->tinyInteger('rating')->nullable()->default(null);
        $table->boolean('currently_playing')->default(false);
        $table->integer('queue_position')->nullable()->default(null);
        $table->boolean('hidden')->default(false);
        $table->integer('wishlist_position')->nullable()->default(null);
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
            $table->engine = 'InnoDB';
            $table->increments('id');
            $table->integer('user_id')->unsigned();
            $table->foreign('user_id')->references('id')->on('users')->onDelete('cascade');
            $table->string('name');
            $table->string('short_name');
            $table->timestamps();
        });

        $schema->create($entity['junctionTable'], function (Blueprint $table) use ($entity) {
            $table->engine = 'InnoDB';
            $table->integer('game_id')->unsigned();
            $table->foreign('game_id')->references('id')->on('games')->onDelete('cascade');
            $table->integer($entity['junctionColumn'])->unsigned();
            $table->foreign($entity['junctionColumn'])->references('id')->on($entity['table'])->onDelete('cascade');
        });
    }
}