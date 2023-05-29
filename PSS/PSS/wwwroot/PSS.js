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
    let element = document.getElementById(id);
    if (element == null) return;
    document.getElementById(id).classList.remove("checked");
    document.getElementById(id).classList.add("unchecked");
}

//Used in LibraryContentViewer. No idea what itemid is supposed to be used for but it stores the index in mediaList of the item.
function getItemId(id) {
    return document.getElementById(id).getAttribute('itemid')
}

function openEdit(index) {
    document.getElementById('text ' + index).style.display = 'inline';
    document.getElementById('close ' + index).style.display = 'inline';
    document.getElementById('save ' + index).style.display = 'inline';
}

function closeEdit(index) {
    document.getElementById('text ' + index).style.display = 'none';
    document.getElementById('close ' + index).style.display = 'none';
    document.getElementById('save ' + index).style.display = 'none';
}

function toggleSelect(index) {
    let select = document.getElementById('select ' + index);
    if (select.style.display === 'none') {
        select.style.display = 'inline';
    } else {
        select.style.display = 'none';
    }
}

function openSelect(index) {
    document.getElementById('select ' + index).style.display = 'inline';
}

function closeSelect(index) {
    document.getElementById('select ' + index).style.display = 'none';
}

function openDtPicker(index)
{
    document.getElementById('dtPicker ' + index).style.display = 'inline';
}

function closeDtPicker(index)
{
    document.getElementById('dtPicker ' + index).style.display = 'none';
}