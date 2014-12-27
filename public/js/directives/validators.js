angular.module('games').directive('validateTimespan', function() {
    return {
        require: 'ngModel',
        link: function(scope, element, attributes, modelCtrl) {
            var regex = /^\d{1,2}:\d{2}(:\d{2})?$/;
            modelCtrl.$validators.timespan = function(modelValue, viewValue) {
                if(modelCtrl.$isEmpty(viewValue))
                    return true;
                return regex.test(viewValue);
            };
        }
    };
}).directive('validateYear', function() {
    return {
        require: 'ngModel',
        link: function(scope, element, attributes, modelCtrl) {
            var regex = /^\d+$/;
            modelCtrl.$validators.year = function(modelValue, viewValue) {
                if(modelCtrl.$isEmpty(viewValue))
                    return true;
                return regex.test(viewValue);
            };
        }
    };
});