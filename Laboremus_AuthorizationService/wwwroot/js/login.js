// Write your JavaScript code.

$(function () {

    // toggle label-active and input-has value classes for username and password
    $('input[name="Username"]').focus(function () {
        // add label-active class and remove input-has-value class 
        $('label[for="' + this.id + '"]').removeClass('input-has-value').addClass('label-active');
    }).blur(function () {
        // add input-has-value class if the input has a value and remove labelactive class in all scenarios
        if (this.value) {
            $('label[for="' + this.id + '"]').removeClass('label-active').addClass('input-has-value');
        } else {
            $('label[for="' + this.id + '"]').removeClass('label-active');
        }
    });

    // $('#username-label').removeClass('label-active').addClass('input-has-value');
    //$('#password-label').removeClass('label-active').addClass('input-has-value');

    $('input[name="Password"]').focus(function () {
        // add label-active class and remove input-has-value class 
        $('label[for="' + this.id + '"]').removeClass('input-has-value').addClass('label-active');
    }).blur(function () {
        // add input-has-value class if the input has a value and remove labelactive class in all scenarios
        if (this.value) {
            $('label[for="' + this.id + '"]').removeClass('label-active').addClass('input-has-value');
        } else {
            $('label[for="' + this.id + '"]').removeClass('label-active');
        }
    });

    $('input[name="Username"]').on("input", function () {
        // add input-has-value class 
        $('label[for="' + this.id + '"]').addClass('input-has-value');
    });

    $('input[name="Password"]').on("input", function () {
        // add input-has-value class 
        $('label[for="' + this.id + '"]').addClass('input-has-value');
    });

    // toggle input type to either text or password by cliclking the password icon
    $('#password-icon').click(function () {
        if ($(this).hasClass("fa-eye-slash")) {
            // add fa-eye class and remove fa-eye-slash class and set the input type to text
            $(this).removeClass("fa-eye-slash").addClass("fa-eye");
            $("#Password").prop("type", "text");
        } else {
            // add fa-eye-slash class and remove fa-eye class and set the input type to password
            $(this).removeClass("fa-eye").addClass("fa-eye-slash")
            $("#Password").prop("type", "password");
        }
    });

    $('#login-form').keydown(function () {
        var key = e.which;
        if (key == 13) {
            // As ASCII code for ENTER key is "13"
            $('#login-form').submit(); // Submit form code
        }
    });

});