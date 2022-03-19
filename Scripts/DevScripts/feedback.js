var warnings = 0;
var feedbackStartTime;

function startFeedbackPage() {
    feedbackStartTime = getTimeStampIL(new Date());
}

function endFeedback() {
    var reasoning = document.getElementById("reasoning_id").value;
    var affect = document.getElementById("affect_id").value;
    var importance = document.getElementById("importance_id").value;
    var otherInfo = document.getElementById("other_info_id").value;
    var issues = document.getElementById("issues_id").value;

    if (reasoning.trim().length == 0 || affect.trim().length == 0 || importance.trim().length == 0 || otherInfo.trim().length == 0 || issues.trim().length == 0) {
        warnings++;
        swal("Please answer the questions to continue", "", "warning", {
            button: "OK",
        });
    }
    else {
        feedbackEndTime = getTimeStampIL(new Date());
        $.ajax({
            type: "POST",
            url: "/star-rank-exp/Feedback/FeedbackData",
            data: {
                feedbackStartTime: feedbackStartTime,
                feedbackEndTime: feedbackEndTime,
                reasoning: reasoning,
                affect: affect,
                importance: importance,
                otherInfo: otherInfo,
                issues: issues,
                warnings: warnings
            },
            success: function () {
                document.getElementById('feedback_id').style.display = "none";
                document.getElementById('submit_id').style.display = "block";
            },
            error: function (jqXHR, exception) {
                window.location.replace("/star-rank-exp/Home/Error?lastScreen=feedback");
            }
        });
    }
}

function redirectToProlific() {
    // TODO NEW_USER - place here the URL provided by Prolific.
    //window.location.replace("");
}