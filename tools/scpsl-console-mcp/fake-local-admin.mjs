#!/usr/bin/env node

import readline from "node:readline";

const port = process.argv.at(-1);
process.stdout.write(`Fake LocalAdmin starting on port ${port}\n`);
setTimeout(() => process.stdout.write("Waiting for players...\n"), 25);

const input = readline.createInterface({
  input: process.stdin,
  terminal: false,
});

input.on("line", (line) => {
  if (line === "exit") {
    process.stdout.write("Fake server stopping\n");
    process.exit(0);
  }
  process.stdout.write(`Executed: ${line}\n`);
});
