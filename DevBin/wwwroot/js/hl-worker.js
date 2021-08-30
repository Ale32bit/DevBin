onmessage = (event) => {
    importScripts("/lib/highlight.pack.js");
    const result = self.hljs.highlight(event.data.code, {
        language: event.data.language
    });

    postMessage(result.value);
};