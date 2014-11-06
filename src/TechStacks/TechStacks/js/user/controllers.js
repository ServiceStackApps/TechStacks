var app = angular.module('user.controllers', []);

app.controller('userFeedCtrl', [
    '$scope', '$routeParams', '$location', 'userService', function ($scope, $routeParams, $location, userService) {
        $scope.currentUserName = $routeParams.userName;
        var sessionLogin = $routeParams.userName.indexOf('s=') != -1;
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
        

        $scope.getInterestedStacks = function() {
            //Using users favorite techs, pull back latest stacks added
        }

        $scope.getSuggestedStacks = function() {
            //Using users favorite stacks, pick 3 most common techs and return 5 stacks not already favorited
            //Simple "you might also like" list
        }

        if (!sessionLogin) {
            userService.getUserCreatedStacks($routeParams.userName).then(function (response) {
                $scope.techStacks = response.data.TechStacks;
            });
        }
    }
]);