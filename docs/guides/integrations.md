# Optional integrations

SER loads without optional frameworks. Integration methods appear only when
their framework is present; unavailable methods remain visible in generated
tooling with their framework requirement.

SER 1.0 was live-tested with the following integration baseline:

| Framework | Tested version | SER surface |
| --- | ---: | --- |
| ProjectMER | 2025.11.2.1 | `MER.*` methods and `!-- OnPMER` events |
| UncomplicatedCustomRoles | 9.5.1 | `UCR.*` methods |
| Callvote | 6.9.0 | `Vote.*` methods |

All three LabAPI integrations were also loaded together, exercised, and kept
enabled across a round restart. They are optional: installing, removing, or
updating one does not make the others a SER requirement. A newer framework
version may still work, but the versions above are the compatibility baseline
verified for the 1.0 release.

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

## UncomplicatedCustomRoles

When UncomplicatedCustomRoles is installed, `UCR.*` methods operate on its
registered roles. Discover the installed surface instead of assuming role IDs
or behavior:

```text
serhelp methods UncomplicatedCustomRoles
serhelp UCR.GetRole
serhelp UCR.GetPlayersWithRole
serhelp UCR.SetRole
```

SER's own `CRole.*` system and the optional `UCR.*` integration are separate.

## Callvote

Callvote adds `Vote.*` methods for constructing and running votes:

```text
serhelp Vote.CreateOption
serhelp Vote.Start
serhelp Vote.StartAndWait
```

Use `_` for `Vote.Start`'s optional asking player when the vote is a general
server question. SER uses the server host as Callvote's initiator in that case.

## Other frameworks

The exact optional method set is generated from the current assembly. Run
`serhelp methods all` and look for the framework requirement on a specific
method rather than relying on a copied list.

## Network operations

HTTP requests, Discord webhooks, and IP-information lookups share the timeout
and maximum response-size limits in the SER plugin configuration. Requests
participate in normal script execution: `attempt` can handle their errors, and
stopping or reloading a waiting script cancels its pending work.
