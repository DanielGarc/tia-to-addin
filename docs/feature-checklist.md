# Feature Checklist

This checklist separates what is already implemented in this repo vs what is still required to run inside TIA Portal.

## Implemented in this repository

- [x] Core operation dispatch by name
- [x] Dry-run and apply flow
- [x] Validation pipeline and blocking behavior
- [x] Change planning model (`ChangeSet`)
- [x] Markdown reports and file logger
- [x] TO-specific rule (`TO001`) and TO operations:
  - [x] `to-prefix-name`
  - [x] `to-create`
  - [x] `to-sync-settings`
  - [x] `to-bulk-configure`
- [x] TIA host integration project with direct references to `AddIn.dll` and `Siemens.Engineering.dll`
- [x] Dependency guard for required TIA assemblies
- [x] Reflection gateway to discover/apply TO name+settings changes and process create actions

## Remaining for production readiness

- [ ] Wire host service into actual Add-In entry points and command UI
- [ ] Replace or harden reflection traversal with strongly typed Siemens API calls for your exact TIA version
- [ ] Add rollback strategy and conflict handling for synchronized settings
- [ ] Validate on target TIA version(s) with real project data
- [ ] Package/deploy procedure for your team environment
