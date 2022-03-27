function showPassword(id) {
    let passwordField = document.getElementById(id);
    passwordField.type = "text";
}

function hidePassword(id) {
    let passwordField = document.getElementById(id);
    passwordField.type = "password";
}