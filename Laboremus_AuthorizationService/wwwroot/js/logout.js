// Write your JavaScript code.

$(function () {
    $('#back-button').click(function (e) {
        e.preventDefault();
        window.history.back();
    });
});