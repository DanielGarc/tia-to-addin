# tia-to-addin

Simplified TIA add-in solution for Technological Objects (TOs).

## What this solution builds

Build `TiaToAddin.sln` and you get a single add-in package folder under the host project output:

- `TiaToAddin.dll`
- `TiaToAddin.addin`

Output location (Debug example):

- `src/TiaAddin.TiaHost/bin/Debug/net48/AddinPackage/`

## One-time setup

In `src/TiaAddin.TiaHost/TiaAddin.TiaHost.csproj`, set `TIA_LIB_DIR` to your TIA PublicAPI folder if different from default.

## Supported TO business operations

- `to-create` (batch TO creation planning)
- `to-sync-settings` (copy settings from selected source TO to targets)
- `to-bulk-configure` (apply setting updates to multiple TOs)
- `to-prefix-name` (optional bulk naming helper)

## Build

Use Visual Studio or:

```bash
dotnet build TiaToAddin.sln -c Release
```

## Notes

- This repo keeps core logic + host packaging in one solution.
- Tests remain in `tests/` but are intentionally not part of the solution to keep add-in builds simple.
