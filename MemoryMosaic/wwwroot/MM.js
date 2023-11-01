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

function toggleBackgroundScrolling()
{
    document.body.style.overflow = document.body.style.overflow === 'hidden' ? '' : 'hidden';
}

function enableBackgroundScrolling()
{
    document.body.style.overflow = '';
}

function disableBackgroundScrolling()
{
    document.body.style.overflow = 'hidden';
}