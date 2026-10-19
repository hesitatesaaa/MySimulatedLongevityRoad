# Save/family regression tests

Run from the project root:

```powershell
python tools/save-family-tests/prepare.py
dotnet run --project tools/save-family-tests/SaveFamilyTests.csproj --configuration Release
```

Requires Python, .NET 10 and the existing Newtonsoft.Json dependency at the project reference path. The harness links the production family, archive store, currency engine, discovery, policy, metrics and deferred UI driver. prepare.py extracts current production repository, epoch, market prune and backpack shutdown methods on each run. It does not maintain separate copies of those implementations.

Unity and native game services are substitutes, including unrelated normalization and market price calculation. Results establish state/asset/probability contracts and callback scheduling, not live WorldBox acceptance, rendered UI appearance, or native save-manager/anchor integration. Existing test-service unused-field warnings are expected.
