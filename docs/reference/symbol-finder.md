# Find a SER symbol

Search by name or by words describing what you want:

```text
serhelp find heal
serhelp find player health
```

The command shows up to ten matches. Each result names its kind, gives a short
description, and points to a help command. Event variables and properties also
show which event or object they belong to. Results that need an integration say
so. Finding a symbol does not run a method or read a live property.

## For tool authors

`SerSymbolCatalogue.Capture()` reads initialized SER registries. Capture on the
host's registry thread after registration finishes, then use the immutable
snapshot for searches. Rebuild the snapshot after registrations change.
The catalogue includes methods and aliases, events and event variables, enums,
registered object and basic value properties, keywords, global variable names,
and flags. It does not read variable values or running scripts' local variables.
Optional event metadata is present only when its assembly has been discovered;
tooling hosts can use the existing optional event loaders before capture.

```csharp
var finder = new SerSymbolFinder(SerSymbolCatalogue.Capture());
var result = await finder.FindAsync(query, scorer: null, limit: 10,
    cancellationToken: cancellationToken);
```

Supply `ISerRelevanceScorer? scorer` per call. Null performs deterministic local
search. SER does not store a global scorer or depend on a scoring provider.
Canonical IDs include the symbol kind and, where needed, its declaring type and
assembly name. They do not include assembly versions. Results retain the original
catalogue records; a scorer can only assign relevance numbers to their IDs.

Local ranking uses exact names and aliases, prefixes, name words, substrings,
description words, then edit distance for spelling mistakes. Ties use ordinal
symbol IDs. Compiler spelling suggestions are unchanged.

Strong name matches offer the first 128 candidates for scoring. Weaker queries
offer the full snapshot, including candidates with no shared words. Exact name
and alias matches remain first after scoring. All candidates must receive one
finite score in [0,1]; missing, duplicate, unknown or invalid scores discard the
entire scoring response. Exceptions and timeouts return the same local results
as null scoring. Caller cancellation is propagated. The default scoring deadline
is 12 seconds, configurable per call up to 60 seconds; results are capped at 50
and queries at 300 characters.

Run the isolated search tests with:

```text
dotnet test Tooling/SymbolFinder.Tests/SymbolFinder.Tests.csproj -c Release
```
