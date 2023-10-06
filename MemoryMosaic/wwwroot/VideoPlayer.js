//TODO: find a not dumb way to only include this file in VideoPlayer, or LCV/Import.

//Dumb hack to ensure getElementsByTagName() actually finds the video element.
function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

let video; //HTMLVideoElement

window.onload = async () => {
    await delay(40);
    video = document.getElementsByTagName("video")[0];

    video.addEventListener("contextmenu", e => e.preventDefault());

    video.addEventListener("click", () => {
        if (video.paused)
            video.play();
        else
            video.pause();
    });
}
