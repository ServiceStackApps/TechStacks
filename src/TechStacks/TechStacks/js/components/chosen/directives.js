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
                                        scope.onAdd({ item: item.selected });
                                    }
                                    if (item.deselected) {
                                        scope.onRemove({ item: item.deselected });
                                    }
                                });
                                initWatch();
                                dataDeferred.resolve();
                            });
                        }

                    });

                    dataDeferred.promise.then(function () {
                        $(element).chosen().val(scope.selectedValues);
                        $(element).chosen().trigger("chosen:updated");
                        scope.controlReady = true;
                    });

                    scope.$watch('selectedValues', function() {
                        $timeout(function () {
                            if (scope.controlReady) {
                                $(element).chosen().val(scope.selectedValues);
                                $(element).chosen().trigger("chosen:updated");
                            }
                        });
                    });
                }
            };
        }
    ])
    .directive('chosen', function () {
        return {
            restrict: 'A',
            scope: {
                source: '='
            },
            link: function (scope, el) {
                scope.$watch('source', function () {
                    el.trigger('chosen:updated');
                });
                el.chosen();
            }
        };
    });
    
})();