var app = angular.module('app', [])
    .controller('receipt', function ($scope) {

        $scope.recompute = function (product, roommate) {
            console.log(product);
            console.log(roommate);
            conseol.log($scope.checked);
        }

    });

var recompute = function (product, roommate) {

    checked = document.getElementsByName("[" + product + "].SplitDecoded[" + roommate + "]")[0].checked;
    
    checkoutElement = document.getElementById("checkout" + roommate);
    checkoutElement.innerText = checked;

}