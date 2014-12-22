/* global angular */
(function () {
    "use strict";
    var app = angular.module('techs.controllers', ['tech.services', 'user.services']);

    app.controller('latestTechsCtrl', [
        '$rootScope', '$scope', 'techServices', '$timeout', function ($rootScope, $scope, techServices, $timeout) {

            function filterTechsByCategory(techs, category) {
                var filteredTechs = [];
                for (var i = 0; i < techs.length; i++) {
                    if (techs[i].Tier == category) {
                        filteredTechs.push(techs[i]);
                    }
                }
                return filteredTechs;
            }

            function getCategoryFilter() {
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
                return categoryFilter;
            }

            var lastSearch;
            $scope.refresh = function () {
                $scope.isBusy = true;
                techServices.searchTech($scope.Search || '').then(function (techs) {
                    var categoryFilter = getCategoryFilter();
                    if (categoryFilter != null) {
                        $scope.techs = filterTechsByCategory(techs, categoryFilter);
                    } else {
                        $scope.techs = techs;
                    }
                    $rootScope.cachedTechs = $scope.techs;
                    $scope.isBusy = false;
                });
            };

            

            $scope.search = function () {
                //Another key pressed before search was fired, cancel search.
                if (lastSearch) {
                    $timeout.cancel(lastSearch);
                    
                }

                //Delay to wait for keypress, prevents searches from being received out of order and UI jumping
                lastSearch = $timeout(function () {
                    $scope.refresh();
                    lastSearch = null;
                }, 150);

            }
            //init page with old cache data then immediately load latest data in background
            if ($rootScope.cachedTechs) {
                $scope.techs = $rootScope.cachedTechs;

                $scope.refresh();
            } else {
                $scope.refresh();
            }
        }
    ]);

    app.controller('techCtrl', [
        '$rootScope', '$scope', 'techServices', '$routeParams', 'userService',
        function ($rootScope, $scope, techServices, $routeParams, userService) {
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
            
            if ($rootScope.cachedTech) {
                $scope.loading = true;
                $scope.tech = $rootScope.cachedTech;
                $scope.relatedStacks = $rootScope.cachedRelatedStacks;
            }

            techServices.getTech($routeParams.techId).then(function (r) {
                $scope.tech = r.Technology;
                $scope.relatedStacks = r.TechnologyStacks;
                refreshFavorites();

                $rootScope.cachedTech = $scope.tech;
                $rootScope.cachedRelatedStacks = $scope.relatedStacks;
                $scope.loading = false;
            });
        }
    ]);

    app.controller('createTechCtrl', [
        '$scope', '$http', '$routeParams', 'techServices', '$location', function ($scope, $http, $routeParams, techServices, $location) {
            $scope.createNewTech = function() {
                techServices.createTech($scope.tech).then(function (tech) {
                    $scope.tech.Id = tech.Id;
                    $location.path("/tech/" + $scope.tech.Slug);
                });
            };
        }
    ]);

    app.controller('editTechCtrl', [
        '$scope', 'techServices', '$routeParams', '$q', '$filter', '$location', 'userService', '$timeout',
            function ($scope, techServices, $routeParams, $q, $filter, $location, userService, $timeout) {

            $scope.refreshTech = function() {
                techServices.getTech($routeParams.techId).then(function (r) {
                    $scope.tech = r.Technology;
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
                    techServices.updateTech($scope.tech).then(function (updatedTech) {
                        $location.path('/tech/' + updatedTech.Slug);
                    });
                });
            };
        }
    ]);
})();

