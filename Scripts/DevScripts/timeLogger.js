
function getTimeStampIL(date) {
    //dd.MM.yyyy HH:mm:ss.SSS

    var d = date;
    d.setHours(d.getHours() + 3);

    var day = d.getUTCDate().toString();
    var month = (d.getUTCMonth() + 1).toString();
    var year = d.getUTCFullYear().toString();

    var hour = d.getUTCHours().toString();
    var minutes = d.getUTCMinutes().toString();
    var seconds = d.getUTCSeconds().toString();
    var milliseconds = d.getUTCMilliseconds().toString();

    // month
    if (month.length < 2)
        month = '0' + month;
    // day
    if (day.length < 2)
        day = '0' + day;
    // hour
    if (hour.length < 2)
        hour = '0' + hour;
    // minutes
    if (minutes.length < 2)
        minutes = '0' + minutes;
    // seconds
    if (seconds.length < 2)
        seconds = '0' + seconds;
    // milliseconds
    if (milliseconds.length == 1)
        milliseconds = '00' + milliseconds;
    else if (milliseconds.length == 2)
        milliseconds = '0' + milliseconds;

    return day + "." + month + "." + year + " " + hour + ":" + minutes + ":" + seconds + ":" + milliseconds;
}