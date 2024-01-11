let video; //HTMLVideoElement
let seekSlider; //HTMLInputElement
let controls;
let playButtonIcon;
let fullscreenButtonIcon;
let volumeIcon;
let volume = 1;
let muted = false;
let volumeSlider;
let currentTimeSpan, durationSpan;
let info, previousInfoDisplay;
let seekSliderInterval;
let hideControlsTimeout;

function cleanupVideo() {
	video.src = "";
	video.muted = true;
	video.disable();

	clearInterval(seekSliderInterval);

	video.onplaying = null;
	video.onpause = null;
	video.oncontextmenu = null;
	video.onclick = null;
	video.ondblclick = null;
	document.onfullscreenchange = null;
	video.onkeydown = null;
	seekSlider.oninput = null;
	volumeSlider.oninput = null;
	document.onmousemove = null;
}

async function initializeVideo() {
	await delay(500);
	
	video = document.querySelector("video");
	seekSlider = document.querySelector("input[type='range']");
	controls = document.getElementById("controls");
	playButtonIcon = controls.firstElementChild.firstElementChild;
	fullscreenButtonIcon = controls.lastElementChild.firstElementChild;
	volumeIcon = document.getElementById("mute-btn").firstElementChild;
	volumeSlider = document.getElementById("volume-slider");
	info = document.getElementById("info");
	currentTimeSpan = document.getElementById("currentTime");
	durationSpan = document.getElementById("duration");
	previousInfoDisplay = "";
	
	playButtonIcon.innerHTML = "pause";

	seekSlider.max = video.duration;
	
	currentTimeSpan.innerHTML = formatSeconds(0);
	durationSpan.innerHTML = formatSeconds(video.duration);

	initializeEvents();
}

function initializeEvents() {
	clearInterval(seekSliderInterval);
	seekSliderInterval = setInterval(() => { seekSlider.value = video.currentTime; currentTimeSpan.innerHTML = formatSeconds(video.currentTime); }, 5);
	
	video.onplaying = () => playButtonIcon.innerHTML = "pause";
	video.onpause = () => playButtonIcon.innerHTML = "play_arrow";

	video.oncontextmenu = e => e.preventDefault();

	video.onclick = () => togglePlaying();

	video.ondblclick = () => toggleFullscreen();

	document.onfullscreenchange = () => {
		if (document.fullscreenElement)
			fullscreenButtonIcon.innerHTML = "fullscreen_exit";
		else
			fullscreenButtonIcon.innerHTML = "fullscreen";
	};

	video.onkeydown = e => {
		switch (e.key) {
			case " ":
			case "k":
				togglePlaying();
				break;

			case "ArrowLeft":
				setCurrentTime(video.currentTime -= 5);
				break;

			case "ArrowRight":
				setCurrentTime(video.currentTime += 5);
				break;

			case "j":
				setCurrentTime(video.currentTime -= 10);
				break;

			case "l":
				setCurrentTime(video.currentTime += 10);
				break;

			case "ArrowUp":
				volumeSlider.value = parseFloat(volumeSlider.value) + 0.1;
				video.volume = volume = volumeSlider.value;
				break;

			case "ArrowDown":
				volumeSlider.value = parseFloat(volumeSlider.value) - 0.1;
				video.volume = volume = volumeSlider.value;
				break;

			case "f":
				toggleFullscreen();
				break;

			case "m":
				toggleMute();
				break;

			case "Home":
				showControls();
				play();
				setCurrentTime(0);
				break;

			case "End":
				showControls();
				pause();
				setCurrentTime(video.duration);
				break;
		}
	}

	seekSlider.oninput = () => video.currentTime = seekSlider.value;

	volumeSlider.oninput = () => {
		video.volume = volume = volumeSlider.value;
		muted = volumeSlider.value <= 0;
		setVolumeIcon();
	}

	document.onmousemove = () => {
		showControls();
		delayHideControls();
	}
}

//Dumb hack to ensure getElementsByTagName() actually finds the video element.
function delay(ms) {
	return new Promise(resolve => setTimeout(resolve, ms));
}

function formatSeconds(seconds) {
	let minutes = Math.floor(seconds / 60);
	let remainingSeconds = Math.round(seconds % 60);

	let formattedMinutes = String(minutes).padStart(1, '0');
	let formattedSeconds = String(remainingSeconds).padStart(2, '0');

	return `${formattedMinutes}:${formattedSeconds}`;
}

function showControls() {
	clearTimeout(hideControlsTimeout);
	controls.style.display = "flex";
}

function delayHideControls() {
	hideControlsTimeout = setTimeout(() => controls.style.display = "none", 1500)
}

function play() {
	video.play();
	playButtonIcon.innerHTML = "pause"
	seekSliderInterval = setInterval(() => { seekSlider.value = video.currentTime; currentTimeSpan.innerHTML = formatSeconds(video.currentTime); }, 5);
	delayHideControls();
}

function pause() {
	video.pause();
	playButtonIcon.innerHTML = "play_arrow"
	clearInterval(seekSliderInterval);
	showControls();
}

function togglePlaying() {
	if (video.paused)
		play();
	else
		pause();
}

function setCurrentTime(value) { seekSlider.value = video.currentTime = value; }

function enterFullscreen() {
	const elem = document.documentElement;
	
	previousInfoDisplay = info.style.display;
	info.style.display = "none";

	if (elem.requestFullscreen)	elem.requestFullscreen();
	else if (elem.webkitRequestFullscreen) elem.webkitRequestFullscreen();
	else if (elem.mozRequestFullScreen) elem.mozRequestFullScreen();
	else if (elem.msRequestFullscreen) elem.msRequestFullscreen();
}

function exitFullscreen() {

	info.style.display = previousInfoDisplay;
	
	if (document.exitFullscreen) document.exitFullscreen();
	else if (document.webkitExitFullscreen) document.webkitExitFullscreen();
	else if (document.mozCancelFullScreen) document.mozCancelFullScreen();
	else if (document.msExitFullscreen) document.msExitFullscreen();
}

function toggleFullscreen() {
	const isInFullscreen = document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement;
	
	if (isInFullscreen)	exitFullscreen();
	else enterFullscreen();
}

function setVolumeIcon() {
	if (volumeSlider.value <= 0)
		volumeIcon.innerHTML = "volume_off";
	else if (volumeSlider.value <= 0.45)
		volumeIcon.innerHTML = "volume_down";
	else
		volumeIcon.innerHTML = "volume_up";
}

function toggleMute() {
	muted = !muted;
	
	if (muted)
		video.volume = volumeSlider.value = 0;
	else if (volume <= 0)
		video.volume = volume = volumeSlider.value = 1;
	else
		video.volume = volumeSlider.value = volume;
	
	setVolumeIcon();
}