/*global angular*/
(function () {
    "use strict";

    //PhantomJS tests can skip check
    if (location.origin !== 'http://localhost:9876') {
        //auto redirect urls without #! convention
        if (location.hash && location.hash.indexOf('#!') < 0) {
            if (location.hash.substring(0, 3) == "#s=")
                location.href = '/#!/';
            else
                location.href = location.href.replace('#', '#!');

            location.reload();
            return;
        } else if (location.pathname.length > "/".length) {
            location.href = location.origin + location.search + "#!" + location.pathname;
            return;
        }
    }

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
        'ui.bootstrap',
        'chosen'
    ]).
        config(['$routeProvider', '$httpProvider', '$locationProvider', function ($routeProvider, $httpProvider, $locationProvider) {
            $routeProvider.when('/', { templateUrl: 'partials/home.html', controller: 'homeCtrl' });
            $routeProvider.when('/techs', { templateUrl: 'partials/techs/latest.html', controller: 'latestTechsCtrl' });
            $routeProvider.when('/techs/create', { templateUrl: 'partials/techs/create.html', controller: 'createTechCtrl' });
            $routeProvider.when('/techs/:techId', { templateUrl: 'partials/techs/tech.html', controller: 'techCtrl' });
            $routeProvider.when('/techs/:techId/edit', { templateUrl: 'partials/techs/edit.html', controller: 'editTechCtrl' });
            $routeProvider.when('/stacks', { templateUrl: 'partials/stacks/latest.html', controller: 'latestStacksCtrl' });
            $routeProvider.when('/stacks/create', { templateUrl: 'partials/stacks/create.html', controller: 'createStackCtrl' });
            $routeProvider.when('/stacks/:stackId', { templateUrl: 'partials/stacks/stack.html', controller: 'stackCtrl' });
            $routeProvider.when('/stacks/:stackId/edit', { templateUrl: 'partials/stacks/edit.html', controller: 'editStackCtrl' });
            $routeProvider.when('/:userName', { templateUrl: 'partials/user/feed.html', controller: 'userFeedCtrl' });
            $routeProvider.otherwise({ redirectTo: '/' });
            
            $locationProvider.html5Mode(false).hashPrefix("!");

            $httpProvider.defaults.transformResponse.push(function (responseData) {
                convertDateStringsToDates(responseData);
                return responseData;
            });

            var regexIso8601 = /^(\d{4}|\+\d{6})(?:-(\d{2})(?:-(\d{2})(?:T(\d{2}):(\d{2}):(\d{2})\.(\d{1,})(Z|([\-+])(\d{2}):(\d{2}))?)?)?)?$/;

            //http://aboutcode.net/2013/07/27/json-date-parsing-angularjs.html
            function convertDateStringsToDates(input) {
                // Ignore things that aren't objects.

                if (typeof input !== "object")  {
                    return input;
                }

                for (var key in input) {
                    if (!input.hasOwnProperty(key)) {
                        continue;
                    }
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
        }])
        .directive('disqus', ['$window', function ($window) {
            return {
                restrict: 'E',
                scope: {
                    disqus_shortname: '@disqusShortname',
                    disqus_identifier: '@disqusIdentifier',
                    disqus_title: '@disqusTitle',
                    disqus_url: '@disqusUrl',
                    disqus_category_id: '@disqusCategoryId',
                    disqus_disable_mobile: '@disqusDisableMobile',
                    readyToBind: "@"
                },
                template: '<div id="disqus_thread"></div><a href="http://disqus.com" class="dsq-brlink">comments powered by <span class="logo-disqus">Disqus</span></a>',
                link: function (scope) {
                    scope.$watch("readyToBind", function (isReady) {

                        // If the directive has been called without the 'ready-to-bind' attribute, we
                        // set the default to "true" so that Disqus will be loaded straight away.
                        if (!angular.isDefined(isReady)) {
                            isReady = "true";
                        }
                        if (scope.$eval(isReady)) {
                            // put the config variables into separate global vars so that the Disqus script can see them
                            var relativeUrl = (location.href.split('#!')[1] || '');
                            var categoryId = relativeUrl && relativeUrl.replace('/', '').split('/')[0];
                            $window.disqus_shortname = scope.disqus_shortname || "techstacks";
                            $window.disqus_identifier = scope.disqus_identifier || relativeUrl;
                            $window.disqus_title = scope.disqus_title;
                            $window.disqus_url = scope.disqus_url || location.href;
                            //$window.disqus_category_id = scope.disqus_category_id || categoryId; //category needs to exist
                            $window.disqus_disable_mobile = scope.disqus_disable_mobile;

                            // get the remote Disqus script and insert it into the DOM, but only if it not already loaded (as that will cause warnings)
                            if (!$window.DISQUS) {
                                var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
                                dsq.src = '//' + $window.disqus_shortname + '.disqus.com/embed.js';
                                (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
                            } else {
                                $window.DISQUS.reset({
                                    reload: true,
                                    config: function () {
                                        this.page.identifier = $window.disqus_identifier;
                                        this.page.url = $window.disqus_url;
                                        this.page.title = $window.disqus_title;
                                    }
                                });
                            }
                        }
                    });
                }
            };
        }]);


})();
