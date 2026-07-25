import assert from "node:assert/strict";
import fs from "node:fs";
import test from "node:test";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repositoryDirectory = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "..",
  ".."
);
const configuration = JSON.parse(
  fs.readFileSync(
    path.join(repositoryDirectory, "VS Code Extension", "language-configuration.json"),
    "utf8"
  )
);

test("declares SER comment and paired-delimiter behavior", () => {
  assert.equal(configuration.comments.lineComment, "#");
  assert.ok(configuration.brackets.some(pair => pair[0] === "{" && pair[1] === "}"));
  assert.ok(configuration.brackets.some(pair => pair[0] === "(" && pair[1] === ")"));
  assert.ok(
    configuration.autoClosingPairs.some(pair => pair.open === "\"" && pair.close === "\"")
  );
});

test("indents statement bodies and outdents extenders and end", () => {
  const increase = new RegExp(configuration.indentationRules.increaseIndentPattern);
  const decrease = new RegExp(configuration.indentationRules.decreaseIndentPattern);

  for (const line of ["if true", "    else", "func $name", "repeat 3", "over @all with @player"]) {
    assert.match(line, increase);
  }
  for (const line of ["elif true", "else", "on_error with $error", "end"]) {
    assert.match(line, decrease);
  }
  assert.doesNotMatch("Broadcast @all 5s \"hello\"", increase);
});
