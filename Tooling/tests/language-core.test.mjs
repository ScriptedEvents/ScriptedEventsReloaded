import assert from "node:assert/strict";
import { createRequire } from "node:module";
import test from "node:test";
import path from "node:path";
import { fileURLToPath } from "node:url";

const require = createRequire(import.meta.url);
const toolingDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const core = require(path.join(toolingDirectory, "shared", "ser-language-core.js"));

const manifest = {
  schemaVersion: 1,
  methods: {
    Broadcast: {
      subgroup: "Broadcast",
      essential: true,
      description: "Shows a message.",
      arguments: [
        { name: "players", argumentKind: "PlayersArgument", mustBeProvided: true, syntax: "@players" },
        { name: "duration", argumentKind: "DurationArgument", mustBeProvided: true, syntax: "duration" },
        { name: "message", argumentKind: "TextArgument", mustBeProvided: true, syntax: "\"message\"" }
      ]
    },
    AdvancedThing: {
      subgroup: "Advanced",
      essential: false,
      description: "An advanced method.",
      returns: "bool value",
      arguments: []
    }
  },
  keywords: { if: { syntax: "if condition" } },
  flags: {
    OnEvent: {
      description: "Runs on an event.",
      inlineArgument: { name: "eventName", required: true },
      arguments: []
    }
  },
  variables: [
    { name: "all", prefix: "@", fullName: "@all", type: "player variable" }
  ],
  events: ["RoundStarted", "Joined"],
  eventDetails: {
    RoundStarted: { variables: [] },
    Joined: {
      variables: [
        { name: "@evPlayer", type: "player value", description: "The player who joined." }
      ]
    }
  }
};

test("normalizes the generated manifest for Blockly", () => {
  const editor = core.toEditorMetadata(manifest);
  assert.equal(editor.Methods.length, 2);
  assert.equal(editor.Methods[0].Subgroup, "Broadcast");
  assert.equal(editor.Methods[0].Arguments[0].Type, "PlayersArgument");
  assert.equal(editor.Methods[1].ReturnType, "BoolValue");
  assert.deepEqual(editor.BeginnerMethods, ["Broadcast"]);
});

test("searches and filters the shared method catalog", () => {
  assert.deepEqual(
    core.searchCatalog(manifest, "broad", { beginnerOnly: true }).map(method => method.name),
    ["Broadcast"]
  );
  assert.deepEqual(
    core.searchCatalog(manifest, "advanced").map(method => method.name),
    ["AdvancedThing"]
  );
});

test("reads enum arguments and event variables from the shared manifest", () => {
  const manifestWithEnums = structuredClone(manifest);
  manifestWithEnums.methods.Broadcast.arguments[1].enumValues = ["One", "Two"];
  assert.deepEqual(core.enumArgumentValues(manifestWithEnums, "Broadcast", 1), ["One", "Two"]);
  assert.deepEqual(core.enumArgumentValues(manifestWithEnums, "Missing", 0), []);
  assert.deepEqual(core.eventVariableNames(manifest, "Joined"), ["@evPlayer"]);
  assert.equal(core.eventSupportsVariable(manifest, "Joined", "@evPlayer"), true);
  assert.equal(core.eventSupportsVariable(manifest, "RoundStarted", "@evPlayer"), false);
});

test("escapes SER text using the SER escape marker", () => {
  assert.equal(core.escapeSerText('say "hello" ~ now'), 'say ~"hello~" ~~ now');
});

test("blocks export when required values remain empty", () => {
  const diagnostics = core.validateGeneratedCode("Broadcast @all 5s ...\n");
  assert.ok(diagnostics.some(item => item.code === "missing-value" && item.severity === "error"));
});

test("requires an event name for optional integration flags", () => {
  for (const flag of ["OnPMER", "OnUCR"]) {
    const diagnostics = core.validateGeneratedCode(`!-- ${flag}\n`);
    assert.ok(diagnostics.some(item => item.code === "missing-flag-argument" && item.severity === "error"));
  }
});

test("does not treat ellipses inside text as missing inputs", () => {
  const diagnostics = core.validateGeneratedCode('Broadcast @all 5s "Wait... what?"\n');
  assert.ok(!diagnostics.some(item => item.code === "missing-value"));
});

test("detects the legacy invalid function-call syntax", () => {
  const diagnostics = core.validateGeneratedCode("run $Add with 1 2\n");
  assert.ok(diagnostics.some(item => item.code === "invalid-function-call"));
});

test("warns when a forever loop has no wait", () => {
  const diagnostics = core.validateGeneratedCode("forever\nPrint \"busy\"\nend\n");
  assert.ok(diagnostics.some(item => item.code === "forever-without-wait"));
});
