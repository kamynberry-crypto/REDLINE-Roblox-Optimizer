# REDLINE — Roblox Optimizer

A small Windows desktop utility built with C# and .NET 8 (no Python runtime).

## Included
- Live CPU usage and system memory usage
- Roblox Player detection
- Quick Boost: temporarily sets a running Roblox Player process to `AboveNormal` priority
- Opens Windows Game Mode settings
- Launches Roblox using the installed Roblox protocol handler
- Connection check to Cloudflare DNS (`1.1.1.1`)

## Important limitations
- No tool can guarantee higher FPS or lower ping.
- The connection check is not a Roblox game-server ping test.
- Ping is mostly affected by your ISP, Wi-Fi/Ethernet, routing, distance, and the Roblox server.
- Quick Boost is temporary and resets when Roblox closes. It may not help on every PC.
- This app does not modify Roblox files, disable security, change registry settings, or close your apps.

## Build the EXE on GitHub (easy route)
1. Create a new GitHub repository.
2. Upload all files from this project ZIP, keeping the `.github/workflows` folder.
3. Open the repository's **Actions** tab.
4. Select **Build Windows EXE** and click **Run workflow** (or push a commit).
5. When the run finishes, open it and download the artifact named **Redline-Roblox-Optimizer-Windows**.
6. Extract the artifact ZIP. It contains `RedlineRobloxOptimizer.exe`.

The workflow creates a self-contained Windows x64 executable. The target PC does not need Python or a separate .NET install.

## Build locally (optional)
Install the .NET 8 SDK on Windows, open a terminal in this folder, then run:

```powershell
dotnet publish RedlineRobloxOptimizer.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```
