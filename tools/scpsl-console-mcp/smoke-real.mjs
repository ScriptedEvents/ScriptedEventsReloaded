#!/usr/bin/env node

import { spawn } from "node:child_process";
import path from "node:path";
import { fileURLToPath } from "node:url";

const directory = path.dirname(fileURLToPath(import.meta.url));
const child = spawn(process.execPath, [path.join(directory, "server.mjs")], {
  cwd: path.resolve(directory, "..", ".."),
  env: process.env,
  stdio: ["pipe", "pipe", "inherit"],
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

function request(method, params = {}, timeoutMs = 120000) {
  const id = nextId++;
  child.stdin.write(`${JSON.stringify({ jsonrpc: "2.0", id, method, params })}\n`);
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => {
      pending.delete(id);
      reject(new Error(`Timed out waiting for ${method}`));
    }, timeoutMs);
    pending.set(id, (message) => {
      clearTimeout(timer);
      resolve(message);
    });
  });
}

function toolResult(message) {
  if (message.result?.isError) {
    throw new Error(message.result.content?.[0]?.text ?? "MCP tool failed");
  }
  return JSON.parse(message.result.content[0].text);
}

try {
  await request("initialize", {
    protocolVersion: "2025-06-18",
    capabilities: {},
    clientInfo: { name: "real-smoke-test", version: "1.0.0" },
  });
  const started = toolResult(
    await request(
      "tools/call",
      {
        name: "build_restart_and_verify",
        arguments: {
          configuration: "Full Debug",
          startup_timeout_ms: 120000,
        },
      },
      300000,
    ),
  );
  const noteworthyStartupLines = started.verification_console
    .split("\n")
    .filter((line) => /\[SER\]|error|exception|warning/i.test(line))
    .slice(-50);
  process.stdout.write(
    `${JSON.stringify(
      {
        build_succeeded: started.build.succeeded,
        output_path: started.build.output_path,
        output_exists: started.build.output_exists,
        deployed_path: started.build.deployed_path,
        deployed_exists: started.build.deployed_exists,
        start_status: started.start_status,
        readiness_line: started.readiness_line,
        noteworthy_startup_lines: noteworthyStartupLines,
      },
      null,
      2,
    )}\n`,
  );

  const status = toolResult(
    await request("tools/call", {
      name: "send_console_command",
      arguments: { command: "serhelp", wait_ms: 2000 },
    }),
  );
  process.stdout.write(`${JSON.stringify(status, null, 2)}\n`);
} finally {
  try {
    const stopped = toolResult(
      await request("tools/call", {
        name: "stop_server",
        arguments: { force: true, timeout_ms: 15000 },
      }, 25000),
    );
    process.stdout.write(`${JSON.stringify(stopped, null, 2)}\n`);
  } finally {
    child.stdin.end();
  }
}
