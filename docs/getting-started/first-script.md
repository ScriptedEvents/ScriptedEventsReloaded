# Make the server say something

You are about to write a complete SER script. No variables, loops, or language
theory yet—just a result you can see immediately.

## 1. Create the file

Run `serhelp start` and open the script directory it prints. Create a file named
`hello.ser` and put this inside:

```ser
Reply "Hello from my first SER script!"
```

`Reply` is a **method**: an instruction that asks SER to do something.

## 2. Run it

Use the server console or Remote Admin:

```text
serrun hello
```

Use the script name, not `hello.ser` and not its full path. You should see your
message followed by confirmation that `hello` was requested to run.

Remote Admin senders need the `ser.run` permission.

## 3. Make players see it

Add another method:

```ser
Reply "Sending a message to everyone..."
Broadcast @all 5s "This server can be scripted!"
```

Run `serrun hello` again. SER refreshes the file before executing it, so the new
broadcast should appear for every connected player.

Read the second line from left to right:

- `Broadcast` is what SER should do;
- `@all` means every player;
- `5s` is how long the message stays visible;
- the quoted text is what they see.

That pattern—**method, then arguments**—already covers a surprising amount of
SER.

## Try one change

Change `5s` to `10s`, edit the message, and run the script again. If you want a
smaller on-screen message, ask SER how `Hint` works:

```text
serhelp Hint
```

## If nothing happens

Run:

```text
serstatus
```

It tells you whether the file was accepted, failed to compile, is disabled by a
leading `#`, lives inside a disabled folder, or conflicts with another file
using the same script name.

Next: [discover methods that change the game](../tutorial/methods.md).
