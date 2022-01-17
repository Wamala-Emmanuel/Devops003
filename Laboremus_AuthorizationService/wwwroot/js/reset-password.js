// Write your JavaScript code.

$(function () {

    // toggle label-active and input-has value classes for username and password
    $('input[name="Email"]').focus(function () {
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

    $('input[name="Password"]').focus(function () {
        // add label-active class and remove input-has-value class 
        $('label[for="' + this.id + '"]').removeClass('input-has-value').addClass('label-active password-label-active');
    }).blur(function () {
        // add input-has-value class if the input has a value and remove labelactive class in all scenarios
        if (this.value) {
            $('label[for="' + this.id + '"]').removeClass('label-active').addClass('input-has-value');
        } else {
            $('label[for="' + this.id + '"]').removeClass('label-active password-label-active');
        }
    });

    $('input[name="ConfirmPassword"]').focus(function () {
        // add label-active class and remove input-has-value class 
        $('label[for="' + this.id + '"]').removeClass('input-has-value').addClass('label-active reset-password-label-active');
    }).blur(function () {
        // add input-has-value class if the input has a value and remove labelactive class in all scenarios
        if (this.value) {
            $('label[for="' + this.id + '"]').removeClass('label-active').addClass('input-has-value');
        } else {
            $('label[for="' + this.id + '"]').removeClass('label-active reset-password-label-active');
        }
    });

    $('input[name="Email"]').on("input", function () {
        // add input-has-value class 
        $('label[for="' + this.id + '"]').addClass('input-has-value');
    });

    $('input[name="Password"]').on("input", function () {
        // add input-has-value class 
        $('label[for="' + this.id + '"]').addClass('input-has-value');
    });

    $('input[name="ConfirmPassword"]').on("input", function () {
        // add input-has-value class 
        $('label[for="' + this.id + '"]').addClass('input-has-value');
    });

    $('#reset-password-form').keydown(function () {
        var key = e.which;
        if (key == 13) {
            // As ASCII code for ENTER key is "13"
            $('#reset-password-form').submit(); // Submit form code
        }
    });

});