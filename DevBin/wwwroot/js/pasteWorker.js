onmessage = (event) => {
    importScripts('/lib/highlight.pack.js');
    
    const result = self.hljs.highlight(event.data.content, {language: event.data.syntax});
    postMessage({
        result: result.value,
        lines: event.data.content.match(/\n/g)?.length ?? 1,
    });
};
