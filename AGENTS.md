# Build handoff

For any implementation change, run a Release build as the final verification
step before handoff:

```powershell
dotnet build SER.csproj -c Release --no-restore
```

The resulting DLL is deployed directly to the server, so do not hand off a
change until this build has completed successfully and the Release artifact is
ready for a server restart.

# Human-facing SER wording

Use the user-facing tutorial series in `docs/tutorial/` as the house style for
config descriptions, commands, help text, errors, documentation, release notes,
and handoff messages.

- Start with what the server owner or script author will see, gain, or need to
  do. Explain the internal mechanism only when it helps them make a decision.
- Prefer short, common words. Assume the reader understands the game and the
  result they want, but not SER internals.
- Introduce one idea at a time: show the smallest useful action or example,
  explain only its new pieces, and put a warning next to the risky step.
- Keep implementation terms such as frame yields, synchronous evaluation,
  manifests, and line-ending normalization in code comments or technical
  reference material. Do not lead user-facing copy with them.
- Remove filler such as "focused", "robust", "cleanly", or "improved" unless
  the sentence says exactly what changed for the user.
- For a bug, say: what the user tried, what went wrong, and what works now.
  Add deeper cause details only when asked or when they change the workaround.
- For a setting, say: what changes when it is on, the practical tradeoff, and
  the recommended choice. Do not describe it through its implementation.
- For a handoff, lead with the result. Then state any action the user must take,
  followed by verification. Keep developer-only detail out of the main summary.
- Put exhaustive behavior and edge cases in reference material rather than
  interrupting the first explanation.

Examples:

- Avoid: "Inserts frame yields while scripts execute, reducing the risk that a
  tight script stalls the server."
  Prefer: "Slows scripts down slightly to help stop them from freezing the
  server. Keep this enabled unless you have checked every active script."
- Avoid: "The synchronous inline expression could not resume after the
  artificial safety yield."
  Prefer: "With SafeScripts on, SER paused the method and checked its answer too
  early."
- Avoid: "Manifest line endings are normalized for deterministic cross-platform
  output."
  Prefer: "Tooling builds now produce the same files on Windows and Linux."

Before handing off human-facing wording, ask: can a server owner tell what
changed and what they should do without knowing how SER is implemented? If not,
rewrite it.
