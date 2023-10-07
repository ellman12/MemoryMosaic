//TODO: find a not dumb way to only include this file in VideoPlayer, or LCV/Import.

//Dumb hack to ensure getElementsByTagName() actually finds the video element.
function delay(ms) {
	return new Promise(resolve => setTimeout(resolve, ms));
}

let video; //HTMLVideoElement
let slider; //HTMLInputElement
let controls;
let playButtonIcon;
let interval;

window.onload = async () => {
	await delay(400);
	video = document.querySelector("video");
	slider = document.querySelector("input[type='range']");
	controls = document.getElementById("controls");
	playButtonIcon = controls.firstElementChild.firstElementChild;

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
		}
	}

	//Another dumb hack
	await delay(440);
	slider.max = video.duration;

	slider.oninput = () => video.currentTime = slider.value;
}

function togglePlaying() {
	if (video.paused) {
		video.play();
		playButtonIcon.innerHTML = "pause"
		interval = setInterval(() => slider.value = video.currentTime, 50);
	}
	else {
		video.pause();
		playButtonIcon.innerHTML = "play_arrow"
		clearInterval(interval);
	}
}
