/* global angular,$ */
(function () {
    "use strict";

    var app = angular.module('chosen', []);

    app.directive('chosenSelect', [
        '$timeout', '$q', function($timeout, $q) {
            return {
                restrict: 'E',
                template: '<select ng-show="data" multiple class="chosen"><option ng-repeat="item in data" value="{{item[itemValue]}}">{{item[itemName]}}</option></select>',
                scope: {
                    data: '=',
                    itemValue: '@',
                    itemName: '@',
                    options: '=',
                    selectedValues: '=',
                    onSelection: '&'
                },
                replace: true,
                link: function(scope, element) {
                    var dataDeferred = $q.defer();

                    //One off bind
                    var initWatch = scope.$watch('data', function() {
                        if (scope.data != null) {
                            $timeout(function() {

                                $(element).chosen(scope.options);
                                $(element).chosen().change(function(event, item) {
                                    if (!scope.controlReady) {
                                        return;
                                    }

                                    if (item.selected) {
                                        scope.selectedValues.push(parseInt(item.selected));
                                    }
                                    if (item.deselected) {
                                        var existingPos = scope.selectedValues.indexOf(parseInt(item.deselected));
                                        if (existingPos >= 0) {
                                            scope.selectedValues.splice(existingPos, 1);
                                        }
                                    }

                                    setValues(scope.selectedValues);
                                });
                                initWatch();
                                dataDeferred.resolve();
                            });
                        }

                    });

                    function setValues(selectedValues) {
                        $(element).chosen().val(selectedValues);
                        $(element).chosen().trigger("chosen:updated");
                    }

                    dataDeferred.promise.then(function () {
                        setValues(scope.selectedValues);
                        scope.controlReady = true;
                    });

                    scope.$watch('selectedValues', function() {
                        $timeout(function () {
                            if (scope.controlReady) {
                                setValues(scope.selectedValues);
                            }
                        });
                    });
                }
            };
        }
    ]);
    
})();