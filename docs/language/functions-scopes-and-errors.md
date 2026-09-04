# Functions, variable lifetimes, and errors

This is a reference page. If you are learning SER in order, build a few scripts
before worrying about these details. They matter when code becomes reusable,
long-running, or difficult to diagnose.

## Inline functions

Define a function before it is used:

```ser
func $Add with $left $right
    return $left + $right
end

$sum = run $Add 2 3
Print "Sum: {$sum}"
```

The function name's prefix declares its return family. A function without a
prefix returns no value:

```ser
func Greet with $name
    Print "Hello {$name}"
end

run Greet "Scientist"
```

Inline `func`/`run` is different from the legacy `!-- Function` flag and
`RunFunc` method used for cross-file compatibility.

Name a legacy function in its flag and call that name with `RunFunc`; the name
is globally unique, so it works even when multiple function sections share a
file:

```ser
!-- Function HandleDoor
-- argument *door

LockDoor *door NoPower
```

Call it with `RunFunc HandleDoor *door`.

## Visibility and lifetime are different ideas

Words such as “local scope” and “function scope” are convenient shorthand, but
SER does **not** implement conventional lexical variable scopes.

Each running script has one local-variable table keyed by prefix and name. A
local value can be read by code executing in that script while the value is in
the table. An `if`, loop, or function body does not create a separate lookup
table or an access boundary of its own.

What changes between variable forms is mainly **who owns the value** and **when
it is removed**.

### Normal local variables

An ordinary assignment adds or replaces a value in the running script's local
table:

```ser
$message = "Hello"
@targets = @all
```

The value remains available to that script until it is replaced, explicitly
deleted, or the script execution finishes.

### Global variables

`global` writes to the round-wide variable table shared by scripts:

```ser
global $roundScore = 0
global $roundScore = $roundScore + 1
```

Reads do not use the `global` keyword:

```ser
Print "Score: {$roundScore}"
```

Global values are cleared with round state. SER prevents an active local and a
global from using the same prefix and name, so scripts cannot rely on lexical
shadowing. Use distinctive global names when unrelated systems share a server.

### `ephm`: a lifetime-limited local

`ephm` is often called an ephemeral scope, but it does not restrict where the
value can be read. It creates an ordinary entry in the same script-local table
and registers that entry for removal when the containing statement finishes.

```ser
func ShowTemporary
    # This function can read the value because it is still alive in the
    # calling script's local table.
    Print "Temporary: {$temporary}"
end

chance 100%
    ephm $temporary = "visible while this statement is active"
    run ShowTemporary
end

# This prints false because the chance statement has finished.
Print "Still exists: {VarExists $temporary}"
```

Important consequences:

- `ephm` must be inside a statement; it is not valid at the script root;
- nested code and functions can read it while its owning statement is active;
- leaving an `if`, function, or other owning statement removes it;
- a loop removes its ephemeral values at the end of each iteration, including
  paths which leave with `continue` or `break`;
- it is a lifetime tool, not a privacy or lexical-shadowing mechanism.

Use `ephm` for scratch values which should be cleaned up predictably. Use an
ordinary local when the rest of the script still needs the value.

### Function arguments

Function arguments are temporarily added to the same script-local table and
removed when that function call finishes. They behave like call-lifetime
locals, not like variables hidden in a separate lexical environment.

Avoid designs which depend on shadowing the same prefixed name across active
calls. SER's model is intentionally smaller than a language with stack frames,
closures, and nested lexical environments.

### Explicit deletion

Use `delete` when a local or global value should stop existing before its normal
lifetime ends:

```ser
delete $temporaryValue
```

`VarExists` can check a value which may already have been removed.

## Prevent errors first

Before handling an exception:

- require event data with `-- require`;
- validate optional variables with `VarExists`;
- check a reference's `isInvalid` property;
- inspect collection length before fetching an index;
- revalidate players and references after a long wait.

## Handle a recoverable error

```ser
attempt
    &items = Coll.Create
    Print {Coll.Fetch &items 2}
on_error with $message $type
    Print "Could not read the item: {$message}"
    Print "Error type: {$type}"
end
```

Keep `attempt` blocks small so it is obvious which instruction failed.
`attempt` handles errors caused by script input or game state. The `stop`
keyword still stops the script, and an internal SER failure still receives an
identifier in the server console instead of being hidden by `on_error`.
Unexpected internal SER failures are presented with a short identifier; full
technical details remain in the server console.

Next reference: [debugging](../guides/debugging.md).
