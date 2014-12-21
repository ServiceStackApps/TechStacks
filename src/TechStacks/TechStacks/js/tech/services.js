(function () {
    "use strict";
    var app = angular.module('tech.services', []);

    app.service('techServices', ['$http', '$q', function ($http, $q) {

        function getResults(promise) {
            var deferred = $q.defer();
            promise
                .success(function (response) {
                    deferred.resolve(response.Result || response.Results || response);
                })
                .error(function (e) {
                    deferred.reject((e && e.ResponseStatus && e.ResponseStatus.Message) || e);
                });;
            return deferred.promise;
        }

        return {
            getTech: function(id) {
                return getResults($http.get('/technology/' + id));
            },
            searchTech: function (searchQuery) {
                return getResults($http.get('/technology/search?NameContains=' + searchQuery + "&DescriptionContains=" + searchQuery));
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
            updateTechnologyChoice: function (technologyChoice) {
                return getResults($http.put('/techchoices/' + technologyChoice.Id, technologyChoice));
            },
            deleteTech: function (tech) {
                return getResults($http.delete('/technology/' + tech.Id));
            },
            updateLockStatus: function (techId, isLocked) {
                return getResults($http.put('/admin/technology/' + techId + '/lock', { IsLocked: isLocked }));
            },
            removeTechChoice: function (techChoice) {
                return getResults($http.delete('/techchoices/' + techChoice.Id));
            },
            makeFavorite: function (tech) {
                return getResults($http.put('/favorites/technology', { TechnologyId: tech.Id }));
            },
            approveLogo: function(tech,status) {
                return getResults($http.put('/admin/technology/' + tech.Id + '/logo', { Approved: status }));
            },
            overview: function () {
                return getResults($http.get('/overview'));
            },
            config: function () {
                return getResults($http.get('/config'));
            }
        };
    }]);
})();

