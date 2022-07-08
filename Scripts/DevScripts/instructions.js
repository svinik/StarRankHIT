// Register the plugin to all charts:
Chart.register(ChartDataLabels);
Chart.defaults.font.family = "'FontAwesome', 'Helvetica Neue', 'Helvetica', 'Arial', 'sans-serif'";

var instructionsStartTime, pageEndTime;

var dataLabels = ['\uf005\uf005\uf005\uf005\uf005', '\uf005\uf005\uf005\uf005', '\uf005\uf005\uf005', '\uf005\uf005', '\uf005'];

var ctx1;
var dataValues1 = [84, 10, 2, 1, 2];
var myChart1;

var ctx2;
var dataValues2 = [67, 18, 3, 4, 8];
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

function startInstructionsPage() {
    instructionsStartTime = getTimeStampIL(new Date());
    resetButtons();
    ctx1 = document.getElementById("myChart1").getContext('2d');
    myChart1 = new Chart(ctx1, {
        type: 'bar',
        data: {
            labels: dataLabels,
            datasets: [{
                data: dataValues1,
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
                data: dataValues2,
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

function endQuiz(option, inPreview) {
    var buttonA = document.getElementsByClassName('choose-button')[0];
    var buttonB = document.getElementsByClassName('choose-button')[1];
    buttonA.disabled = true;
    buttonB.disabled = true;

    pageEndTime = getTimeStampIL(new Date());

    if (inPreview) {
        swal({
            title: "Practice completed",
            text: "To continue, please accept the HIT",
            showConfirmButton: false,
            allowOutsideClick: false,
            closeOnClickOutside: false,
            allowEscapeKey: false,
            preConfirm: false
        }, function () {
            $.ajax({
                type: "POST",
                url: "/Instructions/InstructionsData",
                data: {
                    instructionsStartTime: instructionsStartTime,
                    pageEndTime: pageEndTime,
                    selectedOption: option
                }
            });
        });
    } else {
        swal({
            title: "Practice completed",
            text: "Start the selection task?",
            showConfirmButton: true,
            allowOutsideClick: false,
            closeOnClickOutside: false,
            allowEscapeKey: false,
            preConfirm: false
        }, function (isConfirm) {
            $.ajax({
                type: "POST",
                url: "/Instructions/InstructionsData",
                data: {
                    instructionsStartTime: instructionsStartTime,
                    pageEndTime: pageEndTime,
                    selectedOption: option
                },
                success: function () {
                    window.location.replace("/Evaluation/Evaluation"); //to prevent page back
                },
                error: function (jqXHR, exception) {
                    window.location.replace("/Home/Error?lastScreen=welcome");
                }
            });
        });
    }
    return;
}
