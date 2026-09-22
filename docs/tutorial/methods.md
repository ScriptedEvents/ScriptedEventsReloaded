# Methods change the game

Methods are the fun part of SER. They broadcast messages, heal players, move
them between rooms, change roles, control doors, start effects, call webhooks,
and much more.

You have already used two:

```ser
Reply "The script is running."
Broadcast @all 5s "Hello, facility!"
```

## Find a method instead of memorizing one

Search by name or by what you want to change:

```text
serhelp find player health
```

This shows up to ten matches, including methods, events and properties. Follow
the help command beside a result to learn how to use it.

The running server carries its own reference. Start with the shorter beginner
list:

```text
serhelp methods essential
```

Browse a category when you know the kind of change you want:

```text
serhelp methods Player
serhelp methods Door
serhelp methods Broadcast
```

Then inspect one method:

```text
serhelp GiveItem
```

The answer tells you the argument order, which values are accepted, which
arguments are optional, and which errors are expected. That output is generated
from the installed SER build, so it is safer than copying a method call from an
old post.

## Read a method from left to right

Suppose you want to give everyone a coin:

```ser
GiveItem @all Coin
```

- `GiveItem` is the method.
- `@all` answers **who?**
- `Coin` answers **what?**

Now add a message:

```ser
GiveItem @all Coin
Hint @all 4s "A coin has appeared in your inventory."
```

Methods use exact capitalization. `GiveItem` works; `giveitem` and `GIVEITEM`
do not. Text containing spaces belongs in double quotes.

Ammo uses the same player-first order. This gives every player 30 rounds of
9 mm ammo:

```ser
GiveAmmo @all Ammo9x19 30
```

Use `GetAmmo` when the script needs the current amount. `DestroyAmmo` removes
rounds without dropping them, while `DropAmmo` leaves an ammo pickup in the
world:

```ser
$ammo = GetAmmo @sender Ammo9x19
DestroyAmmo @sender Ammo9x19 10
DropAmmo @sender Ammo9x19 10
```

## Build a tiny admin utility

Create `classd.ser`:

```ser
Reply "Turning every player into Class-D."
SetRole @all ClassD
GiveItem @all Medkit
Broadcast @all 6s "New life, new orange uniform, free medkit."
```

Run it with `serrun classd` on a test round. Four lines are enough to replace a
small one-purpose plugin.

> Methods can have large gameplay effects. Test role changes, damage, round
> controls, and map changes away from a public round first.

## The limitation you should feel

Right now this script runs only when an administrator enters `serrun classd`.
What if players should have a command of their own? What if the script should
run when a player joins?

That is what flags solve.

Next: [make a command or react to an event](flags.md).
