import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const websiteDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repositoryDirectory = path.resolve(websiteDirectory, '..');
const data = JSON.parse(fs.readFileSync(path.join(websiteDirectory, 'data', 'ser-truth-table.json'), 'utf8'));
const target = path.join(repositoryDirectory, 'ser_method_info.js');

if (!fs.existsSync(target)) {
  fs.writeFileSync(target, `const SER_TRUTH_TABLE = ${JSON.stringify(data, null, 2)};\n\nmodule.exports = { SER_TRUTH_TABLE };\n`);
  console.log('Materialized the SER manifest for tooling generation.');
}
