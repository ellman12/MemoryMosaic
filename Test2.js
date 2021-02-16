/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
// let sidebarToggled: boolean = false;
sidebarToggled = true;

function toggleNav() {

    if (sidebarToggled == true) {
        document.getElementById("mySidebar").style.width = "250px";
        document.getElementById("main").style.marginLeft = "250px";
        sidebarToggled = false;
    } else {
        document.getElementById("mySidebar").style.width = "0";
        document.getElementById("main").style.marginLeft = "0";
        sidebarToggled = true;
    }
}