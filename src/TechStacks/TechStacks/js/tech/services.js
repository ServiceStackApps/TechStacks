(function () {
    "use strict";
    var app = angular.module('tech.services', []);

    app.service('techServices', ['$rootScope', '$http', '$q', function ($rootScope, $http, $q) {

        var errorStatusMessages = {
            401: 'Not Authenticated',
            500: 'Internal Server Error'
        };

        var getResults = function (promise) {
            var deferred = $q.defer();
            $rootScope.errorMessage = null;
            promise
                .success(function (response) {
                    deferred.resolve(response.Result || response.Results || response);
                })
                .error(function (e, status) {
                    $rootScope.errorMessage =
                        (e && e.ResponseStatus && e.ResponseStatus.Message) //DTO Error
                         || e //Raw Error
                         || errorStatusMessages[status]; //HTTP Status Error

                    deferred.reject($rootScope.errorMessage);
                });
            return deferred.promise;
        };

        return {
            getResults: getResults,
            getTech: function(id) {
                return getResults($http.get('/technology/' + id));
            },
            getTechPreviousVersions: function (id) {
                return getResults($http.get('/technology/' + id + '/previous-versions'));
            },
            getTechFavorites: function (id) {
                return getResults($http.get('/technology/' + id + '/favorites'));
            },
            searchTech: function (searchQuery) {
                return getResults($http.get('/technology/search?orderBy=Name&NameContains=' + searchQuery + "&DescriptionContains=" + searchQuery));
            },
            getAllTechs: function () {
                return getResults($http.get('/technology'));
            },
            createTech: function (newTech) {
                return getResults($http.post('/technology', newTech));
            },
            updateTech: function (tech) {
                return getResults($http.put('/technology/' + tech.Id, tech));
            },
            deleteTech: function (tech) {
                return getResults($http.delete('/technology/' + tech.Id));
            },
            updateLockStatus: function (techId, isLocked) {
                return getResults($http.put('/admin/technology/' + techId + '/lock', { IsLocked: isLocked }));
            },
            makeFavorite: function (tech) {
                return getResults($http.put('/favorites/technology', { TechnologyId: tech.Id }));
            },
            approveLogo: function(tech,status) {
                return getResults($http.put('/admin/technology/' + tech.Id + '/logo', { Approved: status }));
            },
            getPageStats: function(id) {
                return getResults($http.get('/pagestats/tech/' + id));
            },
            overview: function () {
                return getResults($http.get('/overview'));
            },
            config: function () {
                return getResults($http.get('/config'));
            },
            popularTechStacks: function () {
                var deferred = $q.defer();
                if ($rootScope.cachedPopularTechStacks) {
                    deferred.resolve($rootScope.cachedPopularTechStacks);
                } else {
                    this.overview().then(function (r) {
                        deferred.resolve($rootScope.cachedPopularTechStacks = r.PopularTechStacks);
                    });
                }
                return deferred.promise;
            },
            allTiers: function() {
                var deferred = $q.defer();
                if ($rootScope.allTiers && $rootScope.allTiers.length > 0) {
                    deferred.resolve($rootScope.allTiers);
                }
                getResults($http.get('/config')).then(function(r) {
                    deferred.resolve(r.AllTiers);
                });
                return deferred.promise;
            }
        };
    }]);
})();

