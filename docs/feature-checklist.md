# Feature Checklist

## Implemented

- [x] Single solution for add-in build (`TiaToAddin.sln`)
- [x] Host project references `AddIn.dll` + `Siemens.Engineering.dll`
- [x] Build generates `.addin` manifest + host DLL in one output package folder
- [x] TO create operation (`to-create`)
- [x] TO settings sync operation (`to-sync-settings`)
- [x] TO bulk configure operation (`to-bulk-configure`)

## Remaining hardening

- [ ] Validate generated `.addin` against your exact TIA version requirements
- [ ] Replace reflection access with strongly typed Siemens API calls where needed
- [ ] End-to-end test in TIA project with real TOs
