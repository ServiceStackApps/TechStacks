/* global angular */
(function () {
    "use strict";

    var app = angular.module('stacks.controllers', ['stacks.services']);

    app.controller('stackCtrl', [
        '$rootScope', '$scope', '$routeParams', 'techStackServices', '$filter', 'userService', '$sce',
        function ($rootScope, $scope, $routeParams, techStackServices, $filter, userService, $sce) {

            function isFavoriteTechStack(techStack) {
                for (var i = 0; i < $scope.favoriteTechStacks.length; i++) {
                    var favStack = $scope.favoriteTechStacks[i];
                    if (favStack.Id === techStack.Id) {
                        return true;
                    }
                }
                return false;
            }

            function refreshFavorites() {
                userService.getFavoriteTechStacks().then(function (techStacks) {
                    $scope.isFavorite = isFavoriteTechStack($scope.currentStack);
                });
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
                $scope.stackTiers = [];

                techStackServices.allTiers().then(function (allTiers) {
                    $.map(allTiers, function (tier) {
                        var techChoices = $.grep(techStack.TechnologyChoices, function(x) {
                            return x.Tier == tier.name;
                        });
                        if (techChoices.length > 0) {
                            $scope.stackTiers.push({ title: tier.title, techChoices: techChoices });
                        }
                    });
                });

                $rootScope.cachedStack = $scope.currentStack;
                $rootScope.cachedDetailsHtml = $scope.DetailsHtml;
                $scope.loading = false;

                refreshFavorites();
            });

            techStackServices.getTechStackFavorites($routeParams.stackId).then(function (r) {
                $scope.favoriteCount = r.FavoriteCount;
            });

            techStackServices.getPageStats($routeParams.stackId).then(function (r) {
                $scope.stats = r;
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
                var url = encodeURIComponent(document.location.origin + '/' + $scope.currentStack.Slug);
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
                if ($scope.createInProgress) return;
                
                $scope.createInProgress = true;
                techStackServices.createStack($scope.newStack).then(function (techStack) {
                    $scope.newStack = $scope.newStack || {};
                    $scope.newStack.Id = techStack.Id;
                    $scope.createInProgress = false;
                    $location.path("/" + techStack.Slug);
                }, function () {
                    $scope.createInProgress = false;
                });
            };
            $scope.techChoices = [];

            if (!$scope.newStack)
                $scope.newStack = {};

            $scope.newStack.TechnologyIds = [];

            techStackServices.searchTech('').then(function (searchResults) {
                var expandedResults = [];
                for (var i = 0; i < searchResults.length; i++) {
                    var searchResult = searchResults[i];
                    if (searchResult.Tier == null) continue;
                    expandedResults.push({
                        value: searchResult.Id,
                        name: searchResult.Name + ' - ' + searchResult.Tier
                    });
                }
                $scope.searchResults = expandedResults;
            });
        }
    ]);

    app.controller('editStackCtrl', [
        '$scope', 'techStackServices', '$routeParams', '$q', '$filter', 'userService', '$location',
        function ($scope, techStackServices, $routeParams, $q, $filter, userService, $location) {

            $scope.refreshStack = function () {
                techStackServices.getStack($routeParams.stackId, true).then(function (techStack) {
                    $scope.currentStack = techStack;
                    $scope.currentStack.TechnologyIds = $.map($scope.currentStack.TechnologyChoices, function (techChoice) {
                        return techChoice.TechnologyId;
                    });
                });
            };

            techStackServices.getStackPreviousVersions($routeParams.stackId).then(function(results) {
                $scope.previousVersions = results;
            });
            $scope.loadPreviousVersion = function (version) {
                $.map(['Name', 'VendorName', 'AppUrl', 'ScreenshotUrl', 'Description', 'Details'], function (key) {
                    $scope.currentStack[key] = version[key];
                });
                $scope.currentStack.TechnologyIds = version.TechnologyIds.slice(); //deep clone
            };

            $scope.refreshStack();

            techStackServices.searchTech('').then(function (searchResults) {
                var expandedResults = [];
                for (var i = 0; i < searchResults.length; i++) {
                    var searchResult = searchResults[i];
                    if (searchResult.Tier == null) continue;
                    expandedResults.push({
                        value: searchResult.Id,
                        name: searchResult.Name + ' - ' + searchResult.Tier
                    });
                }
                $scope.searchResults = expandedResults;
            });

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
                    $location.path("/" + updatedStack.Slug);
                }, function () {
                    $scope.busy = false;
                    $scope.updateInProgress = false;
                });
            };

            $scope.hasRole = function (role) {
                return userService.hasRole(role);
            };

            $scope.deleteTechStack = function() {
                techStackServices.deleteTechStack($scope.currentStack).then(function() {
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
