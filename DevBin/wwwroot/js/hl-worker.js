/*let highlighted = false

onmessage = (event) => {
    if (highlighted) return;
    highlighted = true
    console.log("Highlight...");
    console.log(event.data);
    importScripts('/js/highlight.pack.js');
    const result = hljs.hightlight(event.data.content, {
        language: event.data.syntax
    });
    postMessage(result.value);

}
*/
onmessage = (event) => {
    importScripts("/lib/highlight.js/highlight.min.js");
    const result = self.hljs.highlight(event.data.code, {
        language: event.data.language
    });

    //result.value = hljs.lineNumbersValue(result.value)

    postMessage(result.value);
};