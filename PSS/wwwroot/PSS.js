function toggleCheck(id) {
    let className;
    if (document.getElementById(id).classList.contains("unchecked")) {
        document.getElementById(id).classList.remove("unchecked");
        document.getElementById(id).classList.add("checked");
        className = "checked";

    } else if (document.getElementById(id).classList.contains("checked")) {
        document.getElementById(id).classList.remove("checked");
        document.getElementById(id).classList.add("unchecked");
        className = "unchecked";
    }
    
    return className;
}

//Used in ClearChecks()
function removeCheck(id) {
    document.getElementById(id).classList.remove("checked");
    document.getElementById(id).classList.add("unchecked");
}