//If something like a button isn't implemented, have it call this to state that it isn't implemented.
function notImplemented() {
    window.alert("This item is not implemented yet :(");
}

function enableControls() {
    document.getElementById("year-edit").disabled = false;
    document.getElementById("year-edit").focus();
    document.getElementById("month-edit").disabled = false;
    document.getElementById("day-edit").disabled = false;
    document.getElementById("hour-edit").disabled = false;
    document.getElementById("minute-edit").disabled = false;
    document.getElementById("second-edit").disabled = false;
    document.getElementById("period-btn").disabled = false;
    document.getElementById("cancel-btn").disabled = false;
    document.getElementById("apply-btn").disabled = false;
}

function disableControls() {
    document.getElementById("year-edit").disabled = true;
    document.getElementById("month-edit").disabled = true;
    document.getElementById("day-edit").disabled = true;
    document.getElementById("hour-edit").disabled = true;
    document.getElementById("minute-edit").disabled = true;
    document.getElementById("second-edit").disabled = true;
    document.getElementById("period-btn").disabled = true;
    document.getElementById("cancel-btn").disabled = true;
    document.getElementById("apply-btn").disabled = true;
}