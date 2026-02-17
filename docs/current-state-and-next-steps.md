# Current State and Next Steps

## What you have right now

You now have two layers:

1. `TiaAddin.Core` (portable logic)
2. `TiaAddin.TiaHost` (TIA-dependent integration layer)

## Included components

### `TiaAddin.Core`
- Data contracts (`AutomationItem`, `OperationRequest`, `OperationResult`, `ChangeSet`)
- Extension points (`IProjectGateway`, `IOperation`, `IValidationRule`, `IReportWriter`, `ILogger`)
- Execution orchestrator (`AddinEngine`)
- TO-focused operations for business logic:
  - create TOs in batch (`to-create`)
  - sync settings from selected source TO to targets (`to-sync-settings`)
  - bulk configure multiple TOs (`to-bulk-configure`)
  - optional naming helper (`to-prefix-name`)
- TO validation rule `TO001`

### `TiaAddin.TiaHost`
- Direct assembly references to:
  - `AddIn.dll`
  - `Siemens.Engineering.dll`
- Dependency guard that verifies both DLLs are present/loadable
- Reflection-based TIA project gateway that discovers TOs, loads setting snapshots, applies setting updates, and processes TO create actions
- Service methods in `TechnologicalObjectAddinService` for create/sync/bulk-configure flows

## Remaining work on your Windows machine

- Adjust `TIA_LIB_DIR` in `TiaAddin.TiaHost.csproj` to your installed TIA version path if needed.
- Replace/extend reflection mapping with strongly typed Siemens API calls for the exact TO settings structures you use.
- Bind `TechnologicalObjectAddinService` methods into your actual Add-In entry points and UI commands.
- Build/deploy host add-in DLL and run in-TIA smoke tests.

## Suggested first in-TIA tests

1. Create 2-3 TOs using `to-create` in dry-run, then apply.
2. Change one TO manually in TIA and run `to-sync-settings` from that TO to targets.
3. Run `to-bulk-configure` for 1-2 keys across selected TOs.
4. Verify generated report output and resulting TO configuration values.
