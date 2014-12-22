/* global angular */
(function () {
    "use strict";
    var app = angular.module('stacks.services', ['tech.services']);

    app.service('techStackServices', ['$http', '$q', 'techServices',
        function ($http, $q, techServices) {

        var getResults = techServices.getResults;

        return {
            createStack: function (newStack) {
                return getResults($http.post('/techstacks', newStack));
            },
            getStack: function (id, reload) {
                var url = '/techstacks/' + id;
                if (reload) url += '?Reload=true';
                return getResults($http.get(url));
            },
            latestTechStacks: function () {
                return getResults($http.get('/techstacks/latest'));
            },
            allTechs: function () {
                return getResults($http.get('/technology/search'));
            },
            updateStack: function (techStack) {
                return getResults($http.put('/techstacks/' + techStack.Id, techStack));
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

