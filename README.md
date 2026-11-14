# REDLINE V3 — Roblox Performance Center

Windows 10/11 utility written in C#/.NET 8. No Python runtime required.

## New NVIDIA telemetry
- Reads GPU name, utilization, VRAM used/total, temperature and driver version using the NVIDIA `nvidia-smi` command-line utility.
- If `nvidia-smi` is unavailable, the NVIDIA cards show NOT FOUND. Install/update the official NVIDIA driver and reboot; some driver packages may not expose the utility in PATH.
- This is overall GPU telemetry, not per-Roblox GPU utilization.

## Other features
- Live system CPU and RAM usage
- Roblox Player detection, process CPU and memory monitoring
- Temporary process priority boost and restore button
- 15-sample connection test to 1.1.1.1 with average, variation and packet loss estimate
- Shortcuts to Windows Game Mode, Graphics, Power, Network settings and Task Manager

## Build
Upload the project contents to GitHub, preserving `.github/workflows/build-windows.yml`. Open Actions, choose **Build REDLINE Windows EXE**, run it, then download the `REDLINE-v3-Windows` artifact.

## Limits
No optimizer can guarantee higher FPS. This version does not change Roblox files, disable security, kill background processes, or make risky registry changes. The ping test is not a Roblox server ping. NVIDIA readings are system-wide.
