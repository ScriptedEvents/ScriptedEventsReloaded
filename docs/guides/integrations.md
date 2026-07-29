# Optional integrations

SER loads without optional frameworks. Integration methods appear only when
their framework is present; unavailable methods remain visible in generated
tooling with their framework requirement.

## ProjectMER

ProjectMER support includes:

- map and object operations through `MER.*` methods;
- schematic references and animations;
- `!-- OnPMER EventName` event sections;
- `serhelp pmerevents` for the events exposed by the installed version.

Inspect individual operations before use:

```text
serhelp MER.PlayAnimation
serhelp MER.CreateObject
serhelp OnPMER
```

Do not assume a method from a different ProjectMER or SER revision has the same
arguments.

## SER custom roles

`CRole.*` methods build roles from:

1. a role identifier and display name;
2. a base SCP:SL role;
3. a spawn system;
4. optional callbacks and properties.

Use:

```text
serhelp methods CustomRole
serhelp CRole.Register
serhelp CRole.CreateChanceSpawnSystem
serhelp OnCRole
```

The build-validated [`customRoles.ser`](../../Example%20Scripts/customRoles.ser)
shows registration, callbacks, and spawn behavior together.

## Other frameworks

The exact optional method set is generated from the current assembly. Run
`serhelp methods all` and look for the framework requirement on a specific
method rather than relying on a copied list.
