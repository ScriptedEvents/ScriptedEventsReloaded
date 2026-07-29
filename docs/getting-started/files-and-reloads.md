# Files, names, and reloads

## `.ser` and `.txt`

Use `.ser` when your hosting panel allows unknown file types. It identifies SER
scripts clearly and enables editor associations.

Use `.txt` as a compatibility format when a hosting panel does not let users
open or edit `.ser`. Both extensions have identical language semantics.

## Folders and global names

SER scans the script directory recursively, so folders can organize files.
Folders are not namespaces: one base filename identifies one script globally.

These files conflict:

```text
events/welcome.ser
commands/welcome.txt
```

SER registers neither candidate and `serstatus` lists both full paths. Rename
all but one and run `serreload`.

## Disabled files

A filename whose first character is `#` is ignored:

```text
#welcome.ser
```

This is how generated examples remain safe by default. Remove the leading `#`
or copy the file to a new globally unique name, then reload.

## New files versus edits

`serrun name` performs targeted discovery only when `name` is not already
registered. This makes a newly uploaded utility script convenient without
silently reloading unrelated files.

After editing, renaming, enabling, disabling, or deleting registered files, run:

```text
serreload
```

Reloads are transactional. SER compiles and validates the complete changed file
before replacing active event or command bindings. If an edit fails, the last
accepted version remains active and the failed candidate appears in
`serstatus`.

## Multiple sections in one file

Every `!--` declaration begins an independent section:

```ser
!-- OnEvent RoundStarted
Print "Round started"

!-- OnEvent Death
Print "A player died"
```

The file still owns one global base name. Manual diagnostics can address
sections as `filename:1`, `filename:2`, and so on. A bare name is deliberately
ambiguous when the file contains multiple sections.

Next: [methods and values](../language/methods-and-values.md).
