import fs from "node:fs";

export function readGeneratedManifest(manifestPath) {
  const source = fs.readFileSync(manifestPath, "utf8");
  const match = source.match(
    /^\s*const SER_TRUTH_TABLE\s*=\s*([\s\S]*);\s*module\.exports\s*=\s*\{\s*SER_TRUTH_TABLE\s*\};?\s*$/
  );
  if (!match) {
    throw new Error(`Could not read SER_TRUTH_TABLE from ${manifestPath}`);
  }
  const manifest = JSON.parse(match[1]);
  if (!manifest.methods || !manifest.keywords || !manifest.flags) {
    throw new Error("The generated SER manifest is missing required sections.");
  }
  return manifest;
}

export function manifestModuleSource(manifest) {
  return `const SER_TRUTH_TABLE = ${JSON.stringify(manifest, null, 2)};\n\n` +
    "module.exports = { SER_TRUTH_TABLE };\n";
}
