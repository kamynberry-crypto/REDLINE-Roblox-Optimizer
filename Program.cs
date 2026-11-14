using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace RedlineRobloxOptimizer;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private readonly Color bg = Color.FromArgb(13, 13, 16);
    private readonly Color panel = Color.FromArgb(25, 25, 30);
    private readonly Color red = Color.FromArgb(235, 45, 55);
    private readonly Label ramValue = new();
    private readonly Label cpuValue = new();
    private readonly Label robloxValue = new();
    private readonly Label pingValue = new();
    private readonly Label status = new();
    private readonly Button boostButton = new();
    private readonly System.Windows.Forms.Timer refreshTimer = new() { Interval = 2500 };
    private readonly CpuMeter cpuMeter = new();

    public MainForm()
    {
        Text = "REDLINE | Roblox Optimizer";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(780, 600);
        Size = new Size(900, 690);
        BackColor = bg;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var header = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.FromArgb(19, 19, 23), Padding = new Padding(25, 16, 25, 10) };
        var logo = new Label { Text = "REDLINE", ForeColor = red, Font = new Font("Segoe UI", 27, FontStyle.Bold), AutoSize = true, Location = new Point(22, 10) };
        var subtitle = new Label { Text = "ROBLOX PERFORMANCE TOOLKIT", ForeColor = Color.Silver, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Location = new Point(27, 59) };
        header.Controls.Add(logo);
        header.Controls.Add(subtitle);
        Controls.Add(header);

        var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = bg };
        Controls.Add(main);

        var stats = new TableLayoutPanel { Dock = DockStyle.Top, Height = 145, ColumnCount = 3, RowCount = 1, BackColor = bg };
        stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        stats.Controls.Add(MakeStatCard("CPU LOAD", cpuValue, "Checking…"), 0, 0);
        stats.Controls.Add(MakeStatCard("MEMORY IN USE", ramValue, "Checking…"), 1, 0);
        stats.Controls.Add(MakeStatCard("ROBLOX STATUS", robloxValue, "Not detected"), 2, 0);
        main.Controls.Add(stats);

        var center = new Panel { Dock = DockStyle.Top, Height = 225, BackColor = panel, Padding = new Padding(22), Margin = new Padding(0, 18, 0, 0) };
        var boostTitle = new Label { Text = "QUICK BOOST", ForeColor = Color.White, Font = new Font("Segoe UI", 17, FontStyle.Bold), AutoSize = true, Location = new Point(20, 18) };
        var boostDesc = new Label
        {
            Text = "Applies a temporary Above Normal priority to Roblox if it is already running.\nIt does not change game files, exploit Roblox, or promise extra FPS.",
            ForeColor = Color.Silver, Font = new Font("Segoe UI", 9.5F), AutoSize = true, Location = new Point(22, 56)
        };
        boostButton.Text = "APPLY QUICK BOOST";
        boostButton.Location = new Point(22, 112);
        boostButton.Size = new Size(245, 52);
        StyleButton(boostButton, red);
        boostButton.Click += (_, _) => ApplyBoost();
        var gameMode = MakeButton("WINDOWS GAME MODE", 285, 112, 205);
        gameMode.Click += (_, _) => OpenWindowsPage("ms-settings:gaming-gamemode");
        var launch = MakeButton("LAUNCH ROBLOX", 510, 112, 170);
        launch.Click += (_, _) => LaunchRoblox();
        center.Controls.Add(boostTitle);
        center.Controls.Add(boostDesc);
        center.Controls.Add(boostButton);
        center.Controls.Add(gameMode);
        center.Controls.Add(launch);
        main.Controls.Add(center);

        var network = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = panel, Padding = new Padding(22), Margin = new Padding(0, 14, 0, 0) };
        var networkTitle = new Label { Text = "CONNECTION CHECK", ForeColor = Color.White, Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
        pingValue.Text = "Not tested";
        pingValue.ForeColor = red;
        pingValue.Font = new Font("Segoe UI", 19, FontStyle.Bold);
        pingValue.AutoSize = true;
        pingValue.Location = new Point(22, 48);
        var pingButton = MakeButton("TEST CONNECTION", 510, 30, 170);
        pingButton.Click += async (_, _) => await TestConnection(pingButton);
        network.Controls.Add(networkTitle);
        network.Controls.Add(pingValue);
        network.Controls.Add(pingButton);
        main.Controls.Add(network);

        status.Text = "Ready. No permanent Windows or Roblox settings have been changed.";
        status.ForeColor = Color.Gainsboro;
        status.Dock = DockStyle.Top;
        status.Height = 42;
        status.Padding = new Padding(3, 12, 0, 0);
        main.Controls.Add(status);

        var footer = new Label
        {
            Text = "NOTE: Ping depends mainly on your connection, Wi-Fi, route, and Roblox server. This test measures Cloudflare DNS (1.1.1.1), not a Roblox game server.",
            ForeColor = Color.FromArgb(150, 150, 158),
            Font = new Font("Segoe UI", 8.5F),
            Dock = DockStyle.Bottom,
            Height = 38,
            TextAlign = ContentAlignment.MiddleLeft
        };
        main.Controls.Add(footer);

        refreshTimer.Tick += (_, _) => RefreshStats();
        refreshTimer.Start();
        RefreshStats();
    }

    private Control MakeStatCard(string title, Label value, string initial)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = panel, Margin = new Padding(0, 0, 10, 0), Padding = new Padding(14) };
        var t = new Label { Text = title, ForeColor = Color.Silver, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Location = new Point(13, 17) };
        value.Text = initial;
        value.ForeColor = red;
        value.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        value.AutoSize = true;
        value.Location = new Point(13, 52);
        card.Controls.Add(t);
        card.Controls.Add(value);
        return card;
    }

    private Button MakeButton(string text, int x, int y, int width)
    {
        var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 48) };
        StyleButton(b, Color.FromArgb(55, 55, 63));
        return b;
    }

    private void StyleButton(Button button, Color color)
    {
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    private void RefreshStats()
    {
        cpuValue.Text = cpuMeter.ReadPercent() is double c ? $"{c:0}%" : "—";
        var memory = NativeMemory.GetMemoryStatus();
        ramValue.Text = memory is null ? "—" : $"{memory.Value.UsedPercent:0}%";
        var found = Process.GetProcessesByName("RobloxPlayerBeta").Any();
        robloxValue.Text = found ? "RUNNING" : "NOT RUNNING";
        robloxValue.ForeColor = found ? Color.LightGreen : red;
    }

    private void ApplyBoost()
    {
        var processes = Process.GetProcessesByName("RobloxPlayerBeta");
        if (processes.Length == 0)
        {
            status.Text = "Open Roblox first, then apply Quick Boost.";
            return;
        }

        int changed = 0;
        foreach (var process in processes)
        {
            try
            {
                process.PriorityClass = ProcessPriorityClass.AboveNormal;
                changed++;
            }
            catch { }
        }

        status.Text = changed > 0
            ? "Quick Boost applied. Priority resets when Roblox closes."
            : "Windows did not allow the priority change. Roblox is still safe to use.";
        RefreshStats();
    }

    private void LaunchRoblox()
    {
        try
        {
            Process.Start(new ProcessStartInfo("roblox-player:") { UseShellExecute = true });
            status.Text = "Roblox launch request sent.";
        }
        catch
        {
            status.Text = "Could not open Roblox. Make sure the Roblox Player is installed.";
        }
    }

    private static void OpenWindowsPage(string uri)
    {
        try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
        catch { }
    }

    private async Task TestConnection(Button button)
    {
        button.Enabled = false;
        pingValue.Text = "Testing…";
        try
        {
            var result = await Task.Run(async () =>
            {
                using var ping = new Ping();
                return await ping.SendPingAsync("1.1.1.1", 3000);
            });
            pingValue.Text = result.Status == IPStatus.Success ? $"{result.RoundtripTime} ms" : "No reply";
            status.Text = result.Status == IPStatus.Success
                ? "Connection test complete. This is a general network check, not Roblox-server ping."
                : "The test host did not reply. Check your internet connection and try again.";
        }
        catch
        {
            pingValue.Text = "Failed";
            status.Text = "Connection test failed. Check your network and try again.";
        }
        finally { button.Enabled = true; }
    }
}

