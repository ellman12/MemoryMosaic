function toggleBackgroundScrolling() {
	document.body.style.overflow = document.body.style.overflow === 'hidden' ? '' : 'hidden';
}

function enableBackgroundScrolling() {
	document.body.style.overflow = '';
}

function disableBackgroundScrolling() {
	document.body.style.overflow = 'hidden';
}