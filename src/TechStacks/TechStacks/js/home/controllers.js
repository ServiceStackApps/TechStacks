var app = angular.module('home.controllers', ['stacks.services']);

app.controller('homeCtrl', [
    '$scope', '$http', 'techStackServices', 'userService', function ($scope, $http, techStackServices, userService) {
        $scope.allTiers = angular.copy(techStackServices.allTiers);

        userService.isAuthenticated().then(function() {
            userService.getUserFeed().then(function(results) {
                $scope.feedStacks = results;
            });
        }, function (error) {
            techStackServices.searchStacks('').then(function(results) {
                $scope.techStacks = results;
            });
        });

        $scope.isFavorite = function (techStack) {
            var isFav = false;
            for (var i = 0; i < $scope.favoriteTechStacks.length > 0; i++) {
                var favStack = $scope.favoriteTechStacks[i];
                if (favStack.Id == techStack.Id) {
                    isFav = true;
                    break;
                }
            }
            return isFav;
        }

        $scope.addFavorite = function (techStack) {
            userService.addFavoriteTechStack(techStack);
        }

        $scope.removeFavorite = function (techStack) {
            userService.removeFavoriteTechStack(techStack);
        }
    }
]);