/* global angular */
(function () {
    "use strict";
    var app = angular.module('user.controllers', []);

    app.controller('userFeedCtrl', [
        '$scope', '$routeParams', '$location', 'userService', 'techStackServices', function ($scope, $routeParams, $location, userService, techStackServices) {
            $scope.currentUserName = $routeParams.userName;
            var sessionLogin = $routeParams.userName.indexOf('s=') !==-1;
            if (sessionLogin) {
                $location.path('/');
            }
            if (!sessionLogin) {
                userService.getUserAvatar($routeParams.userName).then(function (response) {
                    $scope.avatarUrl = response.data.AvatarUrl || '/img/no-profile64.png';
                }, function (response) {
                    $scope.errorMessage = response.statusText;
                });
            }


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

            if (!sessionLogin) {
                userService.getUserCreatedStacks($routeParams.userName).then(function (response) {
                    $scope.techStacks = response.data.TechStacks;
                });
            }
        }
    ]);
})();

