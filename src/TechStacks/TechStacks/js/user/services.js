/* global angular */
(function () {
    "use strict";
    var app = angular.module('user.services', []);

    app.service('userService', [
        '$rootScope', '$q', '$http', '$timeout',
        function ($rootScope, $q, $http, $timeout) {
            return {
                isAuthenticated: function () {
                    var deferred = $q.defer();
                    if ($rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.resolve($rootScope.currentUserSession);
                        });
                    } else if ($rootScope.isAuthenticated == null) {
                        $http.get('/sessioninfo').success(function (response) {
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
                        $timeout(function () {
                            deferred.reject();
                        });
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
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        if ($rootScope.favoriteTechStacks == null || forceUpdate) {
                            $http.get('/favorites/techstacks').success(function (response) {
                                $rootScope.favoriteTechStacks = response.Favorites || [];
                                deferred.resolve(response.Favorites);
                            }).error(function (error) {
                                $rootScope.favoriteTechStacks = null;
                                deferred.reject(error);
                            });
                        } else {
                            $timeout(function () {
                                deferred.resolve($rootScope.favoriteTechStacks);
                            });
                        }
                    }
                    return deferred.promise;
                },
                getFavoriteTechs: function (forceUpdate) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        if ($rootScope.favoriteTechs == null || forceUpdate) {
                            $http.get('/favorites/techs').success(function (response) {
                                $rootScope.favoriteTechs = response.Favorites || [];
                                deferred.resolve(response.Favorites);
                            }).error(function (error) {
                                $rootScope.favoriteTechs = null;
                                deferred.reject(error);
                            });
                        } else {
                            $timeout(function () {
                                deferred.resolve($rootScope.favoriteTechs);
                            });
                        }
                    }
                    return deferred.promise;
                },
                addFavoriteTechStack: function (techStack) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        $http.put('/favorites/techstacks', {TechnologyStackId: techStack.Id}).success(function (response) {
                            $rootScope.favoriteTechStacks.push(techStack);
                            deferred.resolve(techStack);
                        });
                    }
                    return deferred.promise;
                },
                removeFavoriteTechStack: function (techStack) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        $http.delete('/favorites/techstacks/' + techStack.Id).success(function (response) {
                            for (var i = 0; i < $rootScope.favoriteTechStacks.length; i++) {
                                var favTechStack = $rootScope.favoriteTechStacks[i];
                                if (favTechStack.Id === techStack.Id) {
                                    $rootScope.favoriteTechStacks.splice(i, 1);
                                    break;
                                }
                            }
                            deferred.resolve(techStack);
                        });
                    }
                    return deferred.promise;
                },
                addFavoriteTech: function (tech) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        $http.put('/favorites/techs', {TechnologyId: tech.Id}).success(function (response) {
                            $rootScope.favoriteTechs.push(tech);
                            deferred.resolve(tech);
                        });
                    }
                    return deferred.promise;
                },
                removeFavoriteTech: function (tech) {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        $http.delete('/favorites/techs/' + tech.Id).success(function (response) {
                            for (var i = 0; i < $rootScope.favoriteTechs.length; i++) {
                                var favTechStack = $rootScope.favoriteTechs[i];
                                if (favTechStack.Id === tech.Id) {
                                    $rootScope.favoriteTechs.splice(i, 1);
                                    break;
                                }
                            }
                            deferred.resolve(tech);
                        });
                    }
                    return deferred.promise;
                },
                getUserFeed: function () {
                    var deferred = $q.defer();
                    if (!$rootScope.isAuthenticated) {
                        $timeout(function () {
                            deferred.reject('Not authenticated');
                        });
                    } else {
                        $http.get('/myfeed').success(function (response) {
                            deferred.resolve(response.TechStacks);
                        });
                    }
                    return deferred.promise;
                },
                getUserStacks: function (userName) {
                    return $http.get('/users/' + userName + '/stacks');
                },
                getUserAvatar: function (userName) {
                    return $http.get('/users/' + userName + '/avatar');
                }
            };
        }
    ]);
})();

