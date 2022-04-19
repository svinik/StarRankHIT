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
        url: "/Home/ConsentData/",
        data: {
            pageStartTimeClient: pageStartTime,

            agreed: hasAgreed,
            clickTimeClient: pageEndTime,
        },
        success: function (isPreview) {
            /*if (isPreview) {
                window.location.replace("/Home/Preview");
            } else*/

                if (hasAgreed) {
                    window.location.replace("/PersonalDetails/Index");
            }
            else { // disagree - thank you for your time page.
                    window.location.replace("/Home/Disagree");
            }
        },
        error: function (jqXHR, exception) {
            window.location.replace("/Home/Error?lastScreen=consent");
        }
    });
}