# BertBridge

BertBridge is a general BER tester control framework. It translates vendor-specific device protocols into unified application services for CLI and WPF GUI clients.

## Tech Stack

- .NET 10
- WPF
- MVVM
- SQLite
- System.CommandLine
- Serilog
- PluginSDK for device adapters

## Quick Start

Build:

```powershell
dotnet build BertBridge.sln
```

Run the mock adapter:

```powershell
dotnet run --project src/BertBridge.CLI -- device connect mock://local --name TestMock
dotnet run --project src/BertBridge.CLI -- device list
```

Run tests:

```powershell
dotnet test
```

## Notes

The current persistence model intentionally keeps only the core startup path stable. Some value objects are not fully persisted yet because EF Core 10 preview does not support nullable complex properties cleanly. See `docs/architecture/plugin-adapter-flow.md` for the current flow and next refactor target.
