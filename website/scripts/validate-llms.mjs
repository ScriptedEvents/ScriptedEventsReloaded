import assert from 'node:assert/strict';
import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const websiteDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const buildDirectory = path.join(websiteDirectory, 'build');
const basePath = '/ScriptedEventsReloaded/';
const publicBaseUrl = `https://scriptedevents.github.io${basePath}`;
const indexFilename = path.join(buildDirectory, 'llms.txt');
const fullFilename = path.join(buildDirectory, 'llms-full.txt');
const truthTableFilename = path.join(buildDirectory, 'data', 'ser-truth-table.json');

for (const filename of [indexFilename, fullFilename, truthTableFilename]) {
  assert.ok(fs.existsSync(filename), `Missing generated artifact: ${filename}`);
}

const index = fs.readFileSync(indexFilename, 'utf8');
const full = fs.readFileSync(fullFilename, 'utf8');
const links = [...index.matchAll(/\[[^\]]*(?:\\\][^\]]*)*\]\((https:\/\/[^)]+)\)/g)]
  .map(match => match[1]);
const markdownLinks = links.filter(link => link.startsWith(`${publicBaseUrl}llms/`));

function collectMarkdownFiles(directory, result) {
  for (const entry of fs.readdirSync(directory, {withFileTypes: true})) {
    const filename = path.join(directory, entry.name);
    if (entry.isDirectory()) collectMarkdownFiles(filename, result);
    else if (entry.isFile() && entry.name.endsWith('.md')) result.push(filename);
  }
}

function slug(value) {
  return String(value)
    .toLocaleLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');
}

const generatedDocsDirectory = path.join(websiteDirectory, '.site-docs');
const generatedDocFiles = [];
const markdownAssetFiles = [];
collectMarkdownFiles(generatedDocsDirectory, generatedDocFiles);
collectMarkdownFiles(path.join(buildDirectory, 'llms'), markdownAssetFiles);
assert.equal(markdownLinks.length, markdownAssetFiles.length, 'llms.txt must link every Markdown asset exactly once.');
assert.ok(markdownLinks.every(link => link.endsWith('.md')), 'Every downloadable Markdown URL must end in .md.');
assert.ok(links.every(link => link.startsWith(publicBaseUrl)), 'Every llms.txt URL must respect the GitHub Pages base path.');

const canonicalSourceUrls = [...full.matchAll(/^Canonical source URL: \[[^\]]+\]\((https:\/\/[^)]+)\)$/gm)]
  .map(match => match[1]);
const fullMarkdownUrls = [...full.matchAll(/^Markdown source: \[[^\]]+\]\((https:\/\/[^)]+)\)$/gm)]
  .map(match => match[1]);
assert.equal(canonicalSourceUrls.length, markdownAssetFiles.length, 'Every llms-full.txt page needs a canonical source URL.');
assert.ok(canonicalSourceUrls.every(link => link.startsWith(publicBaseUrl)), 'Every canonical source URL must respect the base path.');
assert.equal(fullMarkdownUrls.length, markdownAssetFiles.length, 'Every llms-full.txt page needs a Markdown source URL.');
assert.ok(fullMarkdownUrls.every(link => link.startsWith(`${publicBaseUrl}llms/`) && link.endsWith('.md')));

for (const link of markdownLinks) {
  const relativeUrl = link.slice(publicBaseUrl.length);
  const target = path.join(buildDirectory, ...decodeURIComponent(relativeUrl).split('/'));
  assert.ok(fs.existsSync(target), `llms.txt target is missing from the build: ${link}`);
}

for (const filename of generatedDocFiles) {
  const relativeFilename = path.relative(generatedDocsDirectory, filename);
  assert.ok(fs.existsSync(path.join(buildDirectory, 'llms', relativeFilename)), `Generated page is absent from llms.txt: ${relativeFilename}`);
}
assert.ok(fs.existsSync(path.join(buildDirectory, 'llms', 'SUMMARY.md')), 'docs/SUMMARY.md is absent from the AI corpus.');

const exampleSourceDirectory = path.join(websiteDirectory, '..', 'Example Scripts');
for (const entry of fs.readdirSync(exampleSourceDirectory, {recursive: true, withFileTypes: true})) {
  if (!entry.isFile() || !/\.(?:ser|txt)$/i.test(entry.name)) continue;
  const examplePath = path.relative(exampleSourceDirectory, path.join(entry.parentPath, entry.name)).replaceAll('\\', '/');
  const exampleName = examplePath.replace(/\.(?:ser|txt)$/i, '');
  const generatedPage = path.join(buildDirectory, 'llms', 'examples', `${slug(exampleName)}.md`);
  assert.ok(fs.existsSync(generatedPage), `Build-validated example is absent from the AI corpus: ${examplePath}`);
}

const sourceFenceCount = markdownAssetFiles.reduce((count, filename) => {
  const markdown = fs.readFileSync(filename, 'utf8').replaceAll('\r\n', '\n');
  return count + (markdown.match(/^```ser(?:\s|$)/gm)?.length || 0);
}, 0);
const fullFenceCount = full.match(/^```ser(?:\s|$)/gm)?.length || 0;
assert.ok(sourceFenceCount > 0, 'The generated documentation must contain fenced SER examples.');
assert.equal(fullFenceCount, sourceFenceCount, 'llms-full.txt must preserve every fenced SER example.');
JSON.parse(fs.readFileSync(truthTableFilename, 'utf8'));

const server = http.createServer((request, response) => {
  const pathname = decodeURIComponent(new URL(request.url, 'http://127.0.0.1').pathname);
  if (!pathname.startsWith(basePath)) {
    response.writeHead(404).end();
    return;
  }

  const relativeFilename = pathname.slice(basePath.length);
  const target = path.resolve(buildDirectory, ...relativeFilename.split('/'));
  const relativeTarget = path.relative(buildDirectory, target);
  if (relativeTarget.startsWith('..') || path.isAbsolute(relativeTarget) || !fs.statSync(target, {throwIfNoEntry: false})?.isFile()) {
    response.writeHead(404).end();
    return;
  }

  response.writeHead(200);
  response.end(fs.readFileSync(target));
});

await new Promise((resolve, reject) => {
  server.once('error', reject);
  server.listen(0, '127.0.0.1', resolve);
});

try {
  const address = server.address();
  assert.ok(address && typeof address === 'object');
  const localBaseUrl = `http://127.0.0.1:${address.port}${basePath}`;
  const pending = [...links];
  const failures = [];
  const workers = Array.from({length: 24}, async () => {
    while (pending.length) {
      const publicUrl = pending.pop();
      const response = await fetch(publicUrl.replace(publicBaseUrl, localBaseUrl));
      if (response.status !== 200) failures.push(`${response.status} ${publicUrl}`);
      await response.arrayBuffer();
    }
  });
  await Promise.all(workers);
  assert.deepEqual(failures, [], `Some llms.txt links did not return HTTP 200:\n${failures.join('\n')}`);
} finally {
  await new Promise((resolve, reject) => server.close(error => error ? reject(error) : resolve()));
}

console.log(
  `Validated ${links.length} HTTP 200 links (${markdownLinks.length} Markdown pages) and ${fullFenceCount} fenced SER examples.`,
);
