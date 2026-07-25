#!/usr/bin/env node

import { spawn } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const projectDirectory = path.resolve(scriptDirectory, "..", "..");
const defaultLocalAdminPath =
  "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SCP Secret Laboratory Dedicated Server\\LocalAdmin.exe";
const maxBufferedLines = positiveInteger(process.env.SCPSL_CONSOLE_BUFFER_LINES, 5000);

let serverProcess = null;
let serverStartedAt = null;
let serverPort = null;
let nextSequence = 1;
let consoleLines = [];
const streamRemainders = new Map();
let inputBuffer = Buffer.alloc(0);
let shuttingDown = false;

const tools = [
  {
    name: "server_status",
    title: "SCP:SL server status",
    description:
      "Show whether the LocalAdmin process owned by this MCP server is running and return its PID, port, uptime, executable, and latest console cursor.",
    inputSchema: { type: "object", additionalProperties: false },
    annotations: {
      readOnlyHint: true,
      destructiveHint: false,
      idempotentHint: true,
      openWorldHint: false,
    },
  },
  {
    name: "start_server",
    title: "Start SCP:SL server",
    description:
      "Start the local SCP:SL dedicated server through LocalAdmin and capture its console. The MCP server must own LocalAdmin in order to send commands and read output.",
    inputSchema: {
      type: "object",
      properties: {
        port: {
          type: "integer",
          minimum: 1,
          maximum: 65535,
          description: "Server port. Defaults to SCPSL_SERVER_PORT or 7777.",
        },
        wait_for: {
          type: "string",
          description:
            "Case-insensitive console regex that indicates readiness. Defaults to 'Waiting for players'. Use an empty string to return immediately after spawn.",
        },
        timeout_ms: {
          type: "integer",
          minimum: 1000,
          maximum: 180000,
          description: "Maximum readiness wait. Defaults to 90000.",
        },
      },
      additionalProperties: false,
    },
    annotations: {
      readOnlyHint: false,
      destructiveHint: false,
      idempotentHint: true,
      openWorldHint: false,
    },
  },
  {
    name: "read_console",
    title: "Read SCP:SL console",
    description:
      "Read captured LocalAdmin output. Pass the cursor from a previous response to receive only newer lines; optionally wait for new output.",
    inputSchema: {
      type: "object",
      properties: {
        cursor: {
          type: "integer",
          minimum: 0,
          description: "Return lines newer than this cursor.",
        },
        tail: {
          type: "integer",
          minimum: 1,
          maximum: 500,
          description: "Maximum lines to return. Defaults to 200.",
        },
        wait_ms: {
          type: "integer",
          minimum: 0,
          maximum: 30000,
          description: "Wait this long when no newer lines exist. Defaults to 0.",
        },
      },
      additionalProperties: false,
    },
    annotations: {
      readOnlyHint: true,
      destructiveHint: false,
      idempotentHint: true,
      openWorldHint: false,
    },
  },
  {
    name: "send_console_command",
    title: "Send SCP:SL console command",
    description:
      "Send one command to the owned LocalAdmin console and return subsequent output. For player-dependent SER tests, first create one or more uniquely named test players with `sermethod AddDummy \"name\"`. The command executes with server-console authority; inspect and avoid destructive commands unless explicitly intended.",
    inputSchema: {
      type: "object",
      required: ["command"],
      properties: {
        command: {
          type: "string",
          minLength: 1,
          maxLength: 2000,
          description: "A single console command without a newline.",
        },
        wait_ms: {
          type: "integer",
          minimum: 0,
          maximum: 30000,
          description: "Time to collect command output. Defaults to 1500.",
        },
        max_lines: {
          type: "integer",
          minimum: 1,
          maximum: 500,
          description: "Maximum output lines to return. Defaults to 200.",
        },
      },
      additionalProperties: false,
    },
    annotations: {
      readOnlyHint: false,
      destructiveHint: true,
      idempotentHint: false,
      openWorldHint: false,
    },
  },
  {
    name: "stop_server",
    title: "Stop SCP:SL server",
    description:
      "Stop the LocalAdmin process owned by this MCP server. It first sends the configured graceful stop command, then can terminate the exact process tree if needed.",
    inputSchema: {
      type: "object",
      properties: {
        force: {
          type: "boolean",
          description:
            "Terminate the owned process tree if graceful shutdown times out. Defaults to false.",
        },
        timeout_ms: {
          type: "integer",
          minimum: 1000,
          maximum: 30000,
          description: "Graceful shutdown timeout. Defaults to 10000.",
        },
      },
      additionalProperties: false,
    },
    annotations: {
      readOnlyHint: false,
      destructiveHint: true,
      idempotentHint: true,
      openWorldHint: false,
    },
  },
  {
    name: "build_plugin",
    title: "Build SER plugin",
    description:
      "Build SER.csproj with dotnet. The existing MSBuild target deploys a successful build to LABAPI_PLUGINS and validates the bundled example scripts.",
    inputSchema: {
      type: "object",
      properties: {
        configuration: {
          type: "string",
          enum: ["Release", "Full Debug", "Partial Debug", "EXILED"],
          description: "Build configuration. Defaults to Full Debug.",
        },
        timeout_ms: {
          type: "integer",
          minimum: 1000,
          maximum: 300000,
          description: "Build timeout. Defaults to 180000.",
        },
      },
      additionalProperties: false,
    },
    annotations: {
      readOnlyHint: false,
      destructiveHint: false,
      idempotentHint: true,
      openWorldHint: false,
    },
  },
  {
    name: "build_restart_and_verify",
    title: "Build, restart, and verify SER",
    description:
      "Stop the owned server if necessary, build and deploy SER, start LocalAdmin, wait for readiness, and return startup logs. Use this after plugin DLL changes.",
    inputSchema: {
      type: "object",
      properties: {
        configuration: {
          type: "string",
          enum: ["Release", "Full Debug", "Partial Debug", "EXILED"],
          description: "Build configuration. Defaults to Full Debug.",
        },
        port: {
          type: "integer",
          minimum: 1,
          maximum: 65535,
          description: "Server port. Defaults to the previous port, SCPSL_SERVER_PORT, or 7777.",
        },
        wait_for: {
          type: "string",
          description: "Readiness regex. Defaults to 'Waiting for players'.",
        },
        startup_timeout_ms: {
          type: "integer",
          minimum: 1000,
          maximum: 180000,
          description: "Startup readiness timeout. Defaults to 90000.",
        },
        log_lines: {
          type: "integer",
          minimum: 1,
          maximum: 500,
          description: "Startup lines returned for verification. Defaults to 250.",
        },
      },
      additionalProperties: false,
    },
    annotations: {
      readOnlyHint: false,
      destructiveHint: true,
      idempotentHint: false,
      openWorldHint: false,
    },
  },
];

function positiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? "", 10);
  return Number.isSafeInteger(parsed) && parsed > 0 ? parsed : fallback;
}

function sleep(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

function executableConfiguration() {
  const command =
    process.env.SCPSL_LOCAL_ADMIN_COMMAND ||
    process.env.SCPSL_LOCAL_ADMIN_PATH ||
    defaultLocalAdminPath;
  let args = [];
  if (process.env.SCPSL_LOCAL_ADMIN_ARGS) {
    try {
      const parsed = JSON.parse(process.env.SCPSL_LOCAL_ADMIN_ARGS);
      if (!Array.isArray(parsed) || parsed.some((item) => typeof item !== "string")) {
        throw new Error("expected a JSON array of strings");
      }
      args = parsed;
    } catch (error) {
      throw new Error(`Invalid SCPSL_LOCAL_ADMIN_ARGS: ${error.message}`);
    }
  }
  const cwd =
    process.env.SCPSL_SERVER_DIRECTORY ||
    (path.isAbsolute(command) ? path.dirname(command) : projectDirectory);
  return { command, args, cwd };
}

function appendLine(stream, text) {
  const cleanText = text.replace(/\u001b\[[0-?]*[ -/]*[@-~]/g, "").replace(/\r$/, "");
  if (!cleanText && stream !== "system") return;
  consoleLines.push({
    sequence: nextSequence++,
    timestamp: new Date().toISOString(),
    stream,
    text: cleanText,
  });
  if (consoleLines.length > maxBufferedLines) {
    consoleLines.splice(0, consoleLines.length - maxBufferedLines);
  }
}

function consumeChunk(stream, chunk) {
  const remainder = streamRemainders.get(stream) ?? "";
  const parts = (remainder + chunk.toString("utf8")).split(/\r?\n/);
  const trailing = parts.pop() ?? "";
  streamRemainders.set(stream, trailing);
  for (const line of parts) appendLine(stream, line);
}

function flushRemainders(...streams) {
  const selectedStreams =
    streams.length > 0 ? streams : [...streamRemainders.keys()];
  for (const stream of selectedStreams) {
    const remainder = streamRemainders.get(stream);
    if (remainder) appendLine(stream, remainder);
    streamRemainders.delete(stream);
  }
}

function isRunning() {
  return Boolean(serverProcess && serverProcess.exitCode === null && !serverProcess.killed);
}

function statusObject() {
  let executable;
  let executableExists = null;
  try {
    const config = executableConfiguration();
    executable = config.command;
    executableExists = path.isAbsolute(executable) ? fs.existsSync(executable) : null;
  } catch (error) {
    executable = `configuration error: ${error.message}`;
  }
  return {
    running: isRunning(),
    pid: isRunning() ? serverProcess.pid : null,
    port: serverPort,
    started_at: serverStartedAt,
    uptime_seconds:
      isRunning() && serverStartedAt
        ? Math.floor((Date.now() - Date.parse(serverStartedAt)) / 1000)
        : null,
    executable,
    executable_exists: executableExists,
    project_directory: projectDirectory,
    latest_cursor: nextSequence - 1,
    buffered_lines: consoleLines.length,
  };
}

function linesAfter(cursor, limit) {
  const candidates =
    cursor === undefined
      ? consoleLines
      : consoleLines.filter((line) => line.sequence > cursor);
  return candidates.slice(-limit);
}

function formatLines(lines) {
  return lines
    .map(
      (line) =>
        `${line.sequence} ${line.timestamp} [${line.stream}] ${line.text}`,
    )
    .join("\n");
}

function processExitPromise(child) {
  if (child.exitCode !== null) {
    return Promise.resolve({ code: child.exitCode, signal: child.signalCode });
  }
  return new Promise((resolve) => {
    child.once("exit", (code, signal) => resolve({ code, signal }));
  });
}

async function waitForConsolePattern(pattern, fromCursor, timeoutMs) {
  const regex = new RegExp(pattern, "i");
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const match = consoleLines.find(
      (line) => line.sequence > fromCursor && regex.test(line.text),
    );
    if (match) return match;
    if (!isRunning()) {
      throw new Error("LocalAdmin exited before the readiness pattern appeared.");
    }
    await sleep(100);
  }
  throw new Error(`Timed out after ${timeoutMs}ms waiting for console pattern /${pattern}/i.`);
}

async function startServer(argumentsObject = {}) {
  if (isRunning()) {
    return {
      already_running: true,
      status: statusObject(),
      cursor: nextSequence - 1,
    };
  }

  const port = argumentsObject.port ?? positiveInteger(process.env.SCPSL_SERVER_PORT, 7777);
  if (!Number.isInteger(port) || port < 1 || port > 65535) {
    throw new Error("port must be an integer from 1 through 65535.");
  }

  const config = executableConfiguration();
  if (path.isAbsolute(config.command) && !fs.existsSync(config.command)) {
    throw new Error(`LocalAdmin executable was not found: ${config.command}`);
  }

  const cursorBeforeStart = nextSequence - 1;
  const childArgs = [...config.args];
  if (process.env.SCPSL_LOCAL_ADMIN_APPEND_PORT !== "false") {
    childArgs.push(String(port));
  }

  appendLine("system", `Starting ${config.command} ${childArgs.join(" ")}`);
  const child = spawn(config.command, childArgs, {
    cwd: config.cwd,
    env: process.env,
    stdio: ["pipe", "pipe", "pipe"],
    windowsHide: true,
  });
  serverProcess = child;
  serverPort = port;
  serverStartedAt = new Date().toISOString();
  streamRemainders.delete("stdout");
  streamRemainders.delete("stderr");

  child.stdout.on("data", (chunk) => consumeChunk("stdout", chunk));
  child.stderr.on("data", (chunk) => consumeChunk("stderr", chunk));
  child.on("exit", (code, signal) => {
    flushRemainders("stdout", "stderr");
    appendLine(
      "system",
      `LocalAdmin exited (code=${code ?? "null"}, signal=${signal ?? "null"}).`,
    );
    if (serverProcess === child) serverProcess = null;
  });

  await new Promise((resolve, reject) => {
    child.once("spawn", resolve);
    child.once("error", reject);
  });

  const waitFor =
    argumentsObject.wait_for === undefined
      ? process.env.SCPSL_READY_PATTERN || "Waiting for players"
      : argumentsObject.wait_for;
  let readiness = null;
  if (waitFor) {
    try {
      readiness = await waitForConsolePattern(
        waitFor,
        cursorBeforeStart,
        argumentsObject.timeout_ms ?? 90000,
      );
    } catch (error) {
      const capturedConsole = formatLines(linesAfter(cursorBeforeStart, 200));
      throw new Error(
        `${error.message}\nCaptured LocalAdmin console:\n${capturedConsole || "(no output)"}`,
      );
    }
  }

  return {
    started: true,
    readiness_line: readiness,
    status: statusObject(),
    cursor: nextSequence - 1,
    console: formatLines(linesAfter(cursorBeforeStart, 200)),
  };
}

async function terminateProcessTree(child) {
  if (!child || child.exitCode !== null) return;
  if (process.platform === "win32") {
    const killer = spawn(
      "taskkill.exe",
      ["/PID", String(child.pid), "/T", "/F"],
      { windowsHide: true, stdio: ["ignore", "pipe", "pipe"] },
    );
    await processExitPromise(killer);
  } else {
    child.kill("SIGKILL");
  }
}

async function stopServer(argumentsObject = {}) {
  if (!isRunning()) {
    return { already_stopped: true, status: statusObject() };
  }

  const child = serverProcess;
  const timeoutMs = argumentsObject.timeout_ms ?? 10000;
  const stopCommand = process.env.SCPSL_STOP_COMMAND || "exit";
  const exitPromise = processExitPromise(child);
  appendLine("system", `Sending graceful stop command: ${stopCommand}`);
  child.stdin.write(`${stopCommand}\n`);

  const outcome = await Promise.race([
    exitPromise.then((result) => ({ exited: true, ...result })),
    sleep(timeoutMs).then(() => ({ exited: false })),
  ]);

  if (!outcome.exited && argumentsObject.force) {
    appendLine("system", `Graceful stop timed out after ${timeoutMs}ms; terminating owned process tree.`);
    await terminateProcessTree(child);
    await Promise.race([exitPromise, sleep(5000)]);
  }

  return {
    stopped: child.exitCode !== null || serverProcess !== child,
    graceful: outcome.exited,
    forced: !outcome.exited && Boolean(argumentsObject.force),
    status: statusObject(),
  };
}

async function readConsole(argumentsObject = {}) {
  const cursor = argumentsObject.cursor;
  const tail = argumentsObject.tail ?? 200;
  let lines = linesAfter(cursor, tail);
  if (lines.length === 0 && (argumentsObject.wait_ms ?? 0) > 0) {
    const deadline = Date.now() + argumentsObject.wait_ms;
    while (lines.length === 0 && Date.now() < deadline) {
      await sleep(Math.min(100, deadline - Date.now()));
      lines = linesAfter(cursor, tail);
    }
  }
  return {
    status: statusObject(),
    cursor: nextSequence - 1,
    line_count: lines.length,
    truncated:
      cursor !== undefined &&
      consoleLines.length > 0 &&
      cursor < consoleLines[0].sequence - 1,
    console: formatLines(lines),
  };
}

async function sendConsoleCommand(argumentsObject) {
  if (!isRunning()) {
    throw new Error("The MCP server does not own a running LocalAdmin process. Call start_server first.");
  }
  const command = argumentsObject.command;
  if (typeof command !== "string" || !command.trim()) {
    throw new Error("command must be a non-empty string.");
  }
  if (/[\r\n\0]/.test(command)) {
    throw new Error("command must contain exactly one console line.");
  }

  const cursor = nextSequence - 1;
  appendLine("input", command);
  serverProcess.stdin.write(`${command}\n`);
  const waitMs = argumentsObject.wait_ms ?? 1500;
  if (waitMs > 0) await sleep(waitMs);
  const lines = linesAfter(cursor, argumentsObject.max_lines ?? 200);
  return {
    command,
    status: statusObject(),
    cursor: nextSequence - 1,
    console: formatLines(lines),
  };
}

async function runCaptured(command, args, options = {}) {
  const cursor = nextSequence - 1;
  appendLine("system", `Running ${command} ${args.join(" ")}`);
  const child = spawn(command, args, {
    cwd: options.cwd ?? projectDirectory,
    env: process.env,
    stdio: ["ignore", "pipe", "pipe"],
    windowsHide: true,
  });
  let stdout = "";
  let stderr = "";
  const outputLimit = 2 * 1024 * 1024;
  child.stdout.on("data", (chunk) => {
    const text = chunk.toString("utf8");
    stdout = (stdout + text).slice(-outputLimit);
    consumeChunk("build", chunk);
  });
  child.stderr.on("data", (chunk) => {
    const text = chunk.toString("utf8");
    stderr = (stderr + text).slice(-outputLimit);
    consumeChunk("build-error", chunk);
  });

  let timedOut = false;
  const exitPromise = processExitPromise(child);
  let timeoutHandle;
  const timeoutPromise = new Promise((resolve) => {
    timeoutHandle = setTimeout(async () => {
      timedOut = true;
      await terminateProcessTree(child);
      resolve({ code: null, signal: "timeout" });
    }, options.timeoutMs ?? 180000);
  });
  const result = await Promise.race([
    exitPromise,
    timeoutPromise,
  ]);
  clearTimeout(timeoutHandle);
  flushRemainders("build", "build-error");
  if (timedOut) throw new Error(`Command timed out: ${command} ${args.join(" ")}`);
  return {
    ...result,
    stdout,
    stderr,
    cursor,
  };
}

async function buildPlugin(argumentsObject = {}) {
  const configuration = argumentsObject.configuration ?? "Full Debug";
  const allowedConfigurations = new Set([
    "Release",
    "Full Debug",
    "Partial Debug",
    "EXILED",
  ]);
  if (!allowedConfigurations.has(configuration)) {
    throw new Error(`Unsupported build configuration: ${configuration}`);
  }
  const projectPath = path.join(projectDirectory, "SER.csproj");
  const buildCommand = process.env.SCPSL_BUILD_COMMAND || "dotnet";
  const result = await runCaptured(
    buildCommand,
    ["build", projectPath, "--configuration", configuration, "--nologo"],
    { timeoutMs: argumentsObject.timeout_ms ?? 180000 },
  );
  const assemblyName = configuration === "EXILED" ? "SER-Exiled.dll" : "SER.dll";
  const outputDirectory =
    configuration === "EXILED" ? "EXILED" : "LABAPI";
  const outputPath = path.join(
    projectDirectory,
    "bin",
    outputDirectory,
    "net48",
    assemblyName,
  );
  const deployedPath = process.env.LABAPI_PLUGINS
    ? path.join(process.env.LABAPI_PLUGINS, assemblyName)
    : null;
  const response = {
    succeeded: result.code === 0,
    exit_code: result.code,
    configuration,
    output_path: outputPath,
    output_exists: fs.existsSync(outputPath),
    deployed_path: deployedPath,
    deployed_exists: deployedPath ? fs.existsSync(deployedPath) : null,
    console: formatLines(linesAfter(result.cursor, 300)),
  };
  if (result.code !== 0) {
    throw new Error(`dotnet build failed with exit code ${result.code}.\n${response.console}`);
  }
  return response;
}

async function buildRestartAndVerify(argumentsObject = {}) {
  const previousPort = serverPort;
  const wasRunning = isRunning();
  if (wasRunning) {
    const stopResult = await stopServer({ force: true, timeout_ms: 10000 });
    if (!stopResult.stopped) {
      throw new Error("Could not stop the currently owned server process.");
    }
  }
  const build = await buildPlugin({
    configuration: argumentsObject.configuration,
    timeout_ms: 240000,
  });
  const startupCursor = nextSequence - 1;
  const start = await startServer({
    port:
      argumentsObject.port ??
      previousPort ??
      positiveInteger(process.env.SCPSL_SERVER_PORT, 7777),
    wait_for: argumentsObject.wait_for,
    timeout_ms: argumentsObject.startup_timeout_ms ?? 90000,
  });
  return {
    previous_server_was_running: wasRunning,
    build,
    start_status: start.status,
    readiness_line: start.readiness_line,
    cursor: nextSequence - 1,
    verification_console: formatLines(
      linesAfter(startupCursor, argumentsObject.log_lines ?? 250),
    ),
  };
}

async function callTool(name, argumentsObject = {}) {
  switch (name) {
    case "server_status":
      return statusObject();
    case "start_server":
      return startServer(argumentsObject);
    case "read_console":
      return readConsole(argumentsObject);
    case "send_console_command":
      return sendConsoleCommand(argumentsObject);
    case "stop_server":
      return stopServer(argumentsObject);
    case "build_plugin":
      return buildPlugin(argumentsObject);
    case "build_restart_and_verify":
      return buildRestartAndVerify(argumentsObject);
    default:
      throw new Error(`Unknown tool: ${name}`);
  }
}

function writeMessage(message) {
  process.stdout.write(`${JSON.stringify(message)}\n`);
}

function resultMessage(id, result) {
  writeMessage({ jsonrpc: "2.0", id, result });
}

function errorMessage(id, code, message, data) {
  writeMessage({
    jsonrpc: "2.0",
    id: id ?? null,
    error: { code, message, ...(data === undefined ? {} : { data }) },
  });
}

async function handleMessage(message) {
  if (!message || message.jsonrpc !== "2.0" || typeof message.method !== "string") {
    errorMessage(message?.id, -32600, "Invalid Request");
    return;
  }

  if (message.method === "notifications/initialized" || message.method === "notifications/cancelled") {
    return;
  }
  if (message.id === undefined) return;

  try {
    switch (message.method) {
      case "initialize":
        resultMessage(message.id, {
          protocolVersion: message.params?.protocolVersion ?? "2025-06-18",
          capabilities: { tools: { listChanged: false } },
          serverInfo: { name: "scpsl-console", version: "1.0.0" },
          instructions:
            "Use this server to test SER against the local SCP:SL dedicated server. LocalAdmin must be started with start_server so this bridge owns its stdin/stdout. After C# plugin changes use build_restart_and_verify, inspect verification_console for load errors and SER's enabled message, then exercise the relevant console command. For .ser-only edits, send 'serreload' and inspect its response. When a test requires players, create one or more uniquely named dummies with `sermethod AddDummy \"name\"` before exercising the behavior. Server console commands have full authority; do not run destructive moderation, shutdown, public-listing, or data-changing commands unless the user explicitly requests them.",
        });
        break;
      case "ping":
        resultMessage(message.id, {});
        break;
      case "tools/list":
        resultMessage(message.id, { tools });
        break;
      case "tools/call": {
        const name = message.params?.name;
        const argumentsObject = message.params?.arguments ?? {};
        try {
          const result = await callTool(name, argumentsObject);
          resultMessage(message.id, {
            content: [{ type: "text", text: JSON.stringify(result, null, 2) }],
            structuredContent: result,
            isError: false,
          });
        } catch (error) {
          resultMessage(message.id, {
            content: [{ type: "text", text: error.stack || error.message }],
            isError: true,
          });
        }
        break;
      }
      default:
        errorMessage(message.id, -32601, "Method not found");
    }
  } catch (error) {
    errorMessage(message.id, -32603, "Internal error", error.message);
  }
}

function parseInput() {
  while (inputBuffer.length > 0) {
    const asText = inputBuffer.toString("utf8");
    if (/^Content-Length:/i.test(asText)) {
      const headerEnd = asText.indexOf("\r\n\r\n");
      if (headerEnd < 0) return;
      const header = asText.slice(0, headerEnd);
      const match = /^Content-Length:\s*(\d+)$/im.exec(header);
      if (!match) {
        inputBuffer = Buffer.alloc(0);
        errorMessage(null, -32700, "Invalid Content-Length header");
        return;
      }
      const contentLength = Number.parseInt(match[1], 10);
      const bodyStart = Buffer.byteLength(asText.slice(0, headerEnd + 4), "utf8");
      if (inputBuffer.length < bodyStart + contentLength) return;
      const body = inputBuffer.subarray(bodyStart, bodyStart + contentLength).toString("utf8");
      inputBuffer = inputBuffer.subarray(bodyStart + contentLength);
      dispatchJson(body);
      continue;
    }

    const newline = inputBuffer.indexOf(0x0a);
    if (newline < 0) return;
    const line = inputBuffer.subarray(0, newline).toString("utf8").trim();
    inputBuffer = inputBuffer.subarray(newline + 1);
    if (line) dispatchJson(line);
  }
}

function dispatchJson(json) {
  try {
    void handleMessage(JSON.parse(json));
  } catch (error) {
    errorMessage(null, -32700, "Parse error", error.message);
  }
}

process.stdin.on("data", (chunk) => {
  inputBuffer = Buffer.concat([inputBuffer, chunk]);
  parseInput();
});

process.stdin.on("end", async () => {
  if (!shuttingDown && isRunning()) {
    shuttingDown = true;
    await stopServer({ force: true, timeout_ms: 5000 });
  }
});

for (const signal of ["SIGINT", "SIGTERM"]) {
  process.on(signal, async () => {
    if (shuttingDown) return;
    shuttingDown = true;
    try {
      await stopServer({ force: true, timeout_ms: 5000 });
    } finally {
      process.exit(0);
    }
  });
}

process.stderr.write(`scpsl-console MCP ready for project ${projectDirectory}\n`);