internal sealed class CpuMeter
{
    private ulong previousIdle, previousKernel, previousUser;
    private bool hasPrevious;

    public double? ReadPercent()
    {
        if (!NativeCpu.GetSystemTimes(out var idle, out var kernel, out var user))
            return null;

        ulong i = idle.ToUInt64(), k = kernel.ToUInt64(), u = user.ToUInt64();
        if (!hasPrevious)
        {
            previousIdle = i; previousKernel = k; previousUser = u; hasPrevious = true;
            return 0;
        }

        ulong idleDelta = i - previousIdle;
        ulong totalDelta = (k - previousKernel) + (u - previousUser);
        previousIdle = i; previousKernel = k; previousUser = u;
        if (totalDelta == 0) return 0;
        return Math.Clamp(100.0 * (totalDelta - idleDelta) / totalDelta, 0, 100);
    }
}

internal static class NativeCpu
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetSystemTimes(out NativeMemory.FILETIME idle, out NativeMemory.FILETIME kernel, out NativeMemory.FILETIME user);
}

internal static class NativeMemory
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FILETIME { public uint Low; public uint High; public ulong ToUInt64() => ((ulong)High << 32) | Low; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        public uint dwMemoryLoad;
        public ulong ullTotalPhys, ullAvailPhys, ullTotalPageFile, ullAvailPageFile, ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    internal readonly record struct MemoryInfo(double UsedPercent);

    internal static MemoryInfo? GetMemoryStatus()
    {
        var data = new MEMORYSTATUSEX();
        return GlobalMemoryStatusEx(data) ? new MemoryInfo(data.dwMemoryLoad) : null;
    }
}
