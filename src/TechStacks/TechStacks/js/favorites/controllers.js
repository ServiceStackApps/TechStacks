/* global angular */
(function () {
    "use strict";
    var app = angular.module('favorites.controllers', ['stacks.services']);

    app.controller('favoritesCtrl', [
        '$rootScope', '$scope', '$http', 'techStackServices', 'userService',
        function ($rootScope, $scope, $http, techStackServices, userService) {

            if ($rootScope.cachedFeedStacks) {
                $scope.feedStacks = $rootScope.cachedFeedStacks;
            }
            
            if ($rootScope.cachedTopTechnologies) {
                $scope.topTechnologies = $rootScope.cachedTopTechnologies;
                $scope.topUsers = $rootScope.cachedTopUsers;
            }

            function refresh() {
                userService.isAuthenticated().then(function () {
                    userService.getUserFeed().then(function (results) {
                        $scope.feedStacks = results;
                        $rootScope.cachedFeedStacks = $scope.feedStacks;
                    });
                });

                techStackServices.overview().then(function (overview) {
                    $scope.techStacks = overview.LatestTechStacks;
                    $scope.topTechnologies = overview.TopTechnologies;
                    $scope.topUsers = overview.TopUsers;
                    $scope.topTechCategories = [];

                    techStackServices.allTiers().then(function (allTiers) {
                        $.map(allTiers, function (tier) {
                            $scope.topTechCategories.push({
                                name: tier.name,
                                title: tier.title,
                                techs: overview.TopTechnologiesByTier[tier.name]
                            });
                        });
                    });

                    $rootScope.cachedTechStacks = $scope.techStacks;
                    $rootScope.cachedTopTechnologies = $scope.topTechnologies;
                    $rootScope.cachedTopUsers = $scope.topUsers;
                    $rootScope.cachedTopTechCategories = $scope.topTechCategories;
                });
            }

            refresh();

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
                userService.removeFavoriteTech(tech)
                .then(refresh);
            };
        }
    ]);
})();
