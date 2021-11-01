onmessage = (event) => {
    importScripts("/lib/highlight.pack.js");
    if (event.data.autodetect) {
        const result = self.hljs.highlightAuto(event.data.code);
        postMessage(result.value);
    } else {
        const result = self.hljs.highlight(event.data.code, {
            language: event.data.language
        });
        postMessage(result.value);
    }
};