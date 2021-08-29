onmessage = (event) => {
    importScripts("/lib/highlight.js/highlight.min.js");
    const result = self.hljs.highlight(event.data.code, {
        language: event.data.language
    });

    postMessage(result.value);
};