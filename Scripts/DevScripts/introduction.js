function EndInstructions() {
    window.location.replace("/star-rank-exp/Consent/Index");
}

function submitHIT() {
    $.ajax({
        type: "POST",
        url: "/star-rank-exp/Feedback/SubmitToMturk",
        data: {},
        success: function (response) {
            window.location.replace(response);
        },
        error: function (jqXHR, exception) {
            window.location.replace("/star-rank-exp/Home/Error?lastScreen=feedback");
        }
    });
}