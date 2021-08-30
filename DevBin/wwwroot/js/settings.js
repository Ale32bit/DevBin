function copyToClipboard(el) {
    let field = document.getElementById(el);
    field.focus();
    field.select();
    document.execCommand("copy");
}

function deleteAccount(form) {
    let password = document.getElementById("deletion-password");

    if (!confirm("Are you really sure you want to delete your account?\nThis operation is irreversible!")) return;

    form.submit();
}

function showPassword(id) {
    let passwordField = document.getElementById(id);
    passwordField.type = "text";
}

function hidePassword(id) {
    let passwordField = document.getElementById(id);
    passwordField.type = "password";
}