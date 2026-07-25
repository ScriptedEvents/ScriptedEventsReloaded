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
