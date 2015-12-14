﻿/* global angular */
(function () {
    "use strict";
    var app = angular.module('techs.controllers', ['tech.services', 'user.services']);

    app.controller('latestTechsCtrl', [
        '$rootScope', '$scope', 'techServices', '$timeout', '$location', function ($rootScope, $scope, techServices, $timeout, $location) {

            $scope.category = $location.search().category;

            function filterTechsByCategory(techs, category) {
                var filteredTechs = [];
                for (var i = 0; i < techs.length; i++) {
                    if (techs[i].Tier == category) {
                        filteredTechs.push(techs[i]);
                    }
                }
                return filteredTechs;
            }

            var lastSearch;
            $scope.refresh = function () {
                $scope.isBusy = true;
                techServices.searchTech($scope.Search || '').then(function (techs) {
                    if ($scope.category != null) {
                        $scope.techs = filterTechsByCategory(techs, $scope.category);
                    } else {
                        $scope.techs = techs;
                    }
                    $rootScope.cachedTechs = $scope.techs;
                    $scope.isBusy = false;
                });
            };
            
            $scope.search = function() {
                //Another key pressed before search was fired, cancel search.
                if (lastSearch) {
                    $timeout.cancel(lastSearch);
                }

                //Delay to wait for keypress, prevents searches from being received out of order and UI jumping
                lastSearch = $timeout(function() {
                    $scope.refresh();
                    lastSearch = null;
                }, 150);
            };
            
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
        function($rootScope, $scope, techServices, $routeParams, userService) {
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
                userService.getFavoriteTechs().then(function(techs) {
                    $scope.isFavorite = isFavoriteTech($scope.tech);
                });
            }

            $scope.addFavorite = function() {
                userService.addFavoriteTech($scope.tech).then(function(techStack) {
                    refreshFavorites();
                    $scope.favoriteCount++;
                });
            };

            $scope.removeFavorite = function() {
                userService.removeFavoriteTech($scope.tech).then(function(techStack) {
                    refreshFavorites();
                    $scope.favoriteCount--;
                });
            };

            $scope.hasRole = function(role) {
                return userService.hasRole(role);
            };

            //load last page with opacity to increase perceived perf
            if ($rootScope.cachedTech) {
                $scope.loading = true;
                $scope.tech = $rootScope.cachedTech;
                $scope.relatedStacks = $rootScope.cachedRelatedStacks;
            }

            techServices.getTech($routeParams.techId).then(function(r) {
                $scope.tech = r.Technology;
                $scope.relatedStacks = r.TechnologyStacks;
                refreshFavorites();

                $rootScope.cachedTech = $scope.tech;
                $rootScope.cachedRelatedStacks = $scope.relatedStacks;
                $scope.loading = false;
            });

            techServices.getPageStats($routeParams.techId).then(function(r) {
                $scope.stats = r;
            });

            techServices.getTechFavorites($routeParams.techId).then(function(r) {
                $scope.favoriteCount = r.FavoriteCount;
            });
        }
    ]);

    app.controller('createTechCtrl', [
        '$scope', '$http', '$routeParams', 'techServices', '$location', function ($scope, $http, $routeParams, techServices, $location) {
            $scope.createNewTech = function() {
                techServices.createTech($scope.tech).then(function (tech) {
                    $scope.tech = tech;
                    $location.path("/tech/" + $scope.tech.Slug);
                });
            };

            if (!$scope.tech)
                $scope.tech = {};

            $scope.tech.Tier = $location.search().category;
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

            techServices.getTechPreviousVersions($routeParams.techId).then(function (results) {
                $scope.previousVersions = results;
            });
            $scope.loadPreviousVersion = function (version) {
                $.map(['Name', 'VendorName', 'Tier', 'Description', 'LogoUrl', 'ProductUrl', 'VendorUrl'], function (key) {
                    $scope.tech[key] = version[key];
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
                techServices.deleteTech($scope.tech).then(function (response) {
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

