// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let exposureList = document.getElementById("paste-exposure");
let encryptKey = document.getElementById("paste-key");
let encryptLabel = document.getElementById("paste-key-label");
exposureList.onchange = function onChange() {
    const value = this.value;
    if (value === "3") { // ENCRYPTED OPTION
        encryptKey.disabled = false;
        encryptKey.required = true;
        encryptKey.classList.add("border-warning");
        encryptKey.classList.remove("text-muted");
        encryptLabel.classList.remove("text-muted");
    } else {
        encryptKey.disabled = true;
        encryptKey.required = false;
        encryptKey.classList.remove("border-warning");
        encryptKey.classList.add("text-muted");
        encryptLabel.classList.add("text-muted");
    }
}

function autoSize() {
    const pasteField = document.getElementById("paste-input");
    pasteField.style.overflowY = "hidden";
    const vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);

    const lines = pasteField.value.split(/\r\n|\r|\n/).length;
    pasteField.rows = (lines);

    if (pasteField.clientHeight > vh * 0.74) {
        pasteField.style.overflowY = "auto";
    } else {
        pasteField.style.overflowY = "hidden";
    }
}

autoSize();

function validate() {
    const pasteField = document.getElementById("paste-input");
    if (exposureList.value === "3") {
        console.log("encrypting...");
        const key = encryptKey.value;
        const paste
            = pasteField.value;
        pasteField.value = encrypt(pasteRaw, key);
        encryptKey.value = ""; // for safety reasons
        document.forms[0].submit();
    } else {
        document.forms[0].submit();
    }
}

/*if (document.location.protocol !== "https:") { // SECURE CONTEXT
    let option = document.getElementById("encrypt-option");
    option.disabled = true;
    option.textContent += "?";
    option.title = "'Encrypted' option only works in secure contexts!";
    option.style.cursor = "help";
}*/

