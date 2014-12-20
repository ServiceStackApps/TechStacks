/* global angular */
(function () {
    "use strict";

    var app = angular.module('stacks.controllers', ['stacks.services']);

    app.controller('stackCtrl', [
        '$rootScope', '$scope', '$routeParams', 'techStackServices', '$filter', 'userService', '$sce',
        function ($rootScope, $scope, $routeParams, techStackServices, $filter, userService, $sce) {

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
                $scope.DetailsHtml = $sce.trustAsHtml(techStack.DetailsHtml);
                angular.forEach($rootScope.allTiers, function (tier) {
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
        '$scope', '$location', 'techStackServices','$q', function ($scope, $location, techStackServices,$q) {
            $scope.createNewStack = function () {
                if ($scope.createInProgress) {
                    return;
                }
                $scope.createInProgress = true;
                techStackServices.createStack($scope.newStack).then(function (techStack) {
                    $scope.newStack = $scope.newStack || {};
                    $scope.newStack.Id = techStack.Id;
                    var techChoicePromises = [];
                    for (var i = 0; i < $scope.techChoices.length; i++) {
                        var techChoice = $scope.techChoices[i];
                        techChoice.TechnologyStackId = $scope.newStack.Id;
                        techChoicePromises.push(techStackServices.addTechChoice(techChoice));
                    }
                    $q.all(techChoicePromises).then(function () {
                        $scope.createInProgress = false;
                        $location.path("/stacks/" + $scope.newStack.Id);
                    });
                }, function (reason) {
                    $scope.createInProgress = false;
                    $scope.errorMessage = reason;
                });
            };
            $scope.techChoices = [];

            $scope.handleAddTech = function (item) {
                var techId = item.split(';')[0];
                var tier = item.split(';')[1];
                var techChoice = { Tier: tier, TechnologyId: techId };
                $scope.techChoices.push(techChoice);
            };

            $scope.handleRemoveTech = function (item) {
                var techId = parseInt(item.split(';')[0]);
                for (var i = 0; i < $scope.techChoices.length; i++) {
                    if ($scope.techChoices[i].Id === techId) {
                        $scope.techChoices.splice(i, 1);
                    }
                }
            };

            techStackServices.searchTech('').then(function (searchResults) {
                var expandedResults = [];
                for (var i = 0; i < searchResults.length; i++) {
                    var searchResult = searchResults[i];
                    if (searchResult.Tier == null) {
                        continue;
                    }
                    var item = {};
                    item.techKey = searchResult.Id + ';' + searchResult.Tier;
                    item.techName = searchResult.Name + ' - ' + searchResult.Tier;
                    expandedResults.push(item);
                }

                $scope.searchResults = expandedResults;
            });

            

            
        }
    ]);

    app.controller('editStackCtrl', [
        '$scope', 'techStackServices', '$routeParams', '$q', '$filter', 'userService', '$location',
        function ($scope, techStackServices, $routeParams, $q, $filter, userService, $location) {

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
                    if (searchResult.Tier == null) {
                        continue;
                    }
                    var item = {};
                    item.techKey = searchResult.Id + ';' + searchResult.Tier;
                    item.techName = searchResult.Name + ' - ' + searchResult.Tier;
                    expandedResults.push(item);
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


            $scope.updateAll = function () {
                if ($scope.updateInProgress) {
                    return;
                }
                $scope.updateInProgress = true;
                $scope.busy = true;
                techStackServices.updateStack($scope.currentStack).then(function () {
                    $scope.busy = false;
                    $scope.updateInProgress = false;
                    $location.path("/stacks/" + $scope.currentStack.Id);
                }, function (reason) {
                    $scope.busy = false;
                    $scope.updateInProgress = false;
                    $scope.errorMessage = reason;
                });
            };

            $scope.updateTechnologyChoice = function(techChoice) {
                return techStackServices.updateTechnologyChoice(techChoice);
            };

            $scope.hasRole = function (role) {
                return userService.hasRole(role);
            };

            $scope.deleteTechStack = function() {
                techStackServices.deleteTechStack($scope.currentStack).success(function() {
                    $location.path('/stacks');
                });
            }
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
