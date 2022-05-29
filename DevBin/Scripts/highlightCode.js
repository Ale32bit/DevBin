const Prism = require("./prism");
const process = require("process");
const code = process.env.CODE;
const lang = process.env.LANG;

const html = Prism.highlight(code, Prism.languages[lang], lang);
console.log(html);
process.exit(0);