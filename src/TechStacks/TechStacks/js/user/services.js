/* global angular */
(function () {
    "use strict";
    var app = angular.module('user.services', []);

    function addEntry(collection, item) {
        collection.push(item);
    }

    function removeEntry(collection, item) {
        for (var i = 0; i < collection.length; i++) {
            if (collection[i].Id === item.Id) {
                collection.splice(i, 1);
            }
        }
    }

    app.service('userService', [
        '$rootScope', '$q', '$http', '$timeout',
        function ($rootScope, $q, $http, $timeout) {
            return {
                isAuthenticated: function () {
                    var deferred = $q.defer();
                    if ($rootScope.isAuthenticated) {
                        deferred.resolve($rootScope.currentUserSession);
                    } else if ($rootScope.isAuthenticated == null) {
                        $http.get('/my-session').success(function (response) {
                            $rootScope.currentUserSession = response;
                            $rootScope.isAuthenticated = true;
                            deferred.resolve(response);
                        })
                            .error(function (error) {
                                $rootScope.currentUserSession = null;
                                $rootScope.isAuthenticated = false;
                                deferred.reject(error);
                            });
                    } else {
                        deferred.reject();
                    }

                    return deferred.promise;
                },
                hasRole: function (role) {
                    if ($rootScope.currentUserSession.Roles == null) {
                        return false;
                    }
                    var result = false;
                    for (var i = 0; i < $rootScope.currentUserSession.Roles.length; i++) {
                        var currentRole = $rootScope.currentUserSession.Roles[i];
                        if (currentRole === role) {
                            result = true;
                            break;
                        }
                    }
                    return result;
                },
                getFavoriteTechStacks: function (forceUpdate) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        if ($rootScope.favoriteTechStacks == null || forceUpdate) {
                            $http.get('/favorites/techtacks').success(function (response) {
                                $rootScope.favoriteTechStacks = response.Results || [];
                                deferred.resolve($rootScope.favoriteTechStacks);
                            }).error(function (error) {
                                $rootScope.favoriteTechStacks = null;
                                deferred.reject(error);
                            });
                        } else {
                            deferred.resolve($rootScope.favoriteTechStacks);
                        }
                    }
                    return deferred.promise;
                },
                getFavoriteTechs: function (forceUpdate) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        if ($rootScope.favoriteTechs == null || forceUpdate) {
                            $http.get('/favorites/technology').success(function (response) {
                                $rootScope.favoriteTechs = response.Results || [];
                                deferred.resolve($rootScope.favoriteTechs);
                            }).error(function (error) {
                                $rootScope.favoriteTechs = null;
                                deferred.reject(error);
                            });
                        } else {
                            deferred.resolve($rootScope.favoriteTechs);
                        }
                    }
                    return deferred.promise;
                },
                addFavoriteTechStack: function (techStack) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        addEntry($rootScope.favoriteTechStacks, techStack);
                        $http.put('/favorites/techtacks/' + techStack.Id).success(function (addedStack) {
                            deferred.resolve(addedStack);
                        })
                        .error(function() {
                            removeEntry($rootScope.favoriteTechStacks, techStack);
                            deferred.reject('Error');
                        });
                    }
                    return deferred.promise;
                },
                removeFavoriteTechStack: function (techStack) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        removeEntry($rootScope.favoriteTechStacks, techStack);
                        $http.delete('/favorites/techtacks/' + techStack.Id).success(function (removedStack) {
                            deferred.resolve(removedStack);
                        })
                        .error(function() {
                            addEntry($rootScope.favoriteTechStacks, techStack);
                            deferred.reject('Error');
                        });
                    }
                    return deferred.promise;
                },
                addFavoriteTech: function (tech) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        addEntry($rootScope.favoriteTechs, tech);
                        $http.put('/favorites/technology/' + tech.Id).success(function (addedTech) {
                            deferred.resolve(addedTech);
                        })
                        .error(function () {
                            removeEntry($rootScope.favoriteTechs, tech);
                            deferred.reject('Error');
                        });
                    }
                    return deferred.promise;
                },
                removeFavoriteTech: function (tech) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        removeEntry($rootScope.favoriteTechs, tech);
                        $http.delete('/favorites/technology/' + tech.Id).success(function(removedTech) {
                            deferred.resolve(removedTech);
                        })
                        .error(function () {
                            addEntry($rootScope.favoriteTechs, tech);
                            deferred.reject('Error');
                        });
                    }
                    return deferred.promise;
                },
                getUserFeed: function () {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        deferred.reject('Not authenticated');
                    } else {
                        $http.get('/my-feed').success(function (response) {
                            deferred.resolve(response.Results);
                        });
                    }
                    return deferred.promise;
                },
                getUserInfo: function (userName) {
                    return $http.get('/users/' + userName);
                }
            };
        }
    ]);
})();

