# REDLINE V4 — GTX 1660 Ti / Ryzen 5 3600 profile

Windows 10/11 C#/.NET 8 utility. No Python runtime required.

## Hardware profile
Configured starting recommendations for:
- NVIDIA GeForce GTX 1660 Ti
- AMD Ryzen 5 3600 (6 cores / 12 threads)
- 16 GB DDR4

The profile recommends beginning with Roblox graphics Manual at quality 3–5, then adjusting based on observed frame rate and visual quality. Windows Graphics settings can be used to select High performance for Roblox. The app includes an NVIDIA Control Panel launcher if installed.

## Monitoring and actions
- System CPU and memory usage
- Roblox process detection, CPU and memory usage
- NVIDIA GPU utilization, VRAM, temperature (via `nvidia-smi` when available)
- Temporary Roblox process priority change with restore button
- 15-sample network test to 1.1.1.1 (not Roblox server ping)
- Shortcuts to Windows Game Mode, Graphics, Power, Network settings, Task Manager and NVIDIA Control Panel

## Build
Upload project files, preserving `.github/workflows/build-windows.yml`. In GitHub Actions run **Build REDLINE Windows EXE** and download artifact **REDLINE-v4-Windows**.

## Limits and safety
No optimizer can guarantee a particular FPS. This utility does not modify Roblox files, kill background processes, disable security, change registry values, or force an overclock. GPU telemetry is system-wide, not Roblox-only. FPS is not directly read from Roblox; use Roblox's built-in performance stats or an in-game FPS display for actual FPS measurement.
