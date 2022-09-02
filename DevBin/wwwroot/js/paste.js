const codeBlock = document.getElementById("paste-content");

function displayLineNumbers() {
    hljs.lineNumbersBlock(codeBlock);
}

function closeAlert() {
    let alert = document.getElementById("too-long-alert");
    alert.classList.remove("show");
}

function displayLinesAnyway(btn) {
    btn.disabled = true;
    displayLineNumbers();
    closeAlert();
}

addEventListener('load', () => {
    hljs.initLineNumbersOnLoad();
    const creationDateEl = document.getElementById("creationDate");
    const lastUpdateDateEl = document.getElementById("lastUpdateDate");
    const creationDate = document.getElementById("creationDateValue");
    const lastUpdateDate = document.getElementById("lastUpdateDateValue");

    creationDateEl.title = new Date(creationDate.value).toLocaleString();
    if(lastUpdateDateEl)
        lastUpdateDateEl.title = new Date(lastUpdateDate.value).toLocaleString();
    
    const syntax = document.getElementById("paste-syntax");
    const worker = new Worker('js/pasteWorker.js');
    worker.onmessage = (event) => { 
        codeBlock.innerHTML = event.data.result;
        if(syntax.value === "auto") {
            let syntaxDisplay = document.getElementById("syntax-display");
            let lang = hljs.getLanguage(event.data.syntax);
            syntaxDisplay.innerText = ": " + lang.name;
        }
        if (event.data.lines <= 4096) {
            displayLineNumbers();
        }
    }
    worker.postMessage({
        content: codeBlock.textContent,
        syntax: syntax.value,
        auto: syntax.value === "auto"
    });
});

