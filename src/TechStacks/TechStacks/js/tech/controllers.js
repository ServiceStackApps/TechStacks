/* global angular */
(function () {
    "use strict";
    var app = angular.module('techs.controllers', ['tech.services', 'user.services']);

    app.controller('latestTechsCtrl', [
        '$scope', 'techServices', function ($scope, techServices) {
            $scope.allTiers = angular.copy(techServices.allTiers);

            $scope.refresh = function () {
                techServices.searchTech($scope.Search || '').then(function (techs) {

                    var categoryFilter = null;
                    if ($scope.category && $scope.category.title) {
                        for (var i = 0; i < $scope.allTiers.length; i++) {
                            var tier = $scope.allTiers[i];
                            if (tier.title === $scope.category.title) {
                                categoryFilter = tier.name;
                                break;
                            }
                        }
                    }
                    
                    if (categoryFilter != null) {
                        var filteredTechs = [];
                        for (var i =0; i<techs.length; i++) {
                            if (techs[i].Tier == categoryFilter) {
                                filteredTechs.push(techs[i]);
                            }
                        }
                        $scope.techs = filteredTechs;
                    } else {
                        $scope.techs = techs;
                    }
                });
            };
            $scope.refresh();
        }
    ]);

    app.controller('techCtrl', [
        '$scope', 'techServices', '$routeParams', 'userService', function ($scope, techServices, $routeParams, userService) {
            function isFavoriteTech(tech) {
                var isFav = false;
                for (var i = 0; i < $scope.favoriteTechs.length > 0; i++) {
                    var favTech = $scope.favoriteTechs[i];
                    if (favTech.Id === tech.Id) {
                        isFav = true;
                        break;
                    }
                }
                return isFav;
            }

            function refreshFavorites() {
                userService.getFavoriteTechs().then(function (techs) {
                    $scope.isFavorite = isFavoriteTech($scope.tech);
                });
            }

            $scope.addFavorite = function () {
                userService.addFavoriteTech($scope.tech).then(function (techStack) {
                    refreshFavorites();
                });
            };

            $scope.removeFavorite = function () {
                userService.removeFavoriteTech($scope.tech).then(function (techStack) {
                    refreshFavorites();
                });
            };

            $scope.hasRole = function (role) {
                return userService.hasRole(role);
            };

            techServices.getTech($routeParams.techId).then(function (tech) {
                $scope.tech = tech;
                refreshFavorites();
                techServices.getRelatedStacks(tech.Id).then(function (stacks) {
                    $scope.relatedStacks = stacks;
                });
            });

            
        }
    ]);

    app.controller('createTechCtrl', [
        '$scope', '$http', '$routeParams', 'techServices', '$location', function ($scope, $http, $routeParams, techServices, $location) {
            $scope.allTiers = angular.copy(techServices.allTiers);

            $scope.createNewTech = function() {
                techServices.createTech($scope.tech).then(function (tech) {
                    $scope.tech.Id = tech.Id;
                    $location.path("/tech/" + $scope.tech.Id);
                });
            };
        }
    ]);

    app.controller('editTechCtrl', [
        '$scope', 'techServices', '$routeParams', '$q', '$filter', '$location', 'userService', '$timeout',
            function ($scope, techServices, $routeParams, $q, $filter, $location, userService, $timeout) {

            $scope.allTiers = angular.copy(techServices.allTiers);
            $scope.refreshTech = function() {
                techServices.getTech($routeParams.techId).then(function (tech) {
                    $scope.tech = tech;
                    $scope.tech.Tiers = $scope.tech.Tiers || [];
                });
            };

            $scope.refreshTech();

            $scope.hasRole = function(role) {
                return userService.hasRole(role);
            };

            $scope.approveLogo = function() {
                techServices.approveLogo($scope.tech, true).then(function (tech) {
                    $scope.tech = tech;
                });
            };

            $scope.unApproveLogo = function () {
                techServices.approveLogo($scope.tech, false).then(function (tech) {
                    $scope.tech = tech;
                });
            };

            $scope.deleteTech = function () {
                techServices.deleteTech($scope.tech).success(function (response) {
                    $location.path('/tech');
                });
            };

            $scope.done = function () {
                //Wait for ng-model digest after change to make sure model is updated.
                $timeout(function () {
                    techServices.updateTech($scope.tech).then(function () {
                        $location.path('/tech/' + $scope.tech.Id);
                    });
                });
            };
        }
    ]);
})();

