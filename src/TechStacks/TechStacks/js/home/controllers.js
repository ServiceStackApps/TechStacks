/* global angular */
(function () {
    "use strict";
    var app = angular.module('home.controllers', ['stacks.services']);

    app.controller('homeCtrl', [
        '$rootScope', '$scope', '$http', 'techStackServices',
        function ($rootScope, $scope, $http, techStackServices) {
            
            if ($rootScope.cachedTechStacks) {
                $scope.techStacks = $rootScope.cachedTechStacks;
                $scope.topTechnologies = $rootScope.cachedTopTechnologies;
                $scope.topUsers = $rootScope.cachedTopUsers;
                $scope.topTechCategories = $rootScope.cachedTopTechCategories;
                $scope.popularTechStacks = $rootScope.cachedPopularTechStacks;
            }

            function refresh() {
                techStackServices.overview().then(function (overview) {
                    $scope.techStacks = overview.LatestTechStacks;
                    $scope.topTechnologies = overview.TopTechnologies;
                    $scope.topUsers = overview.TopUsers;
                    $scope.topTechCategories = [];
                    $scope.popularTechStacks = overview.PopularTechStacks;

                    techStackServices.allTiers().then(function(allTiers) {
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
                    $rootScope.cachedPopularTechStacks = $scope.popularTechStacks;
                });
            }

            refresh();
        }
    ]);
})();
