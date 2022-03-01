var decisions = [];
var startTimeRatingPage;
var msStart;

function startReplay() {
    startTimeRatingPage = document.getElementById("start_time_ratings_id").value;
    decisions_arr_str = document.getElementById("decisions_arr_str_id").value;

    var splitted = decisions_arr_str.split("#");
    var indexStart;
    for (indexStart = 0; indexStart < splitted.length; indexStart++) {
        var commaSplitted = splitted[indexStart].split(",");
        var index = commaSplitted[0].split("->")[1];
        var rating = commaSplitted[1].split("->")[1];
        var date = commaSplitted[2].split("->")[1];

        decisions.push({
            "index": index,
            "rating": rating,
            "date": date,
        });
    }
    replay();
}


function clearRatings() {
    var i;
    // clean
    for (i = 1; i <= 12; i++) {
        var name = "star-" + i;
        document.getElementsByName(name)[0].checked = false;
        document.getElementsByName(name)[1].checked = false;
        document.getElementsByName(name)[2].checked = false;
        document.getElementsByName(name)[3].checked = false;
        document.getElementsByName(name)[4].checked = false;
    }
}

function presentNextRating(i) {
    document.getElementById("replay-star-" + decisions[i].index + "-" + decisions[i].rating).checked = true;

    if (i != (decisions.length - 1)) {
        var end = new Date(decisions[i + 1].date);
        var start = new Date(decisions[i].date);
        var difference = end.getTime() - start.getTime();

        setTimeout(function () {
            presentNextRating(i + 1);
        }, difference);
    }
}

function replay() {
    setTimeout(function () {
        presentNextRating(0);
    }, 1000);
}