import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

const directory = path.dirname(fileURLToPath(import.meta.url));

function createClient() {
  const child = spawn(process.execPath, [path.join(directory, "server.mjs")], {
    cwd: path.resolve(directory, "..", ".."),
    env: {
      ...process.env,
      SCPSL_LOCAL_ADMIN_COMMAND: process.execPath,
      SCPSL_LOCAL_ADMIN_ARGS: JSON.stringify([
        path.join(directory, "fake-local-admin.mjs"),
      ]),
      SCPSL_STOP_COMMAND: "exit",
    },
    stdio: ["pipe", "pipe", "pipe"],
  });

  let nextId = 1;
  let buffer = "";
  const pending = new Map();
  child.stdout.on("data", (chunk) => {
    buffer += chunk.toString("utf8");
    const lines = buffer.split("\n");
    buffer = lines.pop() ?? "";
    for (const line of lines) {
      if (!line.trim()) continue;
      const message = JSON.parse(line);
      const resolver = pending.get(message.id);
      if (resolver) {
        pending.delete(message.id);
        resolver(message);
      }
    }
  });

  function request(method, params = {}) {
    const id = nextId++;
    child.stdin.write(`${JSON.stringify({ jsonrpc: "2.0", id, method, params })}\n`);
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        pending.delete(id);
        reject(new Error(`Timed out waiting for ${method}`));
      }, 5000);
      pending.set(id, (message) => {
        clearTimeout(timer);
        resolve(message);
      });
    });
  }

  return { child, request };
}

test("MCP bridge initializes, starts, commands, reads, and stops", async (t) => {
  const client = createClient();
  t.after(() => {
    if (client.child.exitCode === null) client.child.kill();
  });

  const initialized = await client.request("initialize", {
    protocolVersion: "2025-06-18",
    capabilities: {},
    clientInfo: { name: "test", version: "1.0.0" },
  });
  assert.equal(initialized.result.serverInfo.name, "scpsl-console");
  assert.match(initialized.result.instructions, /sermethod AddDummy "name"/);

  const listed = await client.request("tools/list");
  assert.ok(listed.result.tools.some((tool) => tool.name === "build_restart_and_verify"));
  assert.match(
    listed.result.tools.find((tool) => tool.name === "send_console_command").description,
    /sermethod AddDummy "name"/,
  );

  const started = await client.request("tools/call", {
    name: "start_server",
    arguments: { port: 7788, timeout_ms: 2000 },
  });
  assert.equal(started.result.isError, false);
  const startResult = JSON.parse(started.result.content[0].text);
  assert.equal(startResult.status.running, true);
  assert.match(startResult.console, /Waiting for players/);

  const commanded = await client.request("tools/call", {
    name: "send_console_command",
    arguments: { command: "serreload", wait_ms: 50 },
  });
  const commandResult = JSON.parse(commanded.result.content[0].text);
  assert.match(commandResult.console, /Executed: serreload/);

  const read = await client.request("tools/call", {
    name: "read_console",
    arguments: { cursor: 0 },
  });
  const readResult = JSON.parse(read.result.content[0].text);
  assert.match(readResult.console, /Fake LocalAdmin/);

  const stopped = await client.request("tools/call", {
    name: "stop_server",
    arguments: { timeout_ms: 2000 },
  });
  const stopResult = JSON.parse(stopped.result.content[0].text);
  assert.equal(stopResult.stopped, true);
  client.child.stdin.end();
});
