/* global angular */
(function () {
    "use strict";
    var app = angular.module('home.controllers', ['stacks.services']);

    app.controller('homeCtrl', [
        '$scope', '$http', 'techStackServices', 'userService', function ($scope, $http, techStackServices, userService) {
            $scope.allTiers = angular.copy(techStackServices.allTiers);

            userService.isAuthenticated().then(function() {
                userService.getUserFeed().then(function(results) {
                    $scope.feedStacks = results;
                });
            }, function () {
                techStackServices.latestTechStacks().then(function (techstacks) {
                    $scope.techStacks = techstacks;
                });
            });

            techStackServices.trendingStacks().then(function (trending) {
                $scope.topTechnologies = trending.TopTechnologies;
                $scope.topUsers = trending.TopUsers;
            });

            $scope.isFavoriteTech = function (tech) {
                var isFav = false;
                for (var i = 0; i < $scope.favoriteTechs.length > 0; i++) {
                    var favTech = $scope.favoriteTechs[i];
                    if (favTech.Id === tech.Id) {
                        isFav = true;
                        break;
                    }
                }
                return isFav;
            };

            $scope.isFavoriteTechStack = function (techStack) {
                var isFav = false;
                for (var i = 0; i < $scope.favoriteTechStacks.length; i++) {
                    if ($scope.favoriteTechStacks[i].Id === techStack.Id) {
                        isFav = true;
                        break;
                    }
                }
                return isFav;
            };

            $scope.addFavoriteTechStack = function (techStack) {
                userService.addFavoriteTechStack(techStack);
            };

            $scope.removeFavoriteTechStack = function (techStack) {
                userService.removeFavoriteTechStack(techStack);
            };

            $scope.addFavoriteTech = function (tech) {
                userService.addFavoriteTech(tech);
            };

            $scope.removeFavoriteTech = function (tech) {
                userService.removeFavoriteTech(tech);
            };
        }
    ]);
})();
