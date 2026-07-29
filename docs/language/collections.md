# Collections

A collection variable starts with `&` and stores an ordered list of SER values.
Use collections for inventories, map references, configuration data, and other
lists that are not groups of players. Player groups use `@` values and methods
such as `Take`, `Except`, and `Join`.

## Create, inspect, and edit

Collection indexes start at 1:

```ser
&messages = Coll.Create
Coll.Insert &messages "First message"
Coll.Insert &messages "Second message"
Coll.Insert &messages 42

if {&messages -> length} >= 2
    $second = Coll.Fetch &messages 2
    Print "Second value: {$second}"
end

if {Coll.Contains &messages "First message"}
    Coll.Remove &messages "First message"
end

Coll.RemoveAt &messages 1
```

`Coll.Insert`, `Coll.Remove`, and `Coll.RemoveAt` modify the supplied
collection. Check `length` before fetching an index that may not exist.

## Iterate

Choose a loop variable prefix that matches the contained values. The optional
second loop variable receives the current one-based index:

```ser
&messages = Coll.Create
Coll.Insert &messages "Alpha"
Coll.Insert &messages "Bravo"

over &messages with $message $index
    Print "Message {$index}: {$message}"
end
```

Collections of references require a `*` loop variable. Use
`serhelp MethodName` or property help to confirm what a collection contains.

## Return new collections

`Coll.Join` and `Coll.Subtract` return new collections rather than modifying
their inputs:

```ser
&first = Coll.Create
&second = Coll.Create
Coll.Insert &first "A"
Coll.Insert &second "B"

&combined = Coll.Join &first &second
&remaining = Coll.Subtract &combined &second
Print "Combined: {&combined -> length}; remaining: {&remaining -> length}"
```

Inserting a collection nests it as one value. Use `Coll.Join` when the goal is
to combine contents.

Next: [timing and yielding](timing-and-yielding.md).
