/* global angular */
(function () {
    "use strict";
    var app = angular.module('stacks.services', ['tech.services']);

    app.service('techStackServices', ['$http', '$q', 'techServices', function ($http, $q, techServices) {
        return {
            createStack: function (newStack) {
                var deferred = $q.defer();
                $http.post('/stacks', newStack).success(function (response) {
                    deferred.resolve(response.TechStack);
                });
                return deferred.promise;
            },
            getStack: function (id) {
                var deferred = $q.defer();
                $http.get('/stacks/' + id)
                    .success(function (response) {
                        deferred.resolve(response.TechStack);
                    });
                return deferred.promise;
            },
            latestTechStacks: function () {
                var deferred = $q.defer();
                $http.get('/stacks/latest')
                    .success(function (response) {
                        deferred.resolve(response.TechStacks);
                    });
                return deferred.promise;
            },
            allTechs: function () {
                var deferred = $q.defer();
                $http.get('/techs/search')
                    .success(function (response) {
                        deferred.resolve(response.Results);
                    });
                return deferred.promise;
            },
            updateStack: function (techStack) {
                var deferred = $q.defer();
                $http.put('/stacks/' + techStack.Id, techStack)
                    .success(function (response) {
                        techStack.Name = response.TechStack.Name;
                        techStack.Description = response.TechStack.Description;
                        techStack.Details = response.TechStack.Details;
                        deferred.resolve(techStack);
                    });
                return deferred.promise;
            },
            deleteTechStack: function (techStack) {
                return $http.delete('/stacks/' + techStack.Id);
            },
            updateTechnologyChoice: function (technologyChoice) {
                var deferred = $q.defer();
                $http.put('/techchoices/' + technologyChoice.Id, technologyChoice)
                    .success(function (response) {
                        deferred.resolve(response.TechnologyChoice);
                    })
                    .error(function (error) {
                        deferred.reject(error);
                    });
                return deferred.promise;
            },
            updateLockStatus: function(techStackId, isLocked) {
                var deferred = $q.defer();
                $http.put('/admin/stacks/' + techStackId + '/lock', {IsLocked:isLocked})
                    .success(function (response) {
                        deferred.resolve();
                    })
                    .error(function (error) {
                        deferred.reject(error);
                    });
                return deferred.promise;
            },
            addTechChoice: function (techChoice) {
                var deferred = $q.defer();
                $http.post('/techchoices', techChoice)
                    .success(function (response) {
                        deferred.resolve(response.TechnologyChoice);
                    });
                return deferred.promise;
            },
            removeTechChoice: function (techChoice) {
                var deferred = $q.defer();
                $http.delete('/techchoices/' + techChoice.Id)
                    .success(function (response) {
                        deferred.resolve(response.TechStack);
                    });
                return deferred.promise;
            },
            searchStacks: function (searchQuery) {
                var deferred = $q.defer();
                $http.get('/stacks/search/?NameContains=' + searchQuery + "&DescriptionContains=" + searchQuery)
                    .success(function (response) {
                        deferred.resolve(response.Results);
                    });
                return deferred.promise;
            },
            trendingStacks: function() {
                var deferred = $q.defer();
                $http.get('/stacks/trending')
                    .success(function (response) {
                        deferred.resolve(response);
                    });
                return deferred.promise;
            },
            searchTech: techServices.searchTech,
            allTiers: techServices.allTiers
        };
    }]);
})();

