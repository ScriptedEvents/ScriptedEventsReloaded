import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import { readGeneratedManifest } from "./manifest.mjs";

const toolingDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repositoryDirectory = path.resolve(toolingDirectory, "..");
const extensionDirectory = path.join(repositoryDirectory, "VS Code Extension");

const files = {
  core: path.join(toolingDirectory, "shared", "ser-language-core.js"),
  editor: path.join(toolingDirectory, "visual-editor", "src", "editor.js"),
  standalone: path.join(repositoryDirectory, "SER Visual Editor.html"),
  extensionSource: path.join(extensionDirectory, "src", "extension.js"),
  extensionEditorLogicSource: path.join(extensionDirectory, "src", "ser-editor-logic.js"),
  extensionOutput: path.join(extensionDirectory, "out", "extension.js"),
  extensionEditorLogicOutput: path.join(extensionDirectory, "out", "ser-editor-logic.js"),
  extensionCore: path.join(extensionDirectory, "out", "ser-language-core.js"),
  extensionEditor: path.join(extensionDirectory, "out", "visual-editor.html"),
  thirdPartyLicenses: path.join(repositoryDirectory, "THIRD_PARTY_LICENSES.txt"),
  extensionThirdPartyLicenses: path.join(extensionDirectory, "THIRD_PARTY_LICENSES.txt"),
  manifest: path.join(repositoryDirectory, "ser_method_info.js"),
  extensionManifest: path.join(extensionDirectory, "out", "ser_method_info.js"),
  websiteManifest: path.join(repositoryDirectory, "website", "data", "ser-truth-table.json")
};

for (const [name, filename] of Object.entries(files)) {
  assert.ok(fs.existsSync(filename), `Missing ${name}: ${filename}`);
}

for (const filename of [
  files.core,
  files.editor,
  files.extensionSource,
  files.extensionEditorLogicSource,
  files.extensionOutput,
  files.extensionEditorLogicOutput
]) {
  const result = spawnSync(process.execPath, ["--check", filename], { encoding: "utf8" });
  assert.equal(result.status, 0, `${filename} has invalid JavaScript:\n${result.stderr}`);
}

const manifest = readGeneratedManifest(files.manifest);
const extensionManifest = readGeneratedManifest(files.extensionManifest);
assert.deepEqual(extensionManifest, manifest, "The extension manifest is out of sync with SER.");
assert.deepEqual(
  JSON.parse(fs.readFileSync(files.websiteManifest, "utf8")),
  manifest,
  "The documentation website manifest is out of sync with SER."
);
assert.ok(Object.keys(manifest.methods).length > 0, "The manifest contains no methods.");
assert.ok(
  Object.values(manifest.methods).some(method => method.requiredFramework),
  "The manifest contains no optional-framework methods."
);
assert.ok(Object.keys(manifest.keywords).length > 0, "The manifest contains no keywords.");
assert.ok(Object.keys(manifest.flags).length > 0, "The manifest contains no flags.");

assert.equal(
  fs.readFileSync(files.extensionOutput, "utf8"),
  fs.readFileSync(files.extensionSource, "utf8"),
  "VS Code out/extension.js is stale. Run the tooling build."
);
assert.equal(
  fs.readFileSync(files.extensionEditorLogicOutput, "utf8"),
  fs.readFileSync(files.extensionEditorLogicSource, "utf8"),
  "VS Code out/ser-editor-logic.js is stale. Run the tooling build."
);
assert.equal(
  fs.readFileSync(files.extensionCore, "utf8"),
  fs.readFileSync(files.core, "utf8"),
  "The extension's shared language core is stale."
);

const thirdPartyLicenses = fs.readFileSync(files.thirdPartyLicenses, "utf8");
assert.equal(
  fs.readFileSync(files.extensionThirdPartyLicenses, "utf8"),
  thirdPartyLicenses,
  "The extension's third-party notices are stale. Run the tooling build."
);
for (const requiredNotice of [
  "Blockly 13.2.0",
  "Apache License",
  "AudioPlayerApi 1.1.3",
  "EXILED 9.14.2",
  "NCalc 1.3.8",
  "Newtonsoft.Json 13.0.4",
  "NVorbis 0.10.5",
  "SharpCompress 0.48.1"
]) {
  assert.ok(
    thirdPartyLicenses.includes(requiredNotice),
    `THIRD_PARTY_LICENSES.txt is missing ${requiredNotice}.`
  );
}

for (const filename of [files.standalone, files.extensionEditor]) {
  const html = fs.readFileSync(filename, "utf8");
  assert.ok(html.includes("SER Blocks"), `${filename} is not the beginner SER editor.`);
  assert.ok(html.includes("Choose an idea"), `${filename} is missing the guided recipe entry point.`);
  assert.ok(html.includes("ser_when_event"), `${filename} is missing the curated event block.`);
  assert.ok(html.includes("ser_player_exists"), `${filename} is missing the safe player check.`);
  assert.ok(html.includes("SERLanguageCore"), `${filename} does not include the shared core.`);
  assert.ok(html.includes("Blockly"), `${filename} does not include Blockly.`);
  assert.ok(
    html.includes("THIRD_PARTY_LICENSES.txt"),
    `${filename} does not direct redistributors to the third-party notices.`
  );
  assert.ok(!html.includes("https://unpkg.com/blockly/blockly.min.js"), `${filename} still uses the unpinned CDN.`);
  assert.ok(!html.includes("SER_TOOLING_"), `${filename} contains unexpanded build placeholders.`);
}

const editorSource = fs.readFileSync(files.editor, "utf8");
assert.ok(
  !editorSource.includes("metadata.Methods") && !editorSource.includes("All SER features"),
  "The beginner editor must not dynamically expose the complete SER API."
);

const extensionSource = fs.readFileSync(files.extensionSource, "utf8");
assert.ok(
  !extensionSource.includes("isTrusted = true"),
  "Generated SER documentation must not enable trusted Markdown commands."
);

const standaloneHtml = fs.readFileSync(files.standalone, "utf8");
const extensionHtml = fs.readFileSync(files.extensionEditor, "utf8");
assert.ok(!standaloneHtml.includes("__SER_WEBVIEW_"), "Standalone editor contains webview placeholders.");
assert.ok(extensionHtml.includes("__SER_WEBVIEW_NONCE__"), "Webview script nonce placeholder is missing.");
assert.ok(extensionHtml.includes("__SER_WEBVIEW_MEDIA__"), "Webview media placeholder is missing.");

const packageJson = JSON.parse(
  fs.readFileSync(path.join(extensionDirectory, "package.json"), "utf8")
);
assert.equal(packageJson.main, "./out/extension.js");
assert.ok(
  packageJson.contributes?.commands?.some(command => command.command === "ser.openVisualEditor"),
  "The visual-editor command is not registered."
);

console.log("SER tooling synchronization checks passed.");
