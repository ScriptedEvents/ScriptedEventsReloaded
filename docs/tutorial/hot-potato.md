# Build a Hot Potato event

This is where the smaller ideas pay off. We will build an event which waits for
a random moment, gives one non-SCP player a hot potato, and explodes them if
they keep it.

The complete [`hotPotato.ser`](../../Example%20Scripts/hotPotato.ser) file is
compiled during every SER build. Generate it with `serexamples` if you want to
edit a disabled copy on your server.

## 1. Decide whether the event joins this round

```ser
!-- OnEvent RoundStarted

chance 50%
    Print "Hot Potato event will not be loaded"
    stop
end

Print "Hot Potato event was loaded"
Broadcast @all 5s "Be ready for a Hot Potato!"
```

The flag starts the script with the round. Half the time it stops immediately;
the other half announces itself.

## 2. Repeat, but always give the server room to breathe

```ser
forever
    wait {ToDuration {Random 30 90} seconds}

    # The rest of the event goes here.
end
```

`Random 30 90` chooses the delay. `ToDuration` turns that number into something
`wait` accepts.

Every path through a `forever` loop must eventually wait or otherwise yield.
Without that pause, a script could monopolize the server thread.

## 3. Choose a carrier

Inside the loop:

```ser
@potatoCarrier = Take {Except @alivePlayers @scpPlayers} 1

if {AmountOf @potatoCarrier} is 0
    continue
end
```

`Except` means **alive players, except SCPs**. `Take` selects one of those
players. If nobody qualifies, `continue` skips the rest of this iteration and
returns to the random wait.

## 4. Keep hold of the actual item

```ser
*item = AdvGiveItem @potatoCarrier GunA7

if {*item -> isInvalid}
    continue
end

Hint @potatoCarrier 3s "YOU HAVE THE HOT POTATO!<br>DROP IT OR DIE!"
wait 6s
```

`AdvGiveItem` returns a reference to the item it created. References use `*`
because they point at live game objects rather than plain text or numbers.

The validity check matters: an inventory may be full, a player may disconnect,
or the object may otherwise fail to exist. Never assume a reference survived a
wait.

## 5. Resolve and clean up

```ser
if {*item -> inInventory}
    Explode {*item -> currentOwner}
    Broadcast @all 5s "Player {*item -> currentOwner -> name} has failed the Hot Potato!"
end

AdvDestroyItem *item

chance 70%
    Broadcast @all 5s "The Hot Potato will return soon!"
    continue
else
    Broadcast @all 5s "The Hot Potato got tired and will not return..."
    stop
end
```

If the potato is still held, its current owner explodes. Either way, the item
is destroyed so the event does not leave a special weapon behind. Finally, the
script decides whether to run another iteration.

## What you just used

This one event combines:

- an event flag;
- ordinary and returning methods;
- predefined and custom player values;
- conditions, chance, and timing;
- a long-running loop;
- a live object reference and properties;
- defensive checks and cleanup.

None of those ideas had to come first. They became useful because the event
needed them.

## Where to go next

- Pick another [build-validated example](../guides/examples.md) and change one
  behavior at a time.
- Use the [conditions and loops reference](../language/conditions-and-loops.md)
  for every loop form.
- Read [collections](../language/collections.md) when you need an ordered list
  which is not a player group.
- Read [functions, lifetimes, and errors](../language/functions-scopes-and-errors.md)
  when a script becomes large enough to organize.
- Keep `serhelp` open. It is the authoritative method and event reference for
  the build running on your server.
