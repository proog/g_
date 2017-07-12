angular.module('games', ['ngAnimate', 'ngRoute', 'ngResource', 'ngCookies', 'lr.upload', 'ui.bootstrap', 'angular-loading-bar'])
.config(['$routeProvider', '$httpProvider', 'cfpLoadingBarProvider', function($routeProvider, $httpProvider, loadingBarProvider) {
    $routeProvider
        .when('/:userId?/:gameId?', {
            templateUrl: 'content.html',
            controller: 'gamesCtrl',
            controllerAs: 'ctrl'
        })
        .otherwise({
            redirectTo: '/'
        });
    $httpProvider.interceptors.push(function() {
        return {
            'request': function(config) {
                var token = window.sessionStorage.getItem('token');

                if(token) {
                    config.headers['Authorization'] = 'Bearer ' + token;
                }

                return config;
            }
        };
    });
    loadingBarProvider.latencyThreshold = 500;
    loadingBarProvider.includeSpinner = false;
}])
.run(['$rootScope', '$uibModalStack', function($rootScope, $modalStack) {
    $rootScope.$on('$routeChangeSuccess', function() {
        $modalStack.dismissAll();
    });
}]);
