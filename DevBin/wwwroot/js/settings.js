function deleteAccount(form) {
    if (!confirm("Are you really sure you want to delete your account?\nThis operation is irreversible!")) return;

    form.submit();
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