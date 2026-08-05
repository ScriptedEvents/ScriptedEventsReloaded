# Build handoff

For any implementation change, run a Release build as the final verification
step before handoff:

```powershell
dotnet build SER.csproj -c Release --no-restore
```

The resulting DLL is deployed directly to the server, so do not hand off a
change until this build has completed successfully and the Release artifact is
ready for a server restart.
