// Register the plugin to all charts:
Chart.register(ChartDataLabels);
Chart.defaults.font.family = "'FontAwesome', 'Helvetica Neue', 'Helvetica', 'Arial', 'sans-serif'";

var decisions = [];
//var finalDecisions = [];
var decisionsStr = "";
var evaluationStartTime;
var warnings = 0;
var startDate;
var current = 0;
var pairs = [];


$('[data-pair]').each(function () {
    var pairData = $(this).data('pair');
    pairs.push(pairData);
});

var i = 0;
$('[data-id]').each(function () {
    var idData = $(this).data('id');
    pairs[i]._id = idData.slice(1, -1);
    i++;
});

// convert pairs to prec
for (var pair of pairs) {
    var sum1 = pair.first.star5 + pair.first.star4 + pair.first.star3 + pair.first.star2 + pair.first.star1
    pair.first.star5 = pair.first.star5 / sum1 * 100.0
    pair.first.star4 = pair.first.star4 / sum1 * 100.0
    pair.first.star3 = pair.first.star3 / sum1 * 100.0
    pair.first.star2 = pair.first.star2 / sum1 * 100.0
    pair.first.star1 = pair.first.star1 / sum1 * 100.0

    var sum2 = pair.second.star5 + pair.second.star4 + pair.second.star3 + pair.second.star2 + pair.second.star1
    pair.second.star5 = pair.second.star5 / sum2 * 100.0
    pair.second.star4 = pair.second.star4 / sum2 * 100.0
    pair.second.star3 = pair.second.star3 / sum2 * 100.0
    pair.second.star2 = pair.second.star2 / sum2 * 100.0
    pair.second.star1 = pair.second.star1 / sum2 * 100.0

}

var dataLabels = ['\uf005\uf005\uf005\uf005\uf005', '\uf005\uf005\uf005\uf005', '\uf005\uf005\uf005', '\uf005\uf005', '\uf005'];
var ctx1;
var myChart1;
var ctx2;
var myChart2;

function resetButtons() {
    var buttonA = document.getElementsByClassName('choose-button')[0];
    var buttonB = document.getElementsByClassName('choose-button')[1];
    buttonA.disabled = true;
    buttonB.disabled = true;
    setTimeout(() => {
        buttonA.disabled = false;
        buttonB.disabled = false;
    }, 0)
}

function SetPair(pair) {
    ctx1 = document.getElementById("myChart1").getContext('2d');
    myChart1 = new Chart(ctx1, {
        type: 'bar',
        data: {
            labels: dataLabels,
            datasets: [{
                data: [pair.first.star5, pair.first.star4, pair.first.star3, pair.first.star2, pair.first.star1],
                backgroundColor: 'rgba(255, 99, 132, 1)',
            }]
        },
        options: {
            indexAxis: 'y',
            plugins: {
                tooltip: {
                    enabled: false
                },
                legend: {
                    display: false
                },
                datalabels: {
                    anchor: 'end',
                    align: 'right',
                    formatter: function (value, context) {
                        return Math.round(value) + '%';
                    },
                    font: {
                        weight: 'bold'
                    }
                }
            },
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    min: 0,
                    max: 100,
                    ticks: {
                        display: false
                    },
                    grid: {
                        display: false,
                        drawBorder: false
                    }
                },
                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
    ctx2 = document.getElementById("myChart2").getContext('2d');
    myChart2 = new Chart(ctx2, {
        type: 'bar',
        data: {
            labels: dataLabels,
            datasets: [{
                data: [pair.second.star5, pair.second.star4, pair.second.star3, pair.second.star2, pair.second.star1],
                backgroundColor: 'rgba(255, 99, 132, 1)',
            }]
        },
        options: {
            indexAxis: 'y',
            plugins: {
                tooltip: {
                    enabled: false
                },
                legend: {
                    display: false
                },
                datalabels: {
                    anchor: 'end',
                    align: 'right',
                    formatter: function (value, context) {
                        return Math.round(value) + '%';
                    },
                    font: {
                        weight: 'bold'
                    }
                }
            },
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    min: 0,
                    max: 100,
                    ticks: {
                        display: false
                    },
                    grid: {
                        display: false,
                        drawBorder: false
                    }
                },
                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}

function startEvaluationPage() {
    startDate = new Date();
    evaluationStartTime = getTimeStampIL(new Date());

    SetPair(pairs[current]);
    resetButtons();

    /*for (var index1 = 0; index1 < pairs.length; index1++) {
        finalDecisions.push({
            "index": index1,
            "option": null,
            "id": null,
            "history": "",
            "changes": -1
        });
    }*/
}

function endEvaluation() {
    evaluationEndTime = getTimeStampIL(new Date());
    swal({
        title: "Evaluation Done",
        type: "success",
        confirmButtonText: "Next",
        showConfirmButton: true
    }, function () {
        $.ajax({
            type: "POST",
            url: "/star-rank-exp/Evaluation/EvaluationData",
            data: {
                startDate: startDate,
                evaluationStartTime: evaluationStartTime,
                evaluationEndTime: evaluationEndTime,
                decisions: JSON.stringify(decisions),
                //finalDecisions: JSON.stringify(finalDecisions),
                decisionsStr: decisionsStr,
                warnings: warnings
            },
            success: function () {
                window.location.replace("/star-rank-exp/Feedback/Index"); //to prevent page back
            },
            error: function (jqXHR, exception) {
                window.location.replace("/star-rank-exp/Home/Error?lastScreen=evaluation");
            }
        });
    });
    return;
}

function checkDone() {
    for (var decision of decisions)
        if (decision.option == null)
            return false;
    return true;
}

function optionSelected(option) {
    var d = new Date();
    var time = getTimeStampIL(d);
    var index = current;
    var id = pairs[current]._id;

    var decision = {
        index: index.toString(),
        option: option,
        timestamp: time,
        date: d,
        id: id
    };

    decisions.push(decision);
    var decisionStr = "index->" + index + "," + "option->" + option+ "," + "date->" + d;
    if (decisionsStr == "") {
        decisionsStr += decisionStr;
    }
    else {
        decisionsStr += "#" + decisionStr;
    }
    // update the relevant element in rating array.
    /*if (finalDecisions[index].index != index) {
        alert("indexing error");
        return;
    }
    else {
        finalDecisions[index].option = option;
        finalDecisions[index].id= id;

        if (finalDecisions[index].history == "") {
            finalDecisions[index].history += option;
        }
        else {
            finalDecisions[index].history += "#" + option;
        }

        finalDecisions[index].changes = finalDecisions[index].changes + 1;
    }*/


    next();
}

function advanceProgressBar() {
    var total = pairs.length; //set this on initial page load
    var pcg = Math.floor(current / total * 100);
    document.getElementsByClassName('progress-bar').item(0).setAttribute('aria-valuenow', pcg);
    document.getElementsByClassName('progress-bar').item(0).setAttribute('style', 'width:' + Number(pcg) + '%');
}

function next() {
    current++;
    advanceProgressBar();
    if (current < pairs.length) {
        myChart1.destroy();
        myChart2.destroy();
        SetPair(pairs[current]);
        resetButtons();
    } else {
        endEvaluation();
    }
}