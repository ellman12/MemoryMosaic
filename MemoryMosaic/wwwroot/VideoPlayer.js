//TODO: find a not dumb way to only include this file in VideoPlayer, or LCV/Import.

//Dumb hack to ensure getElementsByTagName() actually finds the video element.
function delay(ms) {
	return new Promise(resolve => setTimeout(resolve, ms));
}

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

	video.onkeydown = e => {
		console.log(e.key)

		switch (e.key) {
			case " ":
			case "k":
				togglePlaying();
				break;

			case "ArrowLeft":
				video.currentTime -= 5;
				break;

			case "ArrowRight":
				video.currentTime += 5;
				break;
				
			case "j":
				video.currentTime -= 10;
				break;
			
			case "l":
				video.currentTime += 10;
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
}

function togglePlaying() {
	if (video.paused) {
		video.play();
		playButtonIcon.innerHTML = "pause"
		interval = setInterval(() => seekSlider.value = video.currentTime, 50);
	} else {
		video.pause();
		playButtonIcon.innerHTML = "play_arrow"
		clearInterval(interval);
	}
}

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