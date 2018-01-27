function updateInventorization() {
    apiGet("/api/inventorization/state?args=hello",
        function (response) {
            console.log("inventorization:");
            console.log(response);
            if (response.balance == 0) return;
            $("#inventorizationBalance").html(response.balance);
            $("#inventorizationBlock").removeClass("hidden");
        });
}