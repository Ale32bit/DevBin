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

let apiKeyButton = document.getElementById("copy-api-key");
let popoverClicked = new bootstrap.Popover(apiKeyButton, {
    content: "Copied!",
    placement: "bottom",
    animation: false,
});

let popoverClick = new bootstrap.Popover(apiKeyButton, {
    content: "Click to copy.",
    placement: "bottom",
});

function copyHover() {
    popoverClick.show();
}

function copyClick() {
    copyToClipboard("api-key");
    popoverClicked.show();
    popoverClick.hide();
}

function copyLeave() {
    popoverClicked.hide();
    popoverClick.hide();
}