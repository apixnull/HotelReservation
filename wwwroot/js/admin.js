
/****** Displaying Alert Messages in a Smooth Way ****************/
document.addEventListener("DOMContentLoaded", function () {
    setTimeout(function () {
        document.querySelectorAll('.auto-dismiss').forEach(function (alert) {
            alert.classList.add('fade-out'); // Start fading out
            setTimeout(() => alert.remove(), 1000); // Remove after fade-out animation
        });
    }, 2300); // Show alert for 2.3 seconds before fading out
});
