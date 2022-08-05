const mariadb = require('mariadb');
const fs = require('fs');
const pool = mariadb.createPool(require("./dbConf.json"));
async function query(...pars) {
    let conn;
    try {
        conn = await pool.getConnection();
        return await conn.query(...pars);
    } catch (err) {
        throw err;
    } finally {
        if (conn) conn.end();
    }
}

async function main() {
    if(!fs.existsSync("./output")){
        fs.mkdirSync("./output");
    }

    if(!fs.existsSync("./input")){
        fs.mkdirSync("./input");
    }

    console.log("Extracting users...");
    let users = await query("SELECT * FROM users");
    let usersOutput = [];
    for (let i = 0; i < users.length; i++) {
        usersOutput.push(users[i]);
    }
    fs.writeFileSync("./input/users.json", JSON.stringify(usersOutput));

    // Managed by the automatic setup
    /*console.log("Extracting syntaxes...");
    let syntaxes = await query("SELECT * FROM syntaxes");
    let syntaxesOutput = [];
    for (let i = 0; i < syntaxes.length; i++) {
        syntaxesOutput.push(syntaxes[i]);
    }
    fs.writeFileSync("./input/syntaxes.json", JSON.stringify(syntaxesOutput));*/

    console.log("Extracting pastes...");
    let pastesOutput = [];
    let pastes = await query("SELECT * FROM pastes");
    for (let i = 0; i < pastes.length; i++) {
        pastesOutput.push(pastes[i]);
    }
    fs.writeFileSync("./input/pastes.json", JSON.stringify(pastesOutput));


    console.log("Run convert.js to migrate to the new format.")
    process.exit(0)
}

main();