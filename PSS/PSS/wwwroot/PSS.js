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

//Used in LibraryContentViewer. No idea what itemid is supposed to be used for but it stores the index in mediaList of the item.
function getItemId(id)
{
    return document.getElementById(id).getAttribute('itemid')
}

//Used in Import.razor
function openEdit(index)
{
    document.getElementById('text ' + index).style.display = 'inline';
    document.getElementById('edit ' + index).style.display = 'none'
    document.getElementById('close ' + index).style.display = 'inline'
    document.getElementById('save ' + index).style.display = 'inline'
}

//Used in Import.razor
function closeEdit(index)
{
    document.getElementById('text ' + index).style.display = 'none';
    document.getElementById('edit ' + index).style.display = 'inline'
    document.getElementById('close ' + index).style.display = 'none'
    document.getElementById('save ' + index).style.display = 'none'
}