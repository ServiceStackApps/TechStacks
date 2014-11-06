var app = angular.module('stacks.controllers', ['stacks.services']);

app.controller('stackCtrl', [
    '$scope', '$routeParams', 'techStackServices', '$filter', 'userService', function ($scope, $routeParams, techStackServices, $filter, userService) {
        $scope.allTiers = angular.copy(techStackServices.allTiers);
        techStackServices.getStack($routeParams.stackId).then(function (techStack) {
            $scope.currentStack = techStack;
            angular.forEach($scope.allTiers, function (tier) {
                tier.show = filterTechChoiceByTier(tier.name).length > 0;
            });
            refreshFavorites();
        });

        $scope.addFavorite = function () {
            userService.addFavoriteTechStack($scope.currentStack).then(function (techStack) {
                refreshFavorites();
            });
        }

        $scope.removeFavorite = function() {
            userService.removeFavoriteTechStack($scope.currentStack).then(function (techStack) {
                refreshFavorites();
            });
        }

        function refreshFavorites() {
            userService.getFavoriteTechStacks().then(function (techStacks) {
                $scope.isFavorite = isFavoriteTechStack($scope.currentStack);
            });
        }

        function isFavoriteTechStack(techStack) {
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

        function filterTechChoiceByTier(tier) {
            return $filter('filter')($scope.currentStack.TechnologyChoices, { Tier: tier });
        }
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
    '$scope', 'techStackServices', '$routeParams', '$q', '$filter', 'userService', function ($scope, techStackServices, $routeParams, $q, $filter, userService) {

        $scope.allTiers = angular.copy(techStackServices.allTiers);

        $scope.refreshStack = function() {
            techStackServices.getStack($routeParams.stackId).then(function(techStack) {
                $scope.currentStack = techStack;
                angular.forEach($scope.allTiers, function(tier) {
                    tier.show = filterTechChoiceByTier(tier.name).length > 0;
                });
            });
        };

        $scope.refreshStack();

        function filterTechChoiceByTier(tier) {
            return $filter('filter')($scope.currentStack.TechnologyChoices, { Tier: tier });
        }

        $scope.$watch('techQuery', function(newVal, oldVal) {
            if ($scope.techQuery) {
                techStackServices.searchTech($scope.techQuery).then(function(searchResults) {
                    $scope.searchResults = searchResults;
                });
            } else {
                $scope.searchResults = [];
            }
        });

        $scope.removeTechChoice = function(techChoice) {
            techStackServices.removeTechChoice(techChoice).then(function(techStack) {
                $scope.refreshStack();
            });
        };

        $scope.doesStackContainTechOfSameTier = function(tech, tier) {
            if ($scope.currentStack.TechnologyChoices != null && $scope.currentStack.TechnologyChoices.length > 0) {
                for (var i = 0; i < $scope.currentStack.TechnologyChoices.length; i++) {
                    var existingTechChoice = $scope.currentStack.TechnologyChoices[i];
                    if (existingTechChoice.TechnologyId == tech.Id && existingTechChoice.Tier == tier) {
                        return false;
                    }
                }
            }
            return true;
        }

        $scope.addTechChoice = function(tech, selectedTier) {
            if ($scope.currentStack.TechnologyChoices != null && $scope.currentStack.TechnologyChoices.length > 0) {
                for (var i = 0; i < $scope.currentStack.TechnologyChoices.length; i++) {
                    var existingTechChoice = $scope.currentStack.TechnologyChoices[i];
                    if (existingTechChoice.TechnologyId == tech.Id && existingTechChoice.Tier == selectedTier) {
                        return;
                    }
                }
            }

            var techChoice = {
                TechnologyStackId: $scope.currentStack.Id,
                TechnologyId: tech.Id,
                Tier: selectedTier
            }
            techStackServices.addTechChoice(techChoice).then(function(techChoice) {
                $scope.refreshStack();
            });
        };

        $scope.updateAll = function() {
            var updatePromises = [];
            updatePromises.push(techStackServices.updateStack($scope.currentStack));
            $scope.busy = true;
            $q.all(updatePromises).then(function() {
                $scope.busy = false;
                window.location.href = "/";
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
    function ($scope,techStackServices) {
        techStackServices.searchStacks('').then(function(results) {
            $scope.techStacks = results.reverse();
        });
    }
]);