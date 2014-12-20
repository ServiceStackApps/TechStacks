/* global angular */
(function () {
    "use strict";
    var app = angular.module('stacks.services', ['tech.services']);

    app.service('techStackServices', ['$http', '$q', 'techServices', function ($http, $q, techServices) {

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
            createStack: function (newStack) {
                return getResults($http.post('/techstacks', newStack));
            },
            getStack: function (id) {
                return getResults($http.get('/techstacks/' + id));
            },
            latestTechStacks: function () {
                return getResults($http.get('/techstacks/latest'));
            },
            allTechs: function () {
                return getResults($http.get('/technology/search'));
            },
            updateStack: function (techStack) {
                var deferred = $q.defer();
                $http.put('/techstacks/' + techStack.Id, techStack)
                    .success(function (response) {
                        techStack.Name = response.TechStack.Name;
                        techStack.Description = response.TechStack.Description;
                        techStack.Details = response.TechStack.Details;
                        deferred.resolve(techStack);
                    }).error(function(error) {
                    deferred.reject(error.ResponseStatus.Message);
                });
                return deferred.promise;
            },
            deleteTechStack: function (techStack) {
                return getResults($http.delete('/techstacks/' + techStack.Id));
            },
            updateTechnologyChoice: function (technologyChoice) {
                return getResults($http.put('/techchoices/' + technologyChoice.Id, technologyChoice));
            },
            updateLockStatus: function(techStackId, isLocked) {
                return getResults($http.put('/admin/techstacks/' + techStackId + '/lock', { IsLocked: isLocked }));
            },
            addTechChoice: function (techChoice) {
                return getResults($http.post('/techchoices', techChoice));
            },
            removeTechChoice: function (techChoice) {
                return getResults($http.delete('/techchoices/' + techChoice.Id));
            },
            searchStacks: function (searchQuery) {
                return getResults($http.get('/techstacks/search?NameContains=' + searchQuery + "&DescriptionContains=" + searchQuery));
            },
            overview: techServices.overview,
            searchTech: techServices.searchTech
        };
    }]);
})();

