# Tests

## Test projects

- `tests/TiaAddin.Core.Tests`
  - `AddinEngineTests`
    - dry-run plans changes and does not apply
    - non-dry-run applies changes
    - validation errors block execution when configured
  - `TechnologicalObjectFeaturesTests`
    - `TO001` flags non-TO items
    - `to-prefix-name` changes only TO items
    - `to-create` plans TO creation actions
    - `to-sync-settings` copies settings from source TO to targets
    - `to-bulk-configure` applies key/value updates to many TOs

## How to run

From repo root:

```bash
dotnet test
```

## Notes

- This container may not have `dotnet` installed.
- Run tests on your Windows machine where your .NET SDK and TIA dependencies are available.
