var app = angular.module('techs.controllers', []);

app.controller('latestTechsCtrl', [
    '$scope', 'techServices', function ($scope, techServices) {
        techServices.getAllTechs().then(function(techs) {
            $scope.techs = techs;
        });

        $scope.makeFavorite = function() {
            
        }
    }
]);

app.controller('techCtrl', [
    '$scope', 'techServices', '$routeParams', 'userService', function ($scope, techServices, $routeParams, userService) {
        techServices.getTech($routeParams.techId).then(function(tech) {
            $scope.tech = tech;
            refreshFavorites();
        });

        $scope.addFavorite = function () {
            userService.addFavoriteTech($scope.tech).then(function (techStack) {
                refreshFavorites();
            });
        }

        $scope.removeFavorite = function () {
            userService.removeFavoriteTech($scope.tech).then(function (techStack) {
                refreshFavorites();
            });
        }

        function refreshFavorites() {
            userService.getFavoriteTechs().then(function (techs) {
                $scope.isFavorite = isFavoriteTech($scope.tech);
            });
        }

        $scope.hasRole = function (role) {
            return userService.hasRole(role);
        };

        function isFavoriteTech(tech) {
            var isFav = false;
            for (var i = 0; i < $scope.favoriteTechs.length > 0; i++) {
                var favTech = $scope.favoriteTechs[i];
                if (favTech.Id == tech.Id) {
                    isFav = true;
                    break;
                }
            }
            return isFav;
        }
    }
]);

app.controller('createTechCtrl', [
    '$scope', '$http', '$routeParams', 'techServices', '$location', function ($scope, $http, $routeParams, techServices, $location) {
        $scope.createNewTech = function() {
            techServices.createTech($scope.tech).then(function (tech) {
                $scope.tech.Id = tech.Id;
                $location.path("/i/techs/" + $scope.tech.Id + "/edit");
            });
        }

        $scope.addTechToTier = function (tier) {
            $scope.tech.Tiers.push(tier);
        };

        $scope.removeTierFromTech = function (tier) {
            $scope.tech.Tiers.splice($scope.tech.Tiers.indexOf(tier), 1);
        }

        $scope.$watch('techQuery', function (newVal, oldVal) {
            if ($scope.techQuery) {
                techStackServices.searchTech($scope.techQuery).then(function (searchResults) {
                    $scope.searchResults = searchResults;
                });
            } else {
                $scope.searchResults = [];
            }
        });
    }
]);

app.controller('editTechCtrl', [
    '$scope', 'techServices', '$routeParams', '$q', '$filter', '$location', 'userService', function ($scope, techServices, $routeParams, $q, $filter, $location, userService) {

        $scope.allTiers = angular.copy(techServices.allTiers);
        $scope.refreshTech = function() {
            techServices.getTech($routeParams.techId).then(function (tech) {
                $scope.tech = tech;
            });
        };

        $scope.addTechToTier = function(tier) {
            $scope.tech.Tiers.push(tier.name);
        };

        $scope.removeTierFromTech = function(tier) {
            $scope.tech.Tiers.splice($scope.tech.Tiers.indexOf(tier), 1);
        }

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

        $scope.updateTech = function() {
            return techServices.updateTech($scope.tech);
        };

        $scope.done = function() {
            techServices.updateTech($scope.tech).then(function() {
                $location.path('/i/techs/' + $scope.tech.Id);
            });
        }
    }
]);

app.directive('chosenTechSelect', ['$timeout',function ($timeout) {
    return {
        restrict: 'E',
        template: '<select ng-show="data" multiple class="chosen"><option ng-repeat="item in data" value="{{item.Id}};{{item.Tier}}">{{item.Name}} - {{item.Tier}}</option></select>',
        scope: {
            data: '=',
            options: '=',
            selectedValues: '=',
            onSelection: '&',
            onAdd: '&',
            onRemove: '&'
        },
        replace:true,
        link: function (scope, element, attrs) {
            //One off bind
            var initWatch = scope.$watch('data', function(newVal, oldval) {
                $timeout(function () {
                    if (scope.data != null && scope.data.length > 0) {
                        $(element).chosen(scope.options);
                        $(element).chosen().change(function (event, item) {
                            if (item.selected) {
                                scope.onAdd({ item: item.selected });
                            }
                            if (item.deselected) {
                                scope.onRemove({ item: item.deselected });
                            }
                        });
                        initWatch();
                    }
                });
            });

            scope.$watch('selectedValues', function(newVal, oldVal) {
                $timeout(function() {
                    if (scope.selectedValues) {
                        $(element).chosen().val(scope.selectedValues);
                        $(element).chosen().trigger("chosen:updated");
                    }
                });
            });
        }
    }
}])