import assert from "node:assert/strict";
import test from "node:test";
import { normalizeManifestLineEndings } from "../scripts/manifest.mjs";

test("normalizes generated manifest line endings recursively", () => {
  const manifest = {
    description: "first\r\nsecond",
    methods: [{ example: "one\rtwo\nthree" }],
    flags: { OnEvent: { description: "alpha\r\nbeta" } }
  };

  assert.deepEqual(normalizeManifestLineEndings(manifest), {
    description: "first\nsecond",
    methods: [{ example: "one\ntwo\nthree" }],
    flags: { OnEvent: { description: "alpha\nbeta" } }
  });
});
