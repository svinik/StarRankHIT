var age = "", gender = "", country = "", cap = "", seconds = "";
var ageAll = "", genderAll = "", countryAll = "", capAll = "", secondsAll = ""; // all tries, separated by #

var mistakes = 0;
var pageStartTime;
var pageEndTime;

function startPersonalDetailsPage() {
    pageStartTime = getTimeStampIL(new Date());
}

function concatValues() { // TODO - make it cleaner!
    if (ageAll == "") {
        ageAll += age;
    }
    else {
        ageAll += "#" + age;
    }
    ///
    if (genderAll == "") {
        genderAll += gender;
    }
    else {
        genderAll += "#" + gender;
    }
    ///
    if (countryAll == "") {
        countryAll += country;
    }
    else {
        countryAll += "#" + country;
    }
    ///
    if (capAll == "") {
        capAll += cap;
    }
    else {
        capAll += "#" + cap;
    }
    ///
    if (secondsAll == "") {
        secondsAll += seconds;
    }
    else {
        secondsAll += "#" + seconds;
    }
}

function endPersonalDetails() {
    // fill in answers.
    age = document.getElementById('age_id').value;
    gender = document.getElementById('gender_id').value;
    country = document.getElementById('country_id').value;
    cap = document.getElementById('cap_id').value;
    seconds = document.getElementById('seconds_id').value;

    parsedAge = parseInt(age)

    if (parsedAge != NaN && parsedAge >= 18 && parsedAge <= 120 && gender != "" && country != "" && (cap == "chen" || cap == "AHi5b#L8") && seconds == "90") {
        concatValues();
        pageEndTime = getTimeStampIL(new Date());

        $.ajax({
            type: "POST",
            url: "/star-rank-exp/PersonalDetails/PersonalDetailsData",
            data: {
                pageStartTimeClient: pageStartTime,
                clickTimeClient: pageEndTime,

                ageAll: ageAll,
                genderAll: genderAll,
                countryAll: countryAll,
                capAll: capAll,
                secondsAll: secondsAll,

                age: age,
                gender: gender,
                country: country,
                cap: cap,
                seconds: seconds,

                mistakes: mistakes,
            },
            success: function () {
                window.location.replace("/star-rank-exp/Instructions/Index");
            },
            error: function (jqXHR, exception) {
                window.location.replace("/star-rank-exp/Home/Error?lastScreen=consent");
            }
        });
    }
    else {
        mistakes++;
        concatValues();

        swal("Please make sure that all the fields are completed correctly.", "", "warning", {
            button: "OK",
        });
    }
}