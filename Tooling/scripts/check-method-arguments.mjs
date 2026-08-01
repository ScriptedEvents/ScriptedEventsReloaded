import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const toolingDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const methodsDirectory = path.resolve(toolingDirectory, "..", "Code", "MethodSystem", "Methods");

const argumentPattern = /new\s+[A-Za-z0-9_<>.,?]+Argument(?:<[^>]+>)?\(\s*"([^"]+)"/g;
const getterPattern = /Args\s*\.\s*Get[A-Za-z0-9_]*(?:<[\s\S]*?>)?\s*\(\s*"([^"]+)"/g;

function getCSharpFiles(directory) {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
    const filename = path.join(directory, entry.name);
    if (entry.isDirectory()) return getCSharpFiles(filename);
    return entry.isFile() && entry.name.endsWith(".cs") ? [filename] : [];
  });
}

for (const filename of getCSharpFiles(methodsDirectory)) {
  const source = fs.readFileSync(filename, "utf8");
  const declared = new Set([...source.matchAll(argumentPattern)].map(match => match[1]));
  for (const match of source.matchAll(/Argument\.PlayersArgumentUpdating\(\s*"([^"]+)"/g)) {
    declared.add(match[1]);
    declared.add(`update ${match[1]}`);
  }
  for (const match of source.matchAll(/MerObjectTypeArguments\.Create(?:Filter)?\(\s*"([^"]+)"/g)) {
    declared.add(match[1]);
  }
  for (const match of source.matchAll(/MerTransformArguments\.Create\(\s*"([^"]+)"/g)) {
    declared.add("MER reference");
    declared.add("mode");
    declared.add(`x ${match[1]}`);
    declared.add(`y ${match[1]}`);
    declared.add(`z ${match[1]}`);
  }
  for (const match of source.matchAll(/MerAnimationArguments\.Create\(\s*"([^"]+)"/g)) {
    declared.add(match[1]);
  }
  const accessed = new Set([...source.matchAll(getterPattern)].map(match => match[1]));

  for (const name of accessed) {
    assert.ok(
      declared.has(name),
      `${path.relative(methodsDirectory, filename)} reads undeclared argument '${name}'.`
    );
  }

  for (const name of declared) {
    assert.ok(
      accessed.has(name),
      `${path.relative(methodsDirectory, filename)} declares unused argument '${name}'.`
    );
  }
}

console.log("SER method argument usage checks passed.");
