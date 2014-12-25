angular.module('games', ['ngRoute', 'ngResource', 'ngCookies', 'lr.upload', 'ui.bootstrap', 'angular-loading-bar'])
.config(['$routeProvider', 'cfpLoadingBarProvider', function($routeProvider, loadingBarProvider) {
    $routeProvider
        .when('/:userId?/:gameId?', {
            templateUrl: 'content.html',
            controller: 'gamesCtrl',
            controllerAs: 'ctrl'
        })
        .otherwise({
            redirectTo: '/'
        });
    loadingBarProvider.latencyThreshold = 500;
    loadingBarProvider.includeSpinner = false;
}])
.run(['$rootScope', '$modalStack', function($rootScope, $modalStack) {
    $rootScope.$on('$routeChangeSuccess', function() {
        $modalStack.dismissAll();
    });
}]);
