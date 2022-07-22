var pageStartTime;
var pageStartTimeParticipant;

var pageEndTime;

function startConsentPage() {
    var date = new Date();
    pageStartTime = getTimeStampIL(date);
}

function EndConsent(hasAgreed) {
    pageEndTime = getTimeStampIL(new Date());

    $.ajax({
        type: "POST",
        url: "/star-rank-exp/Home/ConsentData/",
        data: {
            pageStartTimeClient: pageStartTime,

            agreed: hasAgreed,
            clickTimeClient: pageEndTime,
        },
        success: function () {
            if (hasAgreed) {
                window.location.replace("/star-rank-exp/PersonalDetails/Index");
            }
            else { // disagree - thank you for your time page.
                window.location.replace("/star-rank-exp/Home/Disagree");
            }
        },
        error: function (jqXHR, exception) {
            window.location.replace("/star-rank-exp/Home/Error?lastScreen=consent");
        }
    });
}