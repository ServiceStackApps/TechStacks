'use strict';
var allTechsResponse = { "Techs": [{ "Id": 1, "Name": "ServiceStack", "VendorName": "ServiceStack", "Description": "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!", "Tiers": ["App", "Web", "Client"] }, { "Id": 2, "Name": "IIS", "VendorName": "Microsoft", "Description": "Microsoft's web host", "Tiers": ["Web"] }, { "Id": 3, "Name": "RavenDB", "VendorName": "RavenDB", "Description": "Open source 2nd generation document DB", "Tiers": ["Data"] }, { "Id": 4, "Name": "PostgreSQL", "VendorName": "PostgreSQL", "Description": "The world's most advanced open source database.", "Tiers": ["Data"] }] };
var specificTechResponse = { "Tech": { "Id": 1, "Name": "ServiceStack", "VendorName": "ServiceStack", "Description": "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!", "Tiers": ["App", "Web", "Client"] } };
describe('tech controllers unit tests', function() {
    var $scope,
        $httpBackend,
        allTechsRequestHandler,
        specificTechRequestHandler,
        createLatestTechCtrl,
        createTechCtrl;


    beforeEach(function() {
        module('techs.controllers');
        inject(function($rootScope, $injector) {
            var $controller = $injector.get('$controller');
            $scope = $rootScope.$new();
            $httpBackend = $injector.get('$httpBackend');
            allTechsRequestHandler = $httpBackend.when('GET', '/techs')
                .respond(allTechsResponse);
            specificTechRequestHandler = $httpBackend.when('GET', '/techs/1')
                .respond(specificTechResponse);
            createLatestTechCtrl = function() {
                return $controller('latestTechsCtrl', { $scope: $scope });
            };
            createTechCtrl = function () {
                return $controller('techCtrl', { $scope: $scope, $routeParams: { techId: 1 } });
            };
        });
    });

    it('should have 4 techs on the scope at "techs"', function() {
        var myCtrl1 = createLatestTechCtrl();
        $httpBackend.flush();
        expect(myCtrl1).toBeDefined();
        expect($scope.techs).toBeDefined();
        expect($scope.techs.length).toBe(4);
    }); 

    it('should have specific tech on scope with Id of 1', function () {
        var myCtrl1 = createTechCtrl();
        $httpBackend.flush();
        expect(myCtrl1).toBeDefined();
        expect($scope.tech).toBeDefined();
        expect($scope.tech.Id).toBe(1);
    });
});