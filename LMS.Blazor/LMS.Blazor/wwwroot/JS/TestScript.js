function showAlert(message) {
    alert(message);
}


const notificationButton = document.querySelector(".lms-bell");
const list = document.getElementById("notificationsList");
// list.addEventListener(click, "closeList");

function closeList() {
    if (notificationButton != undefined) {
        console.log("Got notificationButton");

        console.log(
            !notificationButton.hasAttribute("open")
        )

        notificationButton.removeAttribute('open');
        console.log(
            !notificationButton.hasAttribute("open")
        )

        notificationButton.open = false;
        console.log(
            !notificationButton.hasAttribute("open")
        )
    }

    // if (list != undefined) {
    //     console.log("Got list");

    // }

}