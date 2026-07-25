# SCP:SL console MCP server

This dependency-free stdio MCP server lets Codex own a local `LocalAdmin.exe`
process, read its console, send server-console commands, build SER, and run a
build/restart/startup-log verification loop.

The project-scoped `.codex/config.toml` enables it for trusted Codex sessions in
this repository. Restart Codex after adding or changing MCP configuration.

## Tools

- `server_status` — inspect the owned process.
- `start_server` — start LocalAdmin and wait for the server readiness line.
- `read_console` — tail or incrementally read captured console output.
- `send_console_command` — send one server-authority command.
- `stop_server` — gracefully stop, with an explicit force fallback.
- `build_plugin` — run the existing validated SER build/deploy pipeline.
- `build_restart_and_verify` — stop, build, deploy, restart, and return startup
  logs for verification.

LocalAdmin must be launched through `start_server`; an MCP process cannot safely
attach to the stdin/stdout handles of an unrelated, already-running console.

For SER behavior that requires players, create one or more uniquely named
test players through the owned console before exercising the script:

```text
sermethod AddDummy "Codex Test Player"
```

## Configuration

Defaults match the standard Windows Steam dedicated-server install and port
`7777`. Override them with MCP environment variables if needed:

- `SCPSL_LOCAL_ADMIN_PATH`
- `SCPSL_SERVER_DIRECTORY`
- `SCPSL_SERVER_PORT`
- `SCPSL_READY_PATTERN`
- `SCPSL_STOP_COMMAND`
- `SCPSL_CONSOLE_BUFFER_LINES`

`SL_DEV_REFERENCES` and `LABAPI_PLUGINS` are forwarded by the project Codex
configuration so the existing `SER.csproj` references and post-build deployment
continue to work.

## Local test

```powershell
node --test tools/scpsl-console-mcp/server.test.mjs
```

The test uses `fake-local-admin.mjs`; it does not start the real game server.

To run an explicit real-server smoke test (start, wait, run `serhelp`, stop):

```powershell
node tools/scpsl-console-mcp/smoke-real.mjs
```
