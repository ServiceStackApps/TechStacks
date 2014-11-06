angular.module('stacks', ['stacks.controllers', 'stacks.services', 'stacks.filters']);
angular.module('techs', ['techs.controllers','tech.services']);
angular.module('home', ['home.controllers']);
angular.module('user', ['user.controllers', 'user.services']);

// Declare app level module which depends on filters, and services
angular.module('techStackApp', [
  'ngRoute',
  'home',
  'techs',
  'stacks',
  'user',
  'navigation.controllers',
  'ui.bootstrap'
]).
config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
    $routeProvider.when('/', { templateUrl: 'partials/home.html', controller: 'homeCtrl' });
    $routeProvider.when('/i/techs', { templateUrl: 'partials/techs/latest.html', controller: 'latestTechsCtrl' });
    $routeProvider.when('/i/techs/create', { templateUrl: 'partials/techs/create.html', controller: 'createTechCtrl' });
    $routeProvider.when('/i/techs/:techId', { templateUrl: 'partials/techs/tech.html', controller: 'techCtrl' });
    $routeProvider.when('/i/techs/:techId/edit', { templateUrl: 'partials/techs/edit.html', controller: 'editTechCtrl' });
    $routeProvider.when('/i/stacks', { templateUrl: 'partials/stacks/latest.html', controller: 'latestStacksCtrl' });
    $routeProvider.when('/i/stacks/create', { templateUrl: 'partials/stacks/create.html', controller: 'createStackCtrl' });
    $routeProvider.when('/i/stacks/:stackId', { templateUrl: 'partials/stacks/stack.html', controller: 'stackCtrl' });
    $routeProvider.when('/i/stacks/:stackId/edit', { templateUrl: 'partials/stacks/edit.html', controller: 'editStackCtrl' });
    $routeProvider.when('/:userName', { templateUrl: 'partials/user/feed.html', controller: 'userFeedCtrl' });
    $routeProvider.otherwise({ redirectTo: '/' });

    $httpProvider.defaults.transformResponse.push(function (responseData) {
        convertDateStringsToDates(responseData);
        return responseData;
    });

    var regexIso8601 = /^(\d{4}|\+\d{6})(?:-(\d{2})(?:-(\d{2})(?:T(\d{2}):(\d{2}):(\d{2})\.(\d{1,})(Z|([\-+])(\d{2}):(\d{2}))?)?)?)?$/;

    //http://aboutcode.net/2013/07/27/json-date-parsing-angularjs.html
    function convertDateStringsToDates(input) {
        // Ignore things that aren't objects.
        
        if (typeof input !== "object") return input;

        for (var key in input) {
            if (!input.hasOwnProperty(key)) continue;

            var value = input[key];
            var match;
            // Check for string properties which look like dates.
            if (typeof value === "string" && (match = value.match(regexIso8601))) {
                var milliseconds = Date.parse(match[0]);
                if (!isNaN(milliseconds)) {
                    input[key] = new Date(milliseconds);
                }
            } else if (typeof value === "object") {
                // Recurse into object
                convertDateStringsToDates(value);
            }
        }
    }
}]);