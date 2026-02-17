# tia-to-addin

TIA add-in codebase split into:
- `TiaAddin.Core` (portable operation/validation engine)
- `TiaAddin.TiaHost` (TIA-dependent host bridge using `AddIn.dll` and `Siemens.Engineering.dll`)

## Small function overview

### Core (`src/TiaAddin.Core`)
- `AddinEngine` executes: load selection -> validate -> plan/apply changes -> write report.
- Operations:
  - `prefix-name`
  - `replace-name`
  - `uppercase-name`
  - `to-prefix-name`
  - `to-create` (create TOs in batch)
  - `to-sync-settings` (copy settings from source TO to targets)
  - `to-bulk-configure` (apply config values to many TOs)
- Rules:
  - `VAL001` required name
  - `VAL002` max-length warning
  - `VAL003` duplicate name
  - `TO001` TO-only validation
- Infrastructure:
  - `FileReportWriter`
  - `TextFileLogger`

### TIA host (`src/TiaAddin.TiaHost`)
- Direct references to:
  - `AddIn.dll`
  - `Siemens.Engineering.dll`
- `TiaDependencyGuard` verifies those DLLs load.
- `TiaReflectionGateway` maps TOs into core `AutomationItem`, loads TO settings, applies setting updates, and handles TO create actions.
- `TiaAddinBootstrap` and `TechnologicalObjectAddinService` provide TO-focused execution entry points.

## Current status
- ✅ Core and TIA host bridge scaffolding are implemented.
- ⚠️ Final production wiring in your specific TIA environment (entry points/UI/deployment) is still required.

## Documentation
- Feature/status checklist: `docs/feature-checklist.md`
- Integration next steps: `docs/current-state-and-next-steps.md`
- Test inventory and how to run tests: `docs/tests.md`
