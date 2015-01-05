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
            getStack: function (id) {
                return getResults($http.get('/techstacks/' + id));
            },
            getStackPreviousVersions: function (id) {
                return getResults($http.get('/techstacks/' + id + '/previous-versions'));
            },
            getTechStackFavorites: function (id) {
                return getResults($http.get('/techstacks/' + id + '/favorites'));
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
            updateLockStatus: function(techStackId, isLocked) {
                return getResults($http.put('/admin/techstacks/' + techStackId + '/lock', { IsLocked: isLocked }));
            },
            searchStacks: function (searchQuery) {
                return getResults($http.get('/techstacks/search?orderBy=-LastModified&NameContains=' + searchQuery + "&DescriptionContains=" + searchQuery));
            },
            overview: techServices.overview,
            searchTech: techServices.searchTech
        };
    }]);
})();

