function onSubmit(token) {
    let form = $('#paste-form');
    if (!form[0].checkValidity()) {
        form.find(':submit').click();
    } else {
        document.getElementById("captcha-token").value = token;
        form.submit();
    }
}