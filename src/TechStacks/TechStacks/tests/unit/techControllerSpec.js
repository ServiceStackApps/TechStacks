var allTechsResponse = { "Techs": [{ "Id": 1, "Name": "ServiceStack", "VendorName": "ServiceStack", "Description": "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!", "Tiers": ["App", "Web", "Client"] }, { "Id": 2, "Name": "IIS", "VendorName": "Microsoft", "Description": "Microsoft's web host", "Tiers": ["Web"] }, { "Id": 3, "Name": "RavenDB", "VendorName": "RavenDB", "Description": "Open source 2nd generation document DB", "Tiers": ["Data"] }, { "Id": 4, "Name": "PostgreSQL", "VendorName": "PostgreSQL", "Description": "The world's most advanced open source database.", "Tiers": ["Data"] }] };
var specificTechResponse = { "Tech": { "Id": 1, "Name": "ServiceStack", "VendorName": "ServiceStack", "Description": "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!", "Tiers": ["App", "Web", "Client"] } };
var latestTechStacks = { "TechStacks": [{ "TechnologyChoices": [{ "TechnologyId": 1, "TechnologyStackId": 1, "Tier": "Web", "Id": 1, "Name": "ServiceStack", "VendorName": "ServiceStack", "LogoUrl": "https://github.com/ServiceStack/Assets/raw/master/img/artwork/fulllogo-280.png", "Description": "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "layoric", "OwnerId": "1", "Tiers": ["App", "Web", "Client"] }, { "TechnologyId": 4, "TechnologyStackId": 1, "Tier": "Data", "Id": 2, "Name": "PostgreSQL", "VendorName": "PostgreSQL", "LogoUrl": "http://www.myintervals.com/blog/wp-content/uploads/2011/12/postgresql-logo1.png", "Description": "The world's most advanced open source database.", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "ExampleUser", "OwnerId": "1", "Tiers": ["Data"] }, { "TechnologyId": 3, "TechnologyStackId": 1, "Tier": "Data", "Id": 3, "Name": "RavenDB", "VendorName": "RavenDB", "LogoUrl": "http://static.ravendb.net/logo-for-nuget.png", "Description": "Open source 2nd generation document DB", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "ExampleUser", "OwnerId": "1", "Tiers": ["Data"] }, { "TechnologyId": 2, "TechnologyStackId": 1, "Tier": "Web", "Id": 4, "Name": "IIS", "VendorName": "Microsoft", "LogoUrl": "http://www.microsoft.com/web/media/gallery/apps-screenshots/Microsoft-App-Request-Routing.png", "Description": "Microsoft's web host", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "ExampleUser", "OwnerId": "1", "Tiers": ["Web"] }], "Id": 1, "Name": "Initial Stack", "Description": "Example stack11", "CreatedBy": "ExampleUser", "LastModifiedBy": "layoric", "LastModified": "2014-12-17T16:42:03.9192566", "Created": "0001-01-01T00:00:00.0000000", "OwnerId": "1", "Details": "asdasd" }] }
var searchTechs = { "Offset": 0, "Total": 4, "Results": [{ "Id": 1, "Name": "ServiceStack", "VendorName": "ServiceStack", "LogoUrl": "https://github.com/ServiceStack/Assets/raw/master/img/artwork/fulllogo-280.png", "Description": "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "layoric", "OwnerId": "1", "Tiers": ["App", "Web", "Client"] }, { "Id": 2, "Name": "IIS", "VendorName": "Microsoft", "LogoUrl": "http://www.microsoft.com/web/media/gallery/apps-screenshots/Microsoft-App-Request-Routing.png", "Description": "Microsoft's web host", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "ExampleUser", "OwnerId": "1", "Tiers": ["Web"] }, { "Id": 3, "Name": "RavenDB", "VendorName": "RavenDB", "LogoUrl": "http://static.ravendb.net/logo-for-nuget.png", "Description": "Open source 2nd generation document DB", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "ExampleUser", "OwnerId": "1", "Tiers": ["Data"] }, { "Id": 4, "Name": "PostgreSQL", "VendorName": "PostgreSQL", "LogoUrl": "http://www.myintervals.com/blog/wp-content/uploads/2011/12/postgresql-logo1.png", "Description": "The world's most advanced open source database.", "LogoApproved": false, "CreatedBy": "ExampleUser", "LastModifiedBy": "ExampleUser", "OwnerId": "1", "Tiers": ["Data"] }] };

describe('tech controllers unit tests', function () {
    "use strict";
    var $scope,
        $httpBackend,
        allTechsRequestHandler,
        specificTechRequestHandler,
        createLatestTechCtrl,
        latestTechStacksRequestHandler,
        searchTechsRequestHandler,
        createTechCtrl;
    var techStacksRequestHandler;

    beforeEach(function(done) {
        module('testMod');
        inject(function($rootScope, $injector) {
            var $controller = $injector.get('$controller');
            $scope = $rootScope.$new();
            $httpBackend = $injector.get('$httpBackend');
            allTechsRequestHandler = $httpBackend.when('GET', '/technology')
                .respond(allTechsResponse);
            specificTechRequestHandler = $httpBackend.when('GET', '/technology/1')
                .respond(specificTechResponse);
            latestTechStacksRequestHandler = $httpBackend.when('GET', '/techstacks/latest').respond(latestTechStacks);
            searchTechsRequestHandler = $httpBackend.when('GET', '/technology/search?NameContains=&DescriptionContains=')
                .respond(searchTechs);
            techStacksRequestHandler = $httpBackend.when('GET', '/technology/1/techstacks').respond(latestTechStacks);
            createLatestTechCtrl = function() {
                return $controller('latestTechsCtrl', { $scope: $scope });
            };
            createTechCtrl = function () {

                return $controller('techCtrl', { $scope: $scope, $routeParams: { techId: 1 } });
            };
            done();
        });
    });

    afterEach(function() {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
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