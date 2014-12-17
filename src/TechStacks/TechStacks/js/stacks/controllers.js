/* global angular */
(function () {
    "use strict";

    var app = angular.module('stacks.controllers', ['stacks.services']);

    app.controller('stackCtrl', [
        '$scope', '$routeParams', 'techStackServices', '$filter', 'userService', '$sce', function ($scope, $routeParams, techStackServices, $filter, userService, $sce) {
            $scope.allTiers = angular.copy(techStackServices.allTiers);

            function isFavoriteTechStack(techStack) {
                var isFav = false;
                for (var i = 0; i < $scope.favoriteTechStacks.length > 0; i++) {
                    var favStack = $scope.favoriteTechStacks[i];
                    if (favStack.Id === techStack.Id) {
                        isFav = true;
                        break;
                    }
                }
                return isFav;
            }

            function refreshFavorites() {
                userService.getFavoriteTechStacks().then(function (techStacks) {
                    $scope.isFavorite = isFavoriteTechStack($scope.currentStack);
                });
            }

            function filterTechChoiceByTier(tier) {
                return $filter('filter')($scope.currentStack.TechnologyChoices, { Tier: tier });
            }

            techStackServices.getStack($routeParams.stackId).then(function (techStack) {
                $scope.currentStack = techStack;
                console.log('$sce', $sce);
                $scope.DetailsHtml = $sce.trustAsHtml(techStack.DetailsHtml);
                angular.forEach($scope.allTiers, function (tier) {
                    tier.show = filterTechChoiceByTier(tier.name).length > 0;
                });
                refreshFavorites();
            });

            $scope.addFavorite = function () {
                userService.addFavoriteTechStack($scope.currentStack).then(function (techStack) {
                    refreshFavorites();
                });
            };

            $scope.removeFavorite = function() {
                userService.removeFavoriteTechStack($scope.currentStack).then(function (techStack) {
                    refreshFavorites();
                });
            };

            $scope.hasRole = function (role) {
                return userService.hasRole(role);
            };
        }
    ]);

    app.controller('createStackCtrl', [
        '$scope', '$location', 'techStackServices', function ($scope, $location, techStackServices) {
            $scope.createNewStack = function() {
                techStackServices.createStack($scope.newStack).then(function (techStack) {
                    $scope.newStack = $scope.newStack || {};
                    $scope.newStack.Id = techStack.Id;
                    $location.path("/i/stacks/" + $scope.newStack.Id + "/edit");
                });
            };
        }
    ]);

    app.controller('editStackCtrl', [
        '$scope', 'techStackServices', '$routeParams', '$q', '$filter', 'userService', '$location',
        function ($scope, techStackServices, $routeParams, $q, $filter, userService, $location) {

            $scope.allTiers = angular.copy(techStackServices.allTiers);

            $scope.refreshStack = function() {
                techStackServices.getStack($routeParams.stackId).then(function(techStack) {
                    $scope.currentStack = techStack;
                    angular.forEach($scope.allTiers, function(tier) {
                        tier.show = filterTechChoiceByTier(tier.name).length > 0;
                    });
                    $scope.selectedTechs = extractToSelectedTechs($scope.currentStack.TechnologyChoices);
                });
            };

            function extractToSelectedTechs(items) {
                var result = [];
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    item.techKey = item.TechnologyId + ';' + item.Tier;
                    result.push(item.techKey);
                }
                return result;
            }

            $scope.refreshStack();

            techStackServices.searchTech('').then(function (searchResults) {
                var expandedResults = [];
                for (var i = 0; i < searchResults.length; i++) {
                    var searchResult = searchResults[i];
                    if (searchResult.Tiers == null) {
                        continue;
                    }
                    for (var j = 0; j < searchResult.Tiers.length; j++) {
                        var item = {};
                        var tier = searchResult.Tiers[j];
                        angular.extend(item, searchResult);
                        angular.extend(item, { Tier: tier });
                        item.techKey = item.Id + ';' + item.Tier;
                        item.techName = item.Name + ' - ' + item.Tier;
                        expandedResults.push(item);
                    }
                }

                $scope.searchResults = expandedResults;

            });

            function getLocalTechChoice(id) {

                var result;
                var techId = parseInt(id.split(';')[0]);
                var tier = id.split(';')[1];
                for (var i = 0; i < $scope.currentStack.TechnologyChoices.length; i++) {
                    var technologyChoice = $scope.currentStack.TechnologyChoices[i];
                    if (technologyChoice.TechnologyId === techId && technologyChoice.Tier === tier) {
                        result = technologyChoice;
                        break;
                    }
                }
                return result;
            }

            $scope.handleAddTech = function (item) {
                var techId = item.split(';')[0];
                var tier = item.split(';')[1];
                var techChoice = { Tier: tier,TechnologyId: techId, TechnologyStackId: $scope.currentStack.Id };
                techStackServices.addTechChoice(techChoice).then(function (techChoice) {
                    $scope.refreshStack();
                });
            };

            $scope.handleRemoveTech = function (item) {
                var techChoice = getLocalTechChoice(item);
                techStackServices.removeTechChoice(techChoice).then(function (techStack) {
                    $scope.refreshStack();
                });
            };

            function filterTechChoiceByTier(tier) {
                return $filter('filter')($scope.currentStack.TechnologyChoices, { Tier: tier });
            }


            $scope.updateAll = function() {
                var updatePromises = [];
                updatePromises.push(techStackServices.updateStack($scope.currentStack));
                $scope.busy = true;
                $q.all(updatePromises).then(function() {
                    $scope.busy = false;
                    $location.path("/");
                });
            };

            $scope.updateTechnologyChoice = function(techChoice) {
                return techStackServices.updateTechnologyChoice(techChoice);
            };

            $scope.hasRole = function (role) {
                return userService.hasRole(role);
            };
        }
    ]);

    app.controller('latestStacksCtrl', ['$scope', 'techStackServices',
        function ($scope, techStackServices) {
            $scope.refresh = function () {
                techStackServices.searchStacks($scope.Search || '').then(function (results) {
                    $scope.techStacks = results.reverse();
                });
            };
            $scope.refresh();
        }
    ]);
})();
