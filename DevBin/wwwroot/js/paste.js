function highlight() {
    const syntax = document.getElementById("paste-syntax");
    const code = document.getElementById("paste-content");

    const lines = code.innerText.match(/\n/g).length ?? 1;

    let syntaxId = syntax.textContent;

    try {
        syntax.textContent = hljs.getLanguage(syntaxId).name;

        const worker = new Worker('/js/hl-worker.js');
        worker.onmessage = (event) => {
            code.innerHTML = event.data;

            if (lines <= 2048) {
                displayLines();
            }
        }
        worker.postMessage({
            code: code.innerText,
            language: syntaxId,
            //element: code,
        });

        //hljs.highlightElement(code);

    } catch (e) {
        console.error(e);
        syntax.textContent = syntaxId;
    }
}

function displayLines() {
    const code = document.getElementById("paste-content");
    hljs.lineNumbersBlock(code);
}

function displayLinesAnyway(btn) {
    btn.disabled = true;
    displayLines();
    closeAlert();
}

function closeAlert() {
    let alert = document.getElementById("too-long-alert");
    alert.classList.remove("show");
}


addEventListener("load", highlight);
