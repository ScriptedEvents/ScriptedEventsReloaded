# Security policy

## Supported versions

Security fixes are made for the latest SER release. Update to the latest
release before reporting a problem that may already be fixed.

## Report a security problem

Please use a
[private GitHub security advisory](https://github.com/ScriptedEvents/ScriptedEventsReloaded/security/advisories/new).
Do not open a public issue for a problem that could expose a server, its files,
or its players.

Include the SER version, plugin host, SCP:SL version, the smallest script that
shows the problem, and the matching server log. Remove webhook URLs, passwords,
tokens, IP addresses, and player identifiers. If a secret was exposed, replace
it before sending the report.

## Script safety

Install scripts only from people you trust. An SER script can run server
commands, make network requests, and change files that SER is allowed to use.
Review a script and its custom configuration before enabling it on a live
server.
