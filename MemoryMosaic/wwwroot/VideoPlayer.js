let video; //HTMLVideoElement
let seekSlider; //HTMLInputElement
let controls;
let playButtonIcon;
let fullscreenButtonIcon;
let volumeIcon;
let volume = 1;
let muted = false;
let volumeSlider;
let interval;
let timeout;

window.onload = async () => {
	await delay(400);
	video = document.querySelector("video");
	seekSlider = document.querySelector("input[type='range']");
	controls = document.getElementById("controls");
	playButtonIcon = controls.firstElementChild.firstElementChild;
	fullscreenButtonIcon = controls.lastElementChild.firstElementChild;
	volumeIcon = document.getElementById("mute-btn").firstElementChild;
	volumeSlider = document.getElementById("volume-slider");

	video.oncontextmenu = e => e.preventDefault();

	video.onclick = () => togglePlaying();

	video.ondblclick = () => toggleFullscreen();
	
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

	//Another dumb hack
	await delay(440);
	seekSlider.max = video.duration;

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

function showControls() {
	clearTimeout(timeout);
	controls.style.display = "flex";
}

function delayHideControls() {
	timeout = setTimeout(() => controls.style.display = "none", 1500)
}

function play() {
	video.play();
	playButtonIcon.innerHTML = "pause"
	interval = setInterval(() => seekSlider.value = video.currentTime, 50);
	delayHideControls();
}

function pause() {
	video.pause();
	playButtonIcon.innerHTML = "play_arrow"
	clearInterval(interval);
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

	if (elem.requestFullscreen)	elem.requestFullscreen();
	else if (elem.webkitRequestFullscreen) elem.webkitRequestFullscreen();
	else if (elem.mozRequestFullScreen) elem.mozRequestFullScreen();
	else if (elem.msRequestFullscreen) elem.msRequestFullscreen();

	fullscreenButtonIcon.innerHTML = "fullscreen_exit";
}

function exitFullscreen() {
	if (document.exitFullscreen) document.exitFullscreen();
	else if (document.webkitExitFullscreen) document.webkitExitFullscreen();
	else if (document.mozCancelFullScreen) document.mozCancelFullScreen();
	else if (document.msExitFullscreen) document.msExitFullscreen();

	fullscreenButtonIcon.innerHTML = "fullscreen";
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