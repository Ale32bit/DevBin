function encrypt(content, key) {
    let encrypted = CryptoJS.AES.encrypt(content, key).toString();
    return (encrypted);
}

function decrypt(content, key) {
    let decoded = (content);
    let bytes = CryptoJS.AES.decrypt(decoded, key);
    return bytes.toString(CryptoJS.enc.Utf8);
}