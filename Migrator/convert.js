const fs = require("fs")
const mysql = require("mysql");

function dateToSqlFormat(d) {
    return d.toISOString().slice(0, 19).replace('T', ' ');
}

function dateStringToSqlFormat(d) {
    return d.slice(0, 19).replace('T', ' ');
}

function ipToString(ip) {
    if(!ip) return "0.0.0.0";
    return ip.data.join(".");
}

function decodeToUTF8(str) {
    return decodeURIComponent(str.replace(/^0x/, '').replace(/[0-9a-f]{2}/g, '%$&'));
}

function passwordToString(password) {
    return String.fromCharCode(...password.data);
}

console.log("Migrating users and API Tokens...")
const users = require("./input/users.json")

let usersQuery = "INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, LegacyPassword) VALUES ";
let tokensQuery = "INSERT INTO ApiTokens (Name, Token, CreatedAt, OwnerId, AllowGet, AllowCreate, AllowUpdate, AllowDelete, AllowGetUser, AllowCreateFolders, AllowDeleteFolders) VALUES ";

for (let i = 0; i < users.length; i++) {
    let user = users[i];
    usersQuery += `\n( ${user.id}, ${mysql.escape(user.username)}, ${mysql.escape(user.username.toUpperCase())}, ${mysql.escape(user.email)}, ${mysql.escape(user.email.toUpperCase())}, ${user.verified}, 0, 0, 1, 0, '${passwordToString(user.password).replace(/\u0000/g, "0")}' ),`;
    if (user.apiToken !== null && user.apiToken != undefined) {
        tokensQuery += `\n('Migrated from v2', '${user.apiToken}', '${dateToSqlFormat(new Date())}', ${user.id}, 1, 1, 1, 1, 1, 0, 0),`;
    }
}

fs.writeFileSync("./output/users.sql", usersQuery.substring(0, usersQuery.length - 1) + ";");
fs.writeFileSync("./output/tokens.sql", tokensQuery.substring(0, tokensQuery.length - 1) + ";");

// Managed by the automatic setup
/*console.log("Migrating syntaxes...");
const syntaxes = require("./input/syntaxes.json");

let syntaxesQuery = "INSERT INTO Syntaxes (Name, DisplayName, IsHidden) VALUES ";

let rSyntax = [];

for (let i = 0; i < syntaxes.length; i++) {
    let syntax = syntaxes[i];
    syntaxesQuery += `\n('${syntax.name}', '${syntax.pretty}', ${syntax.show == "1" ? 0 : 1}),`;
    rSyntax.push(syntax.name);
}
fs.writeFileSync("./output/syntaxes.sql", syntaxesQuery.substring(0, syntaxesQuery.length - 1) + ";");

let exposuresQuery = `INSERT INTO Exposures (Id, Name, IsListed, IsAuthorOnly) VALUES
(1, 'Public', 1, 0),
(2, 'Unlisted', 0, 0),
(3, 'Private', 0, 1);`;
fs.writeFileSync("./output/exposures.sql", exposuresQuery);*/

console.log("Migrating pastes...");
const pastes = require("./input/pastes.json");

let pastesQuery = "";
for (let i = 0; i < pastes.length; i++) {
    let paste = pastes[i];
    if (paste.exposureId == "4") paste.exposureId = "2";
    if (paste.authorId == "0") paste.authorId = null;
    if (paste.updateDatetime != null)
        paste.updateDatetime = dateStringToSqlFormat(paste.updateDatetime);
    paste.datetime = dateStringToSqlFormat(paste.datetime);

    pastesQuery += `\nINSERT INTO Pastes (Code, Title, Views, DateTime, UpdateDateTime, Cache, Content, UploaderIPAddress, SyntaxName, ExposureId, AuthorId) VALUE ('${paste.code}', ${mysql.escape(paste.title)}, ${paste.views}, '${paste.datetime}', ${paste.updateDatetime != null ? `'${paste.updateDatetime}'` : 'NULL'}, ${mysql.escape(paste.cache)}, ${mysql.escape(paste.content)}, '${ipToString(paste.ipAddress)}', '${rSyntax[parseInt(paste.syntaxId)]}', ${paste.exposureId}, ${paste.authorId});`;
}

fs.writeFileSync("./output/pastes.sql", pastesQuery.substring(0, pastesQuery.length - 1) + ";");

let fullOutput = usersQuery.substring(0, usersQuery.length - 1) + ";\n"
    + tokensQuery.substring(0, tokensQuery.length - 1) + ";\n"
    //+ syntaxesQuery.substring(0, syntaxesQuery.length - 1) + ";\n"
    //+ exposuresQuery + "\n"
    + pastesQuery.substring(0, pastesQuery.length - 1) + ";\n";
fs.writeFileSync("./output/full.sql", fullOutput);

console.log("Migrated all data to the new format!")
console.log("Import the output/full.sql file to the new database!")