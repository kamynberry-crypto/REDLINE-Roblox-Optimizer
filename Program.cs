using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace RedlineRobloxOptimizer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private static readonly Color Back = Color.FromArgb(13, 13, 16);
    private static readonly Color Panel = Color.FromArgb(26, 26, 32);
    private static readonly Color Red = Color.FromArgb(235, 45, 55);

    private readonly Label cpu = ValueLabel();
    private readonly Label memory = ValueLabel();
    private readonly Label roblox = ValueLabel();
    private readonly Label robloxCpu = ValueLabel();
    private readonly Label robloxMemory = ValueLabel();
    private readonly Label gpu = ValueLabel();
    private readonly Label gpuMemory = ValueLabel();
    private readonly Label gpuTemp = ValueLabel();
    private readonly Label ping = ValueLabel();
    private readonly NvidiaReader nvidia = new();
    private readonly Label status = new();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 1500 };
    private readonly CpuReader cpuReader = new();
    private TimeSpan? previousRobloxCpu;
    private DateTime previousRobloxSample;

    public MainForm()
    {
        Text = "REDLINE | Roblox Performance Center";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(920, 870);
        MinimumSize = new Size(920, 870);
        MaximumSize = new Size(920, 870);
        BackColor = Back;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var header = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = Color.FromArgb(20, 20, 25) };
        header.Controls.Add(new Label { Text = "REDLINE", ForeColor = Red, Font = new Font("Segoe UI", 29, FontStyle.Bold), AutoSize = true, Location = new Point(24, 9) });
        header.Controls.Add(new Label { Text = "ROBLOX PERFORMANCE CENTER  //  V2", ForeColor = Color.Silver, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Location = new Point(29, 65) });
        Controls.Add(header);

        var cards = new TableLayoutPanel { Location = new Point(22, 122), Size = new Size(860, 240), ColumnCount = 3, RowCount = 3, BackColor = Back };
        for (int i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
        cards.Controls.Add(Card("SYSTEM CPU", cpu), 0, 0);
        cards.Controls.Add(Card("SYSTEM MEMORY", memory), 1, 0);
        cards.Controls.Add(Card("ROBLOX PLAYER", roblox), 2, 0);
        cards.Controls.Add(Card("ROBLOX CPU", robloxCpu), 0, 1);
        cards.Controls.Add(Card("ROBLOX MEMORY", robloxMemory), 1, 1);
        cards.Controls.Add(Card("NVIDIA GPU LOAD", gpu), 2, 1);
        cards.Controls.Add(Card("GPU MEMORY USED", gpuMemory), 0, 2);
        cards.Controls.Add(Card("GPU TEMPERATURE", gpuTemp), 1, 2);
        cards.Controls.Add(Card("NETWORK TEST", ping), 2, 2);
        Controls.Add(cards);

        var actions = new Panel { Location = new Point(22, 378), Size = new Size(860, 190), BackColor = Panel };
        actions.Controls.Add(SectionTitle("SAFE PERFORMANCE ACTIONS", 18, 15));
        AddButton(actions, "BOOST ROBLOX", 20, 58, 190, ApplyBoost);
        AddButton(actions, "RESTORE NORMAL", 225, 58, 190, RestorePriority);
        AddButton(actions, "TEST CONNECTION", 430, 58, 190, async () => await RunPingTest());
        AddButton(actions, "WINDOWS GAME MODE", 635, 58, 205, () => OpenSettings("ms-settings:gaming-gamemode"));
        AddButton(actions, "GRAPHICS SETTINGS", 20, 119, 190, () => OpenSettings("ms-settings:display-graphics"));
        AddButton(actions, "POWER SETTINGS", 225, 119, 190, () => OpenSettings("ms-settings:powersleep"));
        AddButton(actions, "NETWORK SETTINGS", 430, 119, 190, () => OpenSettings("ms-settings:network-status"));
        AddButton(actions, "TASK MANAGER", 635, 119, 205, () => Process.Start(new ProcessStartInfo("taskmgr.exe") { UseShellExecute = true }));
        Controls.Add(actions);

        var profile = new Panel { Location = new Point(22, 582), Size = new Size(860, 100), BackColor = Panel };
        profile.Controls.Add(SectionTitle("YOUR HARDWARE PROFILE", 18, 12));
        profile.Controls.Add(new Label { Text = "GTX 1660 Ti  |  Ryzen 5 3600  |  16 GB DDR4", ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(20, 43) });
        profile.Controls.Add(new Label { Text = "Suggested starting point: Roblox graphics Manual, quality 3–5; Windows Graphics preference = High performance for Roblox.", ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 8.5F), AutoSize = true, Location = new Point(20, 68) });
        AddButton(profile, "NVIDIA CONTROL PANEL", 635, 25, 205, OpenNvidiaPanel);
        Controls.Add(profile);

        var tips = new Panel { Location = new Point(22, 694), Size = new Size(860, 105), BackColor = Panel };
        tips.Controls.Add(SectionTitle("QUICK CHECKLIST", 18, 12));
        tips.Controls.Add(new Label
        {
            Text = "1. In Roblox: Esc → Settings → set Graphics Mode to Manual, then lower Graphics Quality.\n" +
                   "2. Close heavy apps you recognize in Task Manager (do not end unfamiliar Windows processes).\n" +
                   "3. Prefer Ethernet or strong 5 GHz Wi-Fi; pause downloads and cloud sync while playing.",
            ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 9.2F), AutoSize = true, Location = new Point(20, 43)
        });
        Controls.Add(tips);

        status.Text = "Ready. REDLINE monitors and adjusts only Roblox process priority; it does not alter game files.";
        status.ForeColor = Color.Gainsboro;
        status.Location = new Point(24, 812);
        status.Size = new Size(850, 30);
        Controls.Add(status);

        var note = new Label
        {
            Text = "Ping test uses Cloudflare DNS (1.1.1.1), not the Roblox game server. No optimizer can guarantee higher FPS or lower ping.",
            ForeColor = Color.FromArgb(155, 155, 165), Font = new Font("Segoe UI", 8.2F),
            Location = new Point(24, 840), Size = new Size(850, 24)
        };
        Controls.Add(note);

        timer.Tick += (_, _) => RefreshStats();
        timer.Start();
        RefreshStats();
    }

    private static Label ValueLabel() => new()
    {
        Text = "…", ForeColor = Red, Font = new Font("Segoe UI", 18, FontStyle.Bold),
        AutoSize = true, Location = new Point(15, 42)
    };

    private static Control Card(string title, Label value)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Panel, Margin = new Padding(0, 0, 8, 8) };
        p.Controls.Add(new Label { Text = title, ForeColor = Color.Silver, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(14, 12) });
        p.Controls.Add(value);
        return p;
    }

    private static Label SectionTitle(string text, int x, int y) =>
        new() { Text = text, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(x, y) };

    private static void AddButton(Control parent, string text, int x, int y, int width, Action action)
    {
        var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 43), BackColor = text == "BOOST ROBLOX" ? Red : Color.FromArgb(57, 57, 66), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0;
        b.Click += (_, _) => { try { action(); } catch { } };
        parent.Controls.Add(b);
    }

    private void RefreshStats()
    {
        var systemCpu = cpuReader.Read();
        cpu.Text = systemCpu.HasValue ? $"{systemCpu.Value:0}%" : "N/A";

        var mem = MemoryReader.Read();
        memory.Text = mem.HasValue ? $"{mem.Value:0}% used" : "N/A";

        var gpuInfo = nvidia.Read();
        if (gpuInfo is null)
        {
            gpu.Text = "NOT FOUND";
            gpuMemory.Text = "—";
            gpuTemp.Text = "—";
        }
        else
        {
            gpu.Text = $"{gpuInfo.Value.Utilization}%";
            gpuMemory.Text = $"{gpuInfo.Value.MemoryUsed} / {gpuInfo.Value.MemoryTotal} MB";
            gpuTemp.Text = $"{gpuInfo.Value.Temperature} °C";
        }

        var players = Process.GetProcessesByName("RobloxPlayerBeta");
        if (players.Length == 0)
        {
            roblox.Text = "NOT RUNNING";
            roblox.ForeColor = Color.OrangeRed;
            robloxCpu.Text = "—";
            robloxMemory.Text = "—";
            previousRobloxCpu = null;
            return;
        }

        roblox.Text = "RUNNING";
        roblox.ForeColor = Color.LightGreen;
        try
        {
            var p = players.OrderByDescending(x => x.WorkingSet64).First();
            robloxMemory.Text = $"{p.WorkingSet64 / 1024d / 1024d:0} MB";
            var now = DateTime.UtcNow;
            var total = p.TotalProcessorTime;
            if (previousRobloxCpu.HasValue)
            {
                var seconds = (now - previousRobloxSample).TotalSeconds;
                var cpuSeconds = (total - previousRobloxCpu.Value).TotalSeconds;
                var percent = seconds > 0 ? cpuSeconds / seconds / Environment.ProcessorCount * 100 : 0;
                robloxCpu.Text = $"{Math.Clamp(percent, 0, 100):0}%";
            }
            else robloxCpu.Text = "Sampling…";
            previousRobloxCpu = total;
            previousRobloxSample = now;
        }
        catch
        {
            robloxCpu.Text = "N/A";
            robloxMemory.Text = "N/A";
        }
    }

    private void ApplyBoost()
    {
        var list = Process.GetProcessesByName("RobloxPlayerBeta");
        if (list.Length == 0) { status.Text = "Open Roblox first, then click BOOST ROBLOX."; return; }
        int changed = 0;
        foreach (var p in list)
        {
            try { p.PriorityClass = ProcessPriorityClass.AboveNormal; changed++; }
            catch { }
        }
        status.Text = changed > 0
            ? "Roblox priority set to Above Normal. This is temporary and may not improve performance on every PC."
            : "Windows did not allow the priority change. No system settings were changed.";
    }

    private void RestorePriority()
    {
        var list = Process.GetProcessesByName("RobloxPlayerBeta");
        int changed = 0;
        foreach (var p in list)
        {
            try { p.PriorityClass = ProcessPriorityClass.Normal; changed++; }
            catch { }
        }
        status.Text = changed > 0 ? "Roblox priority restored to Normal." : "No running Roblox Player process was found.";
    }

    private async Task RunPingTest()
    {
        ping.Text = "TESTING…";
        status.Text = "Sending 15 connection checks to 1.1.1.1…";
        var samples = new List<long>();
        int lost = 0;
        try
        {
            using var client = new Ping();
            for (int i = 0; i < 15; i++)
            {
                try
                {
                    var reply = await client.SendPingAsync("1.1.1.1", 1800);
                    if (reply.Status == IPStatus.Success) samples.Add(reply.RoundtripTime);
                    else lost++;
                }
                catch { lost++; }
                await Task.Delay(120);
            }

            if (samples.Count == 0)
            {
                ping.Text = "NO REPLY";
                status.Text = "No ping replies. Check your connection; firewall or network rules may block ICMP.";
                return;
            }

            var average = samples.Average();
            var jitter = samples.Count > 1
                ? samples.Zip(samples.Skip(1), (a, b) => Math.Abs(a - b)).Average()
                : 0;
            ping.Text = $"{average:0} ms";
            status.Text = $"15 checks: avg {average:0} ms | variation {jitter:0} ms | packet loss {lost * 100 / 15}% (to 1.1.1.1, not Roblox).";
        }
        catch
        {
            ping.Text = "FAILED";
            status.Text = "Connection test failed. Try again or check your network.";
        }
    }


    private void OpenNvidiaPanel()
    {
        string[] candidates =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation", "Control Panel Client", "nvcplui.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "NVIDIA Corporation", "Control Panel Client", "nvcplui.exe")
        };
        var exe = candidates.FirstOrDefault(File.Exists);
        try
        {
            if (exe is not null)
                Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
            else
                status.Text = "NVIDIA Control Panel was not found. Install it from Microsoft Store or update NVIDIA drivers.";
        }
        catch { status.Text = "Could not open NVIDIA Control Panel."; }
    }

    private static void OpenSettings(string uri)
    {
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }
}

internal sealed class CpuReader
{
    private ulong lastIdle, lastKernel, lastUser;
    private bool initialized;

    public double? Read()
    {
        if (!Native.GetSystemTimes(out var idle, out var kernel, out var user)) return null;
        ulong i = idle.Value, k = kernel.Value, u = user.Value;
        if (!initialized)
        {
            lastIdle = i; lastKernel = k; lastUser = u; initialized = true;
            return 0;
        }
        ulong idleDelta = i - lastIdle;
        ulong totalDelta = (k - lastKernel) + (u - lastUser);
        lastIdle = i; lastKernel = k; lastUser = u;
        return totalDelta == 0 ? 0 : Math.Clamp(100d * (totalDelta - idleDelta) / totalDelta, 0, 100);
    }
}

internal static class MemoryReader
{
    [StructLayout(LayoutKind.Sequential)]
    private class MemoryStatus
    {
        public uint Length = (uint)Marshal.SizeOf<MemoryStatus>();
        public uint Load;
        public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus status);

    public static double? Read()
    {
        var s = new MemoryStatus();
        return GlobalMemoryStatusEx(s) ? s.Load : null;
    }
}

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FileTime { public uint Low, High; public ulong Value => ((ulong)High << 32) | Low; }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetSystemTimes(out FileTime idle, out FileTime kernel, out FileTime user);
}


internal sealed class NvidiaReader
{
    internal readonly record struct GpuInfo(string Name, int Utilization, int MemoryUsed, int MemoryTotal, int Temperature, string Driver);

    public GpuInfo? Read()
    {
        try
        {
            var start = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=name,utilization.gpu,memory.used,memory.total,temperature.gpu,driver_version --format=csv,noheader,nounits",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var process = Process.Start(start);
            if (process is null) return null;
            if (!process.WaitForExit(1200))
            {
                try { process.Kill(true); } catch { }
                return null;
            }
            if (process.ExitCode != 0) return null;
            var line = process.StandardOutput.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(line)) return null;
            var parts = line.Split(',').Select(x => x.Trim()).ToArray();
            if (parts.Length < 6) return null;
            return new GpuInfo(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), parts[5]);
        }
        catch { return null; }
    }
}
