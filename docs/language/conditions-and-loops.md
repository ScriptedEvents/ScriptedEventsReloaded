# Conditions and loops

## Conditions

```ser
if {@sender -> team} is "SCPs"
    Reply "SCPs cannot use this command"
    stop
elif {@sender -> health} < 50
    Reply "You are badly injured"
else
    Reply "You may continue"
end
```

Common comparisons are `is`, `isnt`, `==`, `!=`, `>`, `<`, `>=`, and `<=`.
Combine them with `and` or `or`. `!` and `not` are not condition operators in
SER.

`stop` ends the current script immediately.

## Chance

Use the returning method inside a larger condition:

```ser
if {Chance 25%}
    Print "The roll succeeded"
end
```

Use the statement for a standalone branch:

```ser
chance 25%
    Print "The roll succeeded"
else
    Print "The roll failed"
end
```

## Fixed repetition

```ser
repeat 3 with $iteration
    Print "Iteration {$iteration}"
end
```

## Conditional loop

```ser
$count = 0
while $count < 3
    $count = $count + 1
    Print $count
end
```

## Iterate over players or collections

```ser
over @alivePlayers with @player
    Hint @player 2s "Hello {@player -> name}"
end
```

Choose the loop variable prefix to match each element.

## Long-running loop

```ser
forever
    wait 1m
    Print "One minute passed"
end
```

Every `forever` loop must eventually yield using `wait`, `wait_until`, or a
yielding method. Otherwise it can monopolize the server thread.

Use `continue` to skip the rest of one iteration and `break` to leave the
nearest loop.

Next: [collections](collections.md).
