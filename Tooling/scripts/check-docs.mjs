import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { readGeneratedManifest } from "./manifest.mjs";

const toolingDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repositoryDirectory = path.resolve(toolingDirectory, "..");
const documentationDirectory = path.join(repositoryDirectory, "docs");
const summaryFilename = path.join(documentationDirectory, "SUMMARY.md");
const manifest = readGeneratedManifest(path.join(repositoryDirectory, "ser_method_info.js"));

assert.ok(fs.existsSync(documentationDirectory), "Missing docs directory.");
assert.ok(fs.existsSync(summaryFilename), "Missing docs/SUMMARY.md.");

function collectMarkdownFiles(directory) {
  return fs.readdirSync(directory, { withFileTypes: true })
    .flatMap(entry => {
      const filename = path.join(directory, entry.name);
      return entry.isDirectory() ? collectMarkdownFiles(filename) : [filename];
    })
    .filter(filename => filename.endsWith(".md"));
}

function normalizeRelative(filename) {
  return path.relative(documentationDirectory, filename).replaceAll("\\", "/");
}

function localMarkdownLinks(content) {
  const links = [];
  const pattern = /!?\[[^\]]*]\(([^)]+)\)/g;
  let match;

  while ((match = pattern.exec(content)) !== null) {
    let target = match[1].trim();
    if (target.startsWith("<") && target.endsWith(">")) {
      target = target.slice(1, -1);
    }

    if (/^(?:https?:|mailto:|#)/i.test(target)) {
      continue;
    }

    target = target.split("#", 1)[0].split("?", 1)[0];
    if (target) {
      links.push(decodeURIComponent(target));
    }
  }

  return links;
}

const markdownFiles = collectMarkdownFiles(documentationDirectory);
const summaryContent = fs.readFileSync(summaryFilename, "utf8");
const summaryTargets = new Set(
  localMarkdownLinks(summaryContent)
    .map(target => normalizeRelative(path.resolve(documentationDirectory, target)))
);

for (const filename of markdownFiles) {
  const content = fs.readFileSync(filename, "utf8");
  const relativeFilename = normalizeRelative(filename);

  for (const target of localMarkdownLinks(content)) {
    const resolvedTarget = path.resolve(path.dirname(filename), target);
    assert.ok(
      fs.existsSync(resolvedTarget),
      `${relativeFilename} links to a missing file: ${target}`
    );
  }

  if (relativeFilename !== "SUMMARY.md") {
    assert.ok(
      summaryTargets.has(relativeFilename),
      `${relativeFilename} is not listed in docs/SUMMARY.md.`
    );
  }
}

const combinedDocumentation = markdownFiles
  .map(filename => fs.readFileSync(filename, "utf8"))
  .join("\n");

for (const stalePattern of [
  "scriptedeventsreloaded.gitbook.io",
  "{% hint",
  "OnEvent Died",
  "LimitPlayers",
  "RemovePlayers"
]) {
  assert.ok(
    !combinedDocumentation.includes(stalePattern),
    `Documentation still contains stale GitBook content: ${stalePattern}`
  );
}

for (const requiredText of [
  "serhelp start",
  "serstatus",
  "serreload",
  ".txt",
  "globally unique",
  "targeted refresh"
]) {
  assert.ok(
    combinedDocumentation.toLowerCase().includes(requiredText.toLowerCase()),
    `Documentation is missing the required 1.0 topic: ${requiredText}`
  );
}

for (const match of combinedDocumentation.matchAll(
  /```ser\s*\n# requires ([^\n]+)\n([\s\S]*?)```/gi
)) {
  const framework = match[1].trim();
  const methods = [...match[2].matchAll(/^\s*([A-Z][A-Za-z0-9_.]*)\b/gm)]
    .map(methodMatch => methodMatch[1])
    .filter(methodName => !["OnPMER", "OnUCR"].includes(methodName));

  assert.ok(methods.length > 0, `The ${framework} example contains no methods to validate.`);
  for (const methodName of methods) {
    assert.ok(
      Object.hasOwn(manifest.methods, methodName),
      `The ${framework} example references an unknown method: ${methodName}`
    );
  }
}

console.log(`SER documentation checks passed (${markdownFiles.length} Markdown files).`);
