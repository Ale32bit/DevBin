function onSubmit(token) {
    document.getElementById("captcha-token").value = token;
    let form = $('#paste-form');
    form.submit();
}

function validate() {
    event.preventDefault();
    let form = $('#paste-form');
    if (!form[0].checkValidity()) {
        $('<input type="submit">').hide().appendTo(form).click().remove();
    } else {
        hcaptcha.execute()
    }
}