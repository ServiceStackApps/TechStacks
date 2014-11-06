var app = angular.module('stacks.filters', []);

app.filter('stack', function() {
    return function (stack, tier) {
        if (stack) {
            var tiers = stack.Tiers;
            return tiers.indexOf(tier) != -1;
        }
        return false;
    }
});