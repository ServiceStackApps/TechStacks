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
                    onSelection: '&',
                    onAdd: '&',
                    onRemove: '&'
                },
                replace: true,
                link: function(scope, element) {
                    var dataDeferred = $q.defer();
                    var selectedDeferred = $q.defer();
                    scope.promises = [];
                    scope.promises.push(dataDeferred.promise);
                    scope.promises.push(selectedDeferred.promise);

                    $q.all(scope.promises).then(function() {
                        //Local state ready
                        $(element).chosen().val(scope.selectedValues);
                        $(element).chosen().trigger("chosen:updated");
                        scope.controlReady = true;
                    });
                    //One off bind
                    var initWatch = scope.$watch('data', function() {
                        $timeout(function() {
                            if (scope.data != null && scope.data.length > 0) {
                                $(element).chosen(scope.options);
                                $(element).chosen().change(function(event, item) {
                                    if (!scope.controlReady)  {
                                        return;
                                    }
                                    if (item.selected) {
                                        scope.onAdd({ item: item.selected });
                                    }
                                    if (item.deselected) {
                                        scope.onRemove({ item: item.deselected });
                                    }
                                });
                                initWatch();
                                dataDeferred.resolve();
                            }
                        });
                    });

                    scope.$watch('selectedValues', function() {
                        $timeout(function() {
                            if (scope.selectedValues) {
                                selectedDeferred.resolve();
                            }
                        });
                    });
                }
            };
        }
    ]);
})();