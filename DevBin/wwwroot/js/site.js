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

/* Legacy code
function autoSize() {
    const pasteField = document.getElementById("paste-input");
    pasteField.style.overflowY = "hidden";
    const vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);

    const lines = pasteField.value.split(/\r\n|\r|\n/).length;
    pasteField.rows = lines;

    if (pasteField.clientHeight > vh * 0.74) {
        pasteField.style.overflowY = "auto";
        pasteField.scrollTop = pasteField.scrollHeight;
    } else {
        pasteField.style.overflowY = "hidden";
    }
}*/