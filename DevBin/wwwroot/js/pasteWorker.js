onmessage = (event) => {
    importScripts('/lib/highlight.pack.js');
    importScripts("/lib/highlightjs-line-numbers.js/highlightjs-line-numbers.min.js")
    const result = self.hljs.highlight(event.data.content, {language: event.data.syntax});
    postMessage(result.value);
};
