/* global angular */
(function () {
    "use strict";
    var app = angular.module('user.controllers', []);

    app.controller('userFeedCtrl', [
        '$scope', '$routeParams', '$location', 'userService', 'techStackServices', function ($scope, $routeParams, $location, userService, techStackServices) {
            $scope.currentUserName = $routeParams.userName;
            //Handle user session logout/login without full reload
            if ($scope.currentUserName.indexOf('s=') === 0
                || $scope.currentUserName.indexOf('f=') === 0) {
                $location.path('/');
                //Fix history
                $location.replace();
                return;
            }
            
            userService.getUserInfo($routeParams.userName).then(function (response) {
                var r = response.data;
                $scope.avatarUrl = r.AvatarUrl;
                $scope.techStacks = r.TechStacks;
                $scope.favoriteTechStacks = r.FavoriteTechStacks;
                $scope.favoriteTechnologies = r.FavoriteTechnologies;
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

