onmessage = (event) => {
    importScripts('/lib/highlight.pack.js');
    let result;
    if(!event.data.auto) {
        result = self.hljs.highlight(event.data.content, {language: event.data.syntax});
    } else {
        result = self.hljs.highlightAuto(event.data.content)
    }
    postMessage({
        result: result.value,
        syntax: result.language,
        lines: event.data.content.match(/\n/g)?.length ?? 1,
    });
};
