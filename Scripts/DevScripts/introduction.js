function EndInstructions() {
    window.location.replace("/star-rank-exp/Consent/Index");
}

function submitHIT(assignmentId) {
    var url = "https://workersandbox.mturk.com/mturk/externalSubmit?assignmentId=" + assignmentId;
    window.location.replace(url);
}