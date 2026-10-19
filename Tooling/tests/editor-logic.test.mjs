import assert from "node:assert/strict";
import { createRequire } from "node:module";
import test from "node:test";
import path from "node:path";
import { fileURLToPath } from "node:url";

const require = createRequire(import.meta.url);
const repositoryDirectory = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "..",
  ".."
);
const editorLogic = require(
  path.join(repositoryDirectory, "VS Code Extension", "src", "ser-editor-logic.js")
);

test("keeps SER tilde-escaped quotes inside the active string", () => {
  const line = 'Broadcast @all 5s "say ~"hello~" now"';
  const range = editorLogic.findQuotedStringAtPosition(line, line.indexOf("hello"));
  assert.deepEqual(range, {
    start: line.indexOf('"'),
    end: line.lastIndexOf('"')
  });
});

test("does not treat tilde-escaped interpolation braces as expressions", () => {
  const line = 'Broadcast @all 5s "literal ~{$name}, actual {$name}"';
  const escapedRange = editorLogic.findQuotedStringAtPosition(line, line.indexOf("$name"));
  const interpolationPosition = line.lastIndexOf("$name");
  const interpolationRange = editorLogic.findQuotedStringAtPosition(line, interpolationPosition);

  assert.ok(escapedRange);
  assert.equal(interpolationRange, null);
});

test("tokenizes quoted SER arguments containing spaces and escaped quotes", () => {
  assert.deepEqual(
    editorLogic.tokenizeSerExpression('Broadcast @all 5s "say ~"hello~" now"'),
    [
      { start: 0, end: 9, text: "Broadcast" },
      { start: 10, end: 14, text: "@all" },
      { start: 15, end: 17, text: "5s" },
      { start: 18, end: 37, text: '"say ~"hello~" now"' }
    ]
  );
});

test("tracks the active method argument after tilde-escaped quotes", () => {
  const method = {
    arguments: [
      { syntax: "@players" },
      { syntax: "duration" },
      { syntax: "\"message\"" },
      { syntax: "$priority" }
    ]
  };
  const line = 'Notify @all 5s "say ~"hello~" now" 2';
  const context = editorLogic.getMethodCallContext(line, line.length, { Notify: method });

  assert.equal(context.methodName, "Notify");
  assert.equal(context.activeArgument, 3);
});

const shippedManifest = require(
  path.join(repositoryDirectory, "VS Code Extension", "out", "ser-language.json")
);

test("counts only arguments before the cursor when it sits in whitespace", () => {
  // A18: editing `SetRole @all  ClassD` with the cursor before ClassD asks for the role.
  const line = "SetRole @all  ClassD";
  const cursor = line.indexOf("ClassD") - 1;
  const context = editorLogic.getMethodCallContext(line, cursor, shippedManifest.methods);

  assert.equal(context.methodName, "SetRole");
  assert.equal(context.activeArgument, 1);
  assert.equal(shippedManifest.methods.SetRole.arguments[context.activeArgument].name, "newRole");

  const trailing = "SetRole @all ";
  assert.equal(
    editorLogic.getMethodCallContext(trailing, trailing.length, shippedManifest.methods).activeArgument,
    1
  );
});

test("resolves the method inside text interpolation", () => {
  // A19: `{ToDuration 3 }` inside quoted text is its own method call.
  const line = 'Print "Delay: {ToDuration 3 }"';
  const context = editorLogic.getMethodCallContext(
    line,
    line.indexOf("}"),
    shippedManifest.methods
  );

  assert.equal(context.methodName, "ToDuration");
  assert.equal(context.activeArgument, 1);
  assert.equal(shippedManifest.methods.ToDuration.arguments[1].name, "unit");
});

test("resolves interpolation while the closing quote is not typed yet", () => {
  const line = 'Print "Delay: {ToDuration 3 ';
  const context = editorLogic.getMethodCallContext(line, line.length, shippedManifest.methods);

  assert.equal(context.methodName, "ToDuration");
  assert.equal(context.activeArgument, 1);
});

test("keeps tilde-escaped interpolation braces as plain text for method context", () => {
  const line = 'Print "literal ~{ToDuration 3 }"';
  const context = editorLogic.getMethodCallContext(
    line,
    line.indexOf("}"),
    shippedManifest.methods
  );

  assert.equal(context.methodName, "Print");
  assert.equal(context.activeArgument, 0);
});

test("handles quoted text nested inside an interpolation", () => {
  const methods = {
    Print: { arguments: [{ syntax: "\"text\"" }] },
    Join: { arguments: [{ syntax: "\"first\"" }, { syntax: "\"second\"" }] }
  };
  const line = 'Print "a {Join "x}" } b"';
  const cursor = line.lastIndexOf("}");
  const context = editorLogic.getMethodCallContext(line, cursor, methods);

  assert.equal(context.methodName, "Join");
  assert.equal(context.activeArgument, 1);
  assert.deepEqual(
    editorLogic.tokenizeSerExpression(line).map(token => token.text),
    ["Print", '"a {Join "x}" } b"']
  );
});
