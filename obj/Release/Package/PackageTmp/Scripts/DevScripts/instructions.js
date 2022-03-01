// Register the plugin to all charts:
Chart.register(ChartDataLabels);

var instructionsStartTime, pageEndTime;

var dataLabels = ['5 star', '4 star', '3 star', '2 star', '1 star'];

var ctx1;
var dataValues1 = [84, 10, 2, 1, 2];
var myChart1;

var ctx2;
var dataValues2 = [67, 18, 3, 4, 8];
var myChart2;

function startInstructionsPage() {
    instructionsStartTime = getTimeStampIL(new Date());
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

function endQuiz(option) {
    pageEndTime = getTimeStampIL(new Date());
    swal({
        title: "Good job!",
        text: "Start the evaluation task?",
        type: "success",
        showConfirmButton: true
    }, function () {
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
    return;
}
