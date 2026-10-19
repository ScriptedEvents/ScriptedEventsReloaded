import assert from "node:assert/strict";
import fs from "node:fs";
import { createRequire } from "node:module";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Runs the real Blocks block definitions, code generator and workspace checks
// in a headless Blockly workspace (no browser page).
const require = createRequire(import.meta.url);
const toolingDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repositoryDirectory = path.resolve(toolingDirectory, "..");
const Blockly = require(path.join(toolingDirectory, "node_modules", "blockly"));
const core = require(path.join(toolingDirectory, "shared", "ser-language-core.js"));
const { SER_TRUTH_TABLE } = require(
  path.join(repositoryDirectory, "VS Code Extension", "out", "ser_method_info.js")
);
const editorSource = fs.readFileSync(
  path.join(toolingDirectory, "visual-editor", "src", "editor.js"),
  "utf8"
);

function sliceBetween(source, startMarker, endMarker) {
  const start = startMarker ? source.indexOf(startMarker) : 0;
  const end = source.indexOf(endMarker);
  assert.ok(start >= 0 && end > start, `editor.js no longer contains ${startMarker || "its start"}…${endMarker}`);
  return source.slice(start, end);
}

function createHeadlessEditor() {
  const definitions = sliceBetween(editorSource, null, "  function shadow(");
  const checks = sliceBetween(editorSource, "  function entryAncestor(", "  function renderDiagnostics(");
  const context = { Blockly, SER_TRUTH_TABLE, SERLanguageCore: core, console };
  vm.createContext(context);
  vm.runInContext(
    `${definitions}
    const workspace = new Blockly.Workspace();
    globalThis.workspace = workspace;
    globalThis.generator = serGenerator;
    ${checks}
    globalThis.check = workspaceDiagnostics;
    })();`,
    context
  );
  return context;
}

function addRoot(editor, type, y, message, fields = {}) {
  const root = editor.workspace.newBlock(type);
  for (const [name, value] of Object.entries(fields)) root.setFieldValue(value, name);
  root.moveBy(0, y);
  const action = editor.workspace.newBlock("ser_broadcast");
  const text = editor.workspace.newBlock("ser_text");
  text.setFieldValue(message, "TEXT");
  action.getInput("MESSAGE").connection.connect(text.outputConnection);
  const target = editor.workspace.newBlock("ser_target");
  action.getInput("TARGET").connection.connect(target.outputConnection);
  const duration = editor.workspace.newBlock("ser_duration");
  action.getInput("DURATION").connection.connect(duration.outputConnection);
  root.getInput("DO").connection.connect(action.previousConnection);
  return root;
}

function diagnosticsFor(editor) {
  const code = editor.generator.workspaceToCode(editor.workspace).trimEnd();
  // Copy out of the sandbox so assertions compare plain arrays from this realm.
  return JSON.parse(JSON.stringify(editor.check(code)));
}

function errorCodes(diagnostics) {
  return diagnostics.filter(item => item.severity === "error").map(item => item.code);
}

test("blocks a manual start mixed with an event start, in either order", () => {
  // C01: placed first, SER rejects the file; placed last, the manual actions run on the event.
  for (const manualFirst of [true, false]) {
    const editor = createHeadlessEditor();
    addRoot(editor, "ser_when_manual", manualFirst ? 0 : 400, "manual");
    addRoot(editor, "ser_when_event", 200, "event", { EVENT: "RoundStarted" });

    const diagnostics = diagnosticsFor(editor);
    assert.deepEqual(errorCodes(diagnostics), ["mixed-manual-entry"]);
    assert.match(
      diagnostics.find(item => item.code === "mixed-manual-entry").message,
      /separate script/
    );
  }
});

test("blocks a manual start mixed with a command start", () => {
  const editor = createHeadlessEditor();
  addRoot(editor, "ser_when_command", 0, "command", { COMMAND: "heal" });
  addRoot(editor, "ser_when_manual", 200, "manual");

  assert.deepEqual(errorCodes(diagnosticsFor(editor)), ["mixed-manual-entry"]);
});

test("blocks more than one manual start", () => {
  const editor = createHeadlessEditor();
  addRoot(editor, "ser_when_manual", 0, "first");
  addRoot(editor, "ser_when_manual", 200, "second");

  const diagnostics = diagnosticsFor(editor);
  assert.deepEqual(errorCodes(diagnostics), ["multiple-manual-entries"]);
  assert.match(
    diagnostics.find(item => item.code === "multiple-manual-entries").message,
    /separate script/
  );
});

test("still allows one manual start, and several event or command starts", () => {
  const manual = createHeadlessEditor();
  addRoot(manual, "ser_when_manual", 0, "manual");
  assert.deepEqual(errorCodes(diagnosticsFor(manual)), []);

  // SER splits a file into one part per event or command start.
  const flagged = createHeadlessEditor();
  addRoot(flagged, "ser_when_event", 0, "event", { EVENT: "RoundStarted" });
  addRoot(flagged, "ser_when_command", 200, "command", { COMMAND: "heal" });
  assert.deepEqual(errorCodes(diagnosticsFor(flagged)), []);
});
