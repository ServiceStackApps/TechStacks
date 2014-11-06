var app = angular.module('navigation.controllers', []);

app.controller('navigationCtrl', [
    '$scope', '$location', '$http', 'userService',
    function ($scope, $location, $http, userService) {
        $scope.IsRouteActive = function(routePath) {
            return routePath == $location.path();
        };

        $scope.isFavoriteTechStack = function(techStack) {
            var isFav = false;
            for (var i = 0; i < $scope.favoriteTechStacks.length; i++) {
                if ($scope.favoriteTechStacks[i].Id == techStack.Id) {
                    isFav = true;
                    break;
                }
            }
            return isFav;
        }

        userService.isAuthenticated().then(function (session) {
            userService.getFavoriteTechStacks();
            userService.getFavoriteTechs();
        }, function(error) {
            
        });
    }
]);