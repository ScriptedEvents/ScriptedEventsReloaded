import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { manifestModuleSource, readGeneratedManifest } from "./manifest.mjs";

const toolingDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repositoryDirectory = path.resolve(toolingDirectory, "..");
const editorSourceDirectory = path.join(toolingDirectory, "visual-editor", "src");
const editorDistributionDirectory = path.join(toolingDirectory, "visual-editor", "dist");
const extensionDirectory = path.join(repositoryDirectory, "VS Code Extension");
const extensionSourceDirectory = path.join(extensionDirectory, "src");
const extensionOutputDirectory = path.join(extensionDirectory, "out");

const paths = {
  license: path.join(repositoryDirectory, "LICENSE"),
  thirdPartyLicenses: path.join(repositoryDirectory, "THIRD_PARTY_LICENSES.txt"),
  thirdPartyLicenses: path.join(repositoryDirectory, "THIRD_PARTY_LICENSES.txt"),
  manifest: path.join(repositoryDirectory, "ser_method_info.js"),
  template: path.join(editorSourceDirectory, "index.html"),
  styles: path.join(editorSourceDirectory, "styles.css"),
  editor: path.join(editorSourceDirectory, "editor.js"),
  core: path.join(toolingDirectory, "shared", "ser-language-core.js"),
  blockly: path.join(toolingDirectory, "node_modules", "blockly", "blockly.min.js"),
  blocklyMedia: path.join(toolingDirectory, "node_modules", "blockly", "media"),
  extensionSource: path.join(extensionSourceDirectory, "extension.js"),
  extensionEditorLogic: path.join(extensionSourceDirectory, "ser-editor-logic.js")
};

for (const [name, requiredPath] of Object.entries(paths)) {
  if (!fs.existsSync(requiredPath)) throw new Error(`Missing ${name}: ${requiredPath}`);
}

const manifest = readGeneratedManifest(paths.manifest);
const template = fs.readFileSync(paths.template, "utf8");
const styles = fs.readFileSync(paths.styles, "utf8");
const core = fs.readFileSync(paths.core, "utf8");
const blockly = fs.readFileSync(paths.blockly, "utf8");
const editorSource = fs.readFileSync(paths.editor, "utf8");
const extensionSource = fs.readFileSync(paths.extensionSource, "utf8");
const extensionEditorLogic = fs.readFileSync(paths.extensionEditorLogic, "utf8");

function buildEditorHtml(mediaPath, webview = false) {
  const editor = editorSource.replaceAll("__SER_BLOCKLY_MEDIA__", mediaPath);
  const scriptOpen = webview ? '<script nonce="__SER_WEBVIEW_NONCE__">' : "<script>";
  return template
    .replace("<!-- SER_TOOLING_STYLES -->", `<style>\n${styles}\n</style>`)
    .replace("<!-- SER_TOOLING_BLOCKLY -->", `${scriptOpen}\n${blockly}\n</script>`)
    .replace(
      "<!-- SER_TOOLING_MANIFEST -->",
      `${scriptOpen}\nconst SER_TRUTH_TABLE = ${JSON.stringify(manifest)};\n</script>`
    )
    .replace("<!-- SER_TOOLING_CORE -->", `${scriptOpen}\n${core}\n</script>`)
    .replace("<!-- SER_TOOLING_EDITOR -->", `${scriptOpen}\n${editor}\n</script>`);
}

fs.mkdirSync(editorDistributionDirectory, { recursive: true });
fs.mkdirSync(extensionOutputDirectory, { recursive: true });

const standaloneHtml = buildEditorHtml("Tooling/visual-editor/dist/media/");
const extensionHtml = buildEditorHtml("__SER_WEBVIEW_MEDIA__/", true);
const manifestJson = `${JSON.stringify(manifest, null, 2)}\n`;

fs.writeFileSync(path.join(editorDistributionDirectory, "index.html"), standaloneHtml);
fs.writeFileSync(path.join(editorDistributionDirectory, "ser-language.json"), manifestJson);
fs.writeFileSync(path.join(repositoryDirectory, "SER Visual Editor.html"), standaloneHtml);

fs.writeFileSync(path.join(extensionOutputDirectory, "extension.js"), extensionSource);
fs.writeFileSync(path.join(extensionOutputDirectory, "ser-editor-logic.js"), extensionEditorLogic);
fs.writeFileSync(path.join(extensionOutputDirectory, "ser-language-core.js"), core);
fs.writeFileSync(path.join(extensionOutputDirectory, "ser_method_info.js"), manifestModuleSource(manifest));
fs.writeFileSync(path.join(extensionOutputDirectory, "visual-editor.html"), extensionHtml);
fs.writeFileSync(path.join(extensionOutputDirectory, "ser-language.json"), manifestJson);
fs.copyFileSync(paths.license, path.join(extensionDirectory, "LICENSE"));
fs.copyFileSync(
  paths.thirdPartyLicenses,
  path.join(extensionDirectory, "THIRD_PARTY_LICENSES.txt")
);
fs.copyFileSync(
  paths.thirdPartyLicenses,
  path.join(extensionDirectory, "THIRD_PARTY_LICENSES.txt")
);

fs.cpSync(paths.blocklyMedia, path.join(editorDistributionDirectory, "media"), {
  recursive: true,
  force: true
});
fs.cpSync(paths.blocklyMedia, path.join(extensionOutputDirectory, "media"), {
  recursive: true,
  force: true
});

console.log(
  `Built SER tooling from schema v${manifest.schemaVersion || "legacy"}: ` +
  `${Object.keys(manifest.methods).length} methods, ` +
  `${Object.keys(manifest.keywords).length} keywords, ` +
  `${Object.keys(manifest.flags).length} flags.`
);
