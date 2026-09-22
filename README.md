# FileSystemMonitor

## Architecture

- `FileSystemMonitor.Web` - ASP.NET Core MVC UI
- `FileSystemMonitor.Api` - REST API + application service
- `FileSystemMonitor.Contracts` - shared DTOs/enums
- `.vscode/launch.json` - starts Web + API together
- `.vscode/tasks.json` - build/test tasks

## Versioning behavior

### First analysis
A first analysis creates **Version 1** and displays every discovered file.

### Next analysis
A second analysis creates **Version 2** and compares the current filesystem state with Version 1:

- Added
- Modified
- Removed
- Unchanged

Every analysis creates a persistent JSON snapshot under:

`FileSystemMonitor.Api/Data/Snapshots/<folder-hash>/`

The analyzer reuses a previous SHA-256 hash when file size and last-write timestamp are unchanged.

## Run in VS Code

1. Install .NET 8 SDK.
2. Open the repository folder in VS Code.
3. Run:
   `dotnet restore`
4. Run:
   `dotnet build`
5. Press F5 and choose **Web + API**.

Web:
`https://localhost:7002`

API:
`https://localhost:7001/`

If HTTPS certificate trust is a problem:

`dotnet dev-certs https --clean`

then:

`dotnet dev-certs https --trust`

## Example

Use a real directory such as:

`C:\Temp\FileMonitorDemo`

Click Analyze.

Then modify a file, add a file, or delete a file and click Analyze again.


