let exposureList = document.getElementById("paste-exposure");
let encryptKey = document.getElementById("paste-key");
let encryptLabel = document.getElementById("paste-key-label");
exposureList.onchange = function onChange() {
    const value = this.value;
    if (value === "4") { // ENCRYPTED OPTION
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

autoSize();

function validate() {
    const pasteField = document.getElementById("paste-input");
    if (exposureList.value === "4") {
        console.log("encrypting...");
        pasteField.value = CryptoJS.AES.encrypt(pasteField.value, encryptKey.value).toString();
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