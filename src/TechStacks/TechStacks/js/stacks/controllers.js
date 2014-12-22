/* global angular */
(function () {
    "use strict";

    var app = angular.module('stacks.controllers', ['stacks.services']);

    app.controller('stackCtrl', [
        '$rootScope', '$scope', '$routeParams', 'techStackServices', '$filter', 'userService', '$sce',
        function ($rootScope, $scope, $routeParams, techStackServices, $filter, userService, $sce) {

            function isFavoriteTechStack(techStack) {
                var isFav = false;
                for (var i = 0; i < $scope.favoriteTechStacks.length; i++) {
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
            
            //load last page with opacity to increase perceived perf
            if ($rootScope.cachedStack) {
                $scope.loading = true;
                $scope.currentStack = $rootScope.cachedStack;
                $scope.DetailsHtml = $rootScope.cachedDetailsHtml;
            }

            techStackServices.getStack($routeParams.stackId).then(function (techStack) {
                $scope.currentStack = techStack;
                $scope.DetailsHtml = $sce.trustAsHtml(techStack.DetailsHtml);
                angular.forEach($rootScope.allTiers, function (tier) {
                    tier.show = filterTechChoiceByTier(tier.name).length > 0;
                });

                $rootScope.cachedStack = $scope.currentStack;
                $rootScope.cachedDetailsHtml = $scope.DetailsHtml;
                $scope.loading = false;

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

            function filterCharactersFromHashTag(name) {
                var dotsFind = new RegExp('[.]','g'),
                    spaceFind = new RegExp(' ','g'),
                    sharpFind =  new RegExp('[#]','g'),
                    plusFind =  new RegExp('[+]','g');
                var result = name.replace(dotsFind, '').replace(spaceFind, '').replace(sharpFind, 'Sharp').replace(plusFind, 'Plus');
                return result;
            }

            function extractHasTags() {
                var result = [];
                for (var i = 0; i < $scope.currentStack.TechnologyChoices.length; i++) {
                    var techChoice = $scope.currentStack.TechnologyChoices[i];
                    result.push('#' + filterCharactersFromHashTag(techChoice.Name));
                }
                return result;
            }

            $scope.share = function (type) {
                var url = encodeURIComponent(document.location.origin + '/stacks/' + $scope.currentStack.Slug);
                var message = encodeURIComponent('Checkout ' + $scope.currentStack.Name + ' on Techstacks.io !');
                var name = encodeURIComponent($scope.currentStack.Name);
                var hashTags = extractHasTags();
                var allHashTags = '';
                for (var i = 0; i < hashTags.length; i++) {
                    var hashTag = hashTags[i];
                    //116 for 2 spaces and encoded url on twitter (22).
                    if ((allHashTags.length + message.length + hashTag.length + 1) > 116) {
                        break;
                    }
                    allHashTags += ' ' + hashTag;
                }
                allHashTags = encodeURIComponent(allHashTags);
                var width = 575,
                    height = 400,
                    left = ($(window).width() - width) / 2,
                    top = ($(window).height() - height) / 2,
                    opts = 'status=1' +
                        ',width=' + width +
                        ',height=' + height +
                        ',top=' + top +
                        ',left=' + left;
                switch (type) {
                    case 'facebook':
                        window.open('https://www.facebook.com/sharer/sharer.php?u=' + url + '&t=' + message + '%20' + url, '', opts);
                        break;
                    case 'twitter':
                        window.open('https://twitter.com/intent/tweet?text=' + message + '%20' + url + '%20' + allHashTags, '', opts);
                        break;
                    case 'google':
                        window.open('https://plus.google.com/share?url=' + url, '', opts);
                        break;
                    case 'reddit':
                        window.open('http://www.reddit.com/submit?url=' + url + '&title=' + name, '', opts);
                        break;
                    case 'linkedin':
                        window.open('http://www.linkedin.com/shareArticle?mini=true&url=' + url + '&title=' + message, '', opts);
                        break;
                    case 'email':
                        window.open('mailto:?subject=' + name + '&body=' + message + '%20' + url);
                        break;
                    default:
                }
                return false;
            }
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
                        $location.path("/stacks/" + $scope.newStack.Slug);
                    });
                }, function () {
                    $scope.createInProgress = false;
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

            function filterTechChoiceByTier(tier) {
                return $filter('filter')($scope.currentStack.TechnologyChoices, { Tier: tier });
            }

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

            function extractToSelectedTechs(items) {
                var result = [];
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    item.techKey = item.TechnologyId + ';' + item.Tier;
                    result.push(item.techKey);
                }
                return result;
            }

            $scope.refreshStack = function() {
                techStackServices.getStack($routeParams.stackId, true).then(function(techStack) {
                    $scope.currentStack = techStack;
                    angular.forEach($scope.allTiers, function(tier) {
                        tier.show = filterTechChoiceByTier(tier.name).length > 0;
                    });
                    $scope.selectedTechs = extractToSelectedTechs($scope.currentStack.TechnologyChoices);
                });
            };

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


            $scope.updateAll = function () {
                if ($scope.updateInProgress) {
                    return;
                }
                $scope.updateInProgress = true;
                $scope.busy = true;
                techStackServices.updateStack($scope.currentStack).then(function(updatedStack) {
                    $scope.busy = false;
                    $scope.updateInProgress = false;
                    $scope.currentStack = updatedStack;
                    $location.path("/stacks/" + updatedStack.Slug);
                }, function () {
                    $scope.busy = false;
                    $scope.updateInProgress = false;
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
            };
        }
    ]);

    app.controller('latestStacksCtrl', ['$rootScope', '$scope', 'techStackServices',
        function ($rootScope, $scope, techStackServices) {
            
            //init page with old cache data then immediately load latest data in background
            if ($rootScope.cachedTechStacks) {
                $scope.techStacks = $rootScope.cachedTechStacks;
            }

            $scope.refresh = function () {
                techStackServices.searchStacks($scope.Search || '').then(function (results) {
                    $scope.techStacks = results;
                    $rootScope.cachedTechStacks = $scope.techStacks;
                });
            };
            $scope.refresh();
        }
    ]);
})();
