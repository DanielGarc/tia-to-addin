# Current State

## Simplified structure

Single solution: `TiaToAddin.sln`

- `TiaAddin.Core` (business logic)
- `TiaAddin.TiaHost` (TIA integration + packaging)

Tests are kept in `tests/` but excluded from the solution to keep production add-in builds simple.

## Build artifact behavior

Building `TiaAddin.TiaHost` now generates:

- `TiaToAddin.dll`
- `TiaToAddin.addin`

in:

- `bin/<Configuration>/net48/AddinPackage/`

The `.addin` file is generated from `AddinTemplate.addin` during build.

## Required environment setup

- Ensure `TIA_LIB_DIR` points to your installed TIA PublicAPI folder.
- Build on Windows with TIA assemblies available.

## TO features currently wired

- Create TOs (`to-create`)
- Sync settings from source TO (`to-sync-settings`)
- Bulk configure TO settings (`to-bulk-configure`)
- Prefix TO names (`to-prefix-name`)
