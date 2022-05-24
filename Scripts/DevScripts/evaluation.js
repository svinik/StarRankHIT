// Register the plugin to all charts:
Chart.register(ChartDataLabels);
Chart.defaults.font.family = "'FontAwesome', 'Helvetica Neue', 'Helvetica', 'Arial', 'sans-serif'";

var decisions = [];
var evaluationStartTime;
var startDate;
var current = 0;
var pairs = [];

var firstValIdx = 5
var secondValIdx = 11

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

// create validation pairs
var valPair1 = {
    first: {
        star5: 54,
        star4: 21,
        star3: 15,
        star2: 4,
        star1: 6
    },
    second: {
        star5: 7,
        star4: 0,
        star3: 3,
        star2: 18,
        star1: 82
    },
    id: "val1"
}

var valPair2 = {
    first: {
        star5: 4,
        star4: 5,
        star3: 7,
        star2: 17,
        star1: 67
    },
    second: {
        star5: 53,
        star4: 37,
        star3: 2,
        star2: 1,
        star1: 7
    },
    id: "val2"
}

pairs.splice(firstValIdx, 0, valPair1);
pairs.splice(secondValIdx, 0, valPair2);

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
    }, 4000)
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
}

function checkValidationPairs() {
    var valDecision1 = decisions[firstValIdx];
    var valDecision2 = decisions[secondValIdx];
    decisions.splice(secondValIdx, 1);
    decisions.splice(firstValIdx, 1);

    if (valDecision1.option != 'a')
        return false;
    if (valDecision2.option != 'b')
        return false;
    return true;
}

function endEvaluation() {
    var buttonA = document.getElementsByClassName('choose-button')[0];
    var buttonB = document.getElementsByClassName('choose-button')[1];
    buttonA.disabled = true;
    buttonB.disabled = true;
    var passedValidation = checkValidationPairs();
    evaluationEndTime = getTimeStampIL(new Date());
    swal({
        title: "Selection Done",
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
                passedValidation: passedValidation
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