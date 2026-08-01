import fs from 'node:fs';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath} from 'node:url';

const websiteDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repositoryDirectory = path.resolve(websiteDirectory, '..');
const manifestFilename = path.join(repositoryDirectory, 'ser_method_info.js');
const outputFilename = path.join(websiteDirectory, 'data', 'ser-truth-table.json');

if (!fs.existsSync(manifestFilename)) {
  throw new Error('Missing ser_method_info.js. Build SER before synchronizing the website data.');
}

const require = createRequire(import.meta.url);
delete require.cache[require.resolve(manifestFilename)];
const manifest = require(manifestFilename).SER_TRUTH_TABLE;
fs.mkdirSync(path.dirname(outputFilename), {recursive: true});
fs.writeFileSync(outputFilename, `${JSON.stringify(manifest, null, 2)}\n`);
console.log(`Synchronized website data from SER schema v${manifest.schemaVersion}.`);
