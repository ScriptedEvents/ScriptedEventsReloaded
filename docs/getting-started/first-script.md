# Your first script

Run `serhelp start` and open the script directory it prints.

Create `hello.ser`:

```ser
Print "Hello from SER!"
```

Then use the server console or Remote Admin:

```text
serrun hello
```

You should see `Hello from SER!` followed by confirmation that the script was
requested to run.

If `hello` was not registered, `serrun` searches the entire script directory
for a new `hello.ser` or `hello.txt`, registers that file, and retries
automatically. You do not need to run `serreload` before the first execution of
a newly added utility script.

Remote Admin senders require the `ser.run` permission.

## Make the script do more

```ser
Print "Starting the greeting"
wait 1s
Print "Hello after one second"
```

Instructions normally execute from top to bottom. `wait` pauses only this
script's execution.

## If it does not run

Use:

```text
serstatus
```

It distinguishes:

- an accepted script;
- a file that failed to compile or register;
- a disabled filename or an excluded folder name beginning with `#`;
- multiple files competing for the same script name.

Next: [files, names, and reloads](files-and-reloads.md).
