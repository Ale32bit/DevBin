addEventListener('load', () => {
    const creationDateEl = document.getElementById("creationDate");
    const lastUpdateDateEl = document.getElementById("lastUpdateDate");
    const creationDate = document.getElementById("creationDateValue");
    const lastUpdateDate = document.getElementById("lastUpdateDateValue");

    creationDateEl.title = new Date(creationDate.value).toLocaleString();
    if(lastUpdateDateEl)
        lastUpdateDateEl.title = new Date(lastUpdateDate.value).toLocaleString();
    
    const code = document.getElementById("paste-content");
    const syntax = document.getElementById("paste-syntax");
    const worker = new Worker('js/pasteWorker.js');
    worker.onmessage = (event) => { code.innerHTML = event.data; }
    worker.postMessage({
        content: code.textContent,
        syntax: syntax.value,
    });
});