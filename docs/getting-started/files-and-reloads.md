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

## Disabled files and folders

A file or folder name whose first character is `#` is ignored:

```text
#welcome.ser
#old-scripts/
```

SER does not search inside a disabled folder, so all of its nested files and
folders are excluded from discovery. This is how generated examples remain safe
by default. Remove the leading `#` from a file or folder (or copy the file to a
new globally unique name), then reload. `serstatus` lists disabled files and
excluded folders separately; it does not enumerate files inside excluded folders.

SER also does not follow symbolic links or directory junctions while scanning.
This keeps discovery inside the configured SER directory and prevents link cycles
from stalling a reload. `serstatus` lists every linked folder it skipped.

## Targeted and full reloads

Every file-backed execution request performs a targeted refresh immediately before
the script runs. This includes `serrun`, events, custom commands, callbacks, triggers,
and calls from another script. New files and edits therefore take effect on their
next direct execution without reloading unrelated files.

Round restart refreshes the complete script directory. To refresh everything immediately,
including renamed, enabled, disabled, or deleted files and bindings which cannot trigger
yet, run:

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
