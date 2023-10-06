//TODO: find a not dumb way to only include this file in VideoPlayer, or LCV/Import.

//Dumb hack to ensure getElementsByTagName() actually finds the video element.
function delay(ms) {
	return new Promise(resolve => setTimeout(resolve, ms));
}

let video; //HTMLVideoElement

window.onload = async () => {
	await delay(40);
	video = document.getElementsByTagName("video")[0];

	video.oncontextmenu = e => e.preventDefault()

	video.onclick = () => {
		togglePlaying()
	}

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
}

function togglePlaying() {
	if (video.paused)
		video.play();
	else
		video.pause();
}
