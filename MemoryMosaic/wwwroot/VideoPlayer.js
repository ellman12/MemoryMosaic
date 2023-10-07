//TODO: find a not dumb way to only include this file in VideoPlayer, or LCV/Import.

//Dumb hack to ensure getElementsByTagName() actually finds the video element.
function delay(ms) {
	return new Promise(resolve => setTimeout(resolve, ms));
}

let video; //HTMLVideoElement
let slider; //HTMLInputElement
let interval;

window.onload = async () => {
	await delay(400);
	video = document.querySelector("video");
	slider = document.querySelector("input[type='range']");

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

	video.addEventListener('play', () => interval = setInterval(syncSlider, 50));
	function syncSlider() { slider.value = video.currentTime; }

	video.addEventListener('pause', () => clearInterval(interval));
}

function togglePlaying() {
	if (video.paused)
		video.play();
	else
		video.pause();
}
