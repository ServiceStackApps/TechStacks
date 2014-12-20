/* global angular */
(function () {
    "use strict";
    var app = angular.module('user.controllers', []);

    app.controller('userFeedCtrl', [
        '$scope', '$routeParams', '$location', 'userService', 'techStackServices', function ($scope, $routeParams, $location, userService, techStackServices) {
            $scope.currentUserName = $routeParams.userName;
            //Handle user session logout/login without full reload
            if ($scope.currentUserName.indexOf('s=') === 0) {
                $location.path('/');
                //Fix history
                $location.replace();
                return;
            }
            userService.getUserAvatar($routeParams.userName).then(function (response) {
                $scope.avatarUrl = response.data.AvatarUrl || '/img/no-profile64.png';
            }, function (response) {
                $scope.errorMessage = response.statusText;
            });

            userService.getUserStacks($routeParams.userName).then(function (response) {
                $scope.techStacks = response.data.TechStacks;
                $scope.favoriteTechStacks = response.data.FavoriteTechStacks;
                $scope.favoriteTechnologies = response.data.FavoriteTechnologies;
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

