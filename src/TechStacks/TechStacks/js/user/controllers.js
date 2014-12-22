/* global angular */
(function () {
    "use strict";
    var app = angular.module('user.controllers', []);

    app.controller('userFeedCtrl', [
        '$rootScope', '$scope', '$routeParams', '$location', 'userService', 'techStackServices',
        function ($rootScope, $scope, $routeParams, $location, userService, techStackServices) {
            $scope.currentUserName = $routeParams.userName;
            //Handle user session logout/login without full reload
            if ($scope.currentUserName.indexOf('s=') === 0
                || $scope.currentUserName.indexOf('f=') === 0) {
                $location.path('/');
                //Fix history
                $location.replace();
                return;
            }
            
            //load last page with opacity to increase perceived perf
            if ($rootScope.cachedAvatarUrl) {
                $scope.loading = true;
                $scope.avatarUrl = $rootScope.cachedAvatarUrl;
                $scope.techStacks = $rootScope.cachedTechStacks;
                $scope.favoriteTechStacks = $rootScope.cachedFavoriteTechStacks;
                $scope.favoriteTechnologies = $rootScope.cachedFavoriteTechnologies;
            }
            
            userService.getUserInfo($routeParams.userName).then(function (response) {
                var r = response.data;
                $scope.avatarUrl = r.AvatarUrl;
                $scope.techStacks = r.TechStacks;
                $scope.favoriteTechStacks = r.FavoriteTechStacks;
                $scope.favoriteTechnologies = r.FavoriteTechnologies;
                
                //cache last data
                $rootScope.cachedAvatarUrl = $scope.avatarUrl;
                $rootScope.cachedTechStacks = $scope.techStacks;
                $rootScope.cachedFavoriteTechStacks = $scope.favoriteTechStacks;
                $rootScope.cachedFavoriteTechnologies = $scope.favoriteTechnologies;
                $scope.loading = false;
            });
            
            $scope.deleteStack = function(selectedStack) {
                techStackServices.deleteTechStack(selectedStack).success(function () {
                    for (var i = 0; i < $scope.techStacks.length; i++) {
                        var techStack = $scope.techStacks[i];
                        if (techStack.Id === selectedStack.Id) {
                            $scope.techStacks.splice(i, 1);
                            break;
                        }
                    }
                });
            };
        }
    ]);
})();

