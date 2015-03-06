<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="">
    <link rel="icon" href="favicon.ico">
    <title>g_</title>
    <link href="https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap.min.css" rel="stylesheet">
</head>
<body>
<div class="container" style="width: 500px; padding-bottom: 50px;">
    <h1 class="page-header">g_ installer</h1>
    <?php if($success): ?>
        <p class="lead">
            Cool beans! g_ has been successfully installed.
        </p>
        <a class="btn btn-primary btn-lg" href="../">
            Open g_
        </a>
    <?php else: ?>
        <p class="lead">
            This page will help you set up your own installation of g_.
        </p>
        <form method="post">
            <h2>Set up database</h2>
            <p>
                g_ needs to create database tables to store its data. Please enter your database configuration here.
                You can always change these options by modifying <code>config/db.json</code> after setup has finished successfully.
            </p>
            <?php if($flash['db']): ?>
                <div class="alert alert-danger">
                    <?php echo $flash['db']; ?>
                </div>
            <?php endif; ?>
            <div class="form-group">
                <label>Database host</label>
                <input type="text" class="form-control" placeholder="localhost" name="dbhost" value="<?= $post['dbhost'] ?>">
            </div>
            <div class="form-group">
                <label>Database port</label>
                <input type="text" class="form-control" placeholder="3306" name="dbport" value="<?= $post['dbport'] ?>">
            </div>
            <div class="form-group">
                <label>Database name</label>
                <input type="text" class="form-control" name="dbname" required value="<?= $post['dbname'] ?>">
                <p class="help-block">
                    The database in which to create tables.
                </p>
            </div>
            <div class="form-group">
                <label>Database username</label>
                <input type="text" class="form-control" name="dbuser" required value="<?= $post['dbuser'] ?>">
                <p class="help-block">
                    g_ needs a user with sufficient permissions for creating tables and inserting, updating and deleting records.
                </p>
            </div>
            <div class="form-group">
                <label>Database password</label>
                <input type="password" class="form-control" name="dbpass" required value="<?= $post['dbpass'] ?>">
            </div>
            <div class="form-group">
                <label>Table prefix</label>
                <input type="text" class="form-control" placeholder="g_" name="dbprefix" value="<?= $post['dbprefix'] ?>">
                <p class="help-block">
                    By default, g_ will create tables named g_games, g_genres, g_users, etc.
                    If you don't want to use the g_ prefix, you can change it here.
                </p>
            </div>
            <h2>Create default user</h2>
            <p>
                g_ needs at least one user to function. Create your login details here.
            </p>
            <?php if($flash['user']): ?>
                <div class="alert alert-danger">
                    <?php echo $flash['user']; ?>
                </div>
            <?php endif; ?>
            <div class="form-group">
                <label>Username</label>
                <input type="text" class="form-control" placeholder="Something like MrGamer23" name="username" required value="<?= $post['username'] ?>">
                <p class="help-block">
                    This username will be visible in the application to identify your collection.
                </p>
            </div>
            <div class="form-group">
                <label>Password</label>
                <input type="password" class="form-control" placeholder="Something that's not 1234" name="password" required value="<?= $post['password'] ?>">
            </div>
            <h2>Other settings</h2>
            <?php if($flash['other']): ?>
                <div class="alert alert-danger">
                    <?php echo $flash['other']; ?>
                </div>
            <?php endif; ?>
            <div class="form-group">
                <label>Giant Bomb API key</label>
                <input type="text" class="form-control" placeholder="Your API key" name="apikey" value="<?= $post['apikey'] ?>">
                <p class="help-block">
                    g_ relies on the Giant Bomb game wiki for assisted game creation, making it possible to automatically retrieve most information when adding games to your collection.<br>
                    If you want to use this feature, you need to create a free Giant Bomb account and <a href="http://www.giantbomb.com/api/" target="_blank">request an API key</a>.
                    Without an API key, assisted game creation will be unavailable.
                </p>
            </div>
            <div class="jumbotron">
                <h2>Install g_</h2>
                <p>
                    That's all folks! Time to do this thing!
                </p>
                <button type="submit" class="btn btn-primary btn-lg center-block">Install</button>
            </div>
        </form>
    <?php endif; ?>
</div>
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.2/jquery.min.js"></script>
</body>
</html>