function showPassword(id) {
    let passwordField = document.getElementById(id);
    passwordField.type = "text";
}

function hidePassword(id) {
    let passwordField = document.getElementById(id);
    passwordField.type = "password";
}

function copyToClipboard(el) {
    let field = document.getElementById(el);
    field.focus();
    field.select();
    document.execCommand("copy");
}