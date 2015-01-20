<!DOCTYPE html>
<html lang="en" ng-app="games">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="">
    <link rel="icon" href="favicon.ico">
    <title>g_</title>
    <link href="../css/bootstrap.min.css" rel="stylesheet">
</head>
<body ng-controller="setupCtrl">
<nav class="navbar navbar-default navbar-static-top">
    <div class="container" style="width: 500px;">
        <div class="navbar-header">
            <a class="navbar-brand">g_ setup</a>
        </div>
    </div>
</nav>
<div class="container" style="width: 500px;">
    <h1 class="page-header">g_ setup</h1>
    <p class="lead">
        This page will help you set up your own installation of g_, which will create tables named g_games, g_platforms, g_users, etc. to store its data.<br>
        Please note that if any of these tables already exist, <em>they will be dropped</em>.
    </p>
    <?php if($flash['db']): ?>
        <div class="alert alert-danger">
            <?php echo $flash['db']; ?>
        </div>
    <?php endif; ?>
    <form action="setup" method="post" name="form">
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
            <input type="text" class="form-control" placeholder="Something like MRGAMER22" name="username" ng-model="username" required maxlength="20">
        </div>
        <div class="form-group">
            <label>Password</label>
            <input type="password" class="form-control" placeholder="Something that's not 1234" name="password" ng-model="password" required>
        </div>
        <button type="submit" class="btn btn-primary" ng-disabled="form.$invalid">Continue</button>
    </form>
</div>
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.2/jquery.min.js"></script>
<script src="//ajax.googleapis.com/ajax/libs/angularjs/1.3.5/angular.min.js"></script>
<script>
    angular.module('games', []).controller('setupCtrl', ['$scope', function ($scope) {

    }]);
</script>
</body>
</html>