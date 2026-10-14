// HAAVK loading animation in front of the ChatGPT desktop app.
// Plays haavk_02.mp4 full screen (with sound) while ChatGPT starts underneath; Esc skips.
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

static class Program
{
    [STAThread]
    static void Main()
    {
        var app = new Application();
        app.Run(new LaunchWindow());
    }
}

class LaunchWindow : Window
{
    const string ChatGptAppId = "OpenAI.Codex_2p2nqsd0c76g0!App";

    readonly MediaElement video;
    readonly Grid root;
    readonly TextBlock hint;
    readonly DispatcherTimer keepFront;
    bool chatGptStarted;
    bool finishing;
    DateTime shownAt;

    public LaunchWindow()
    {
        Title = "ChatGPT · HAAVK";
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Maximized;
        Topmost = true;
        Background = Brushes.Black;
        Cursor = Cursors.None;

        var dir = AppDomain.CurrentDomain.BaseDirectory;
        video = new MediaElement
        {
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Close,
            Stretch = Stretch.Uniform,
            Volume = 1.0,
            Source = new Uri(Path.Combine(dir, "haavk_02.mp4"))
        };
        video.MediaEnded += (s, e) => Finish();
        video.MediaFailed += (s, e) => { StartChatGpt(); Close(); };

        hint = new TextBlock
        {
            Text = "Esc  跳过",
            Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
            FontSize = 14,
            Margin = new Thickness(0, 0, 32, 26),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        root = new Grid();
        root.Children.Add(video);
        root.Children.Add(hint);
        Content = root;

        // ChatGPT's own window may take the foreground while it starts; stay in front so Esc keeps working.
        keepFront = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        keepFront.Tick += (s, e) =>
        {
            if (!IsActive) Activate();
            if ((DateTime.Now - shownAt).TotalSeconds > 8) keepFront.Stop();
        };

        Loaded += (s, e) =>
        {
            shownAt = DateTime.Now;
            video.Play();
            keepFront.Start();
            After(0.6, StartChatGpt);
            After(3.0, () => hint.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.8))));
        };
        KeyDown += (s, e) => { if (e.Key == Key.Escape) Finish(); };
        MouseDown += (s, e) => Activate();
    }

    void After(double seconds, Action action)
    {
        var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
        t.Tick += (s, e) => { t.Stop(); action(); };
        t.Start();
    }

    void StartChatGpt()
    {
        if (chatGptStarted) return;
        chatGptStarted = true;
        try
        {
            // the supported way to start a Store / packaged app by its AppUserModelID
            var mgr = (IApplicationActivationManager)new ApplicationActivationManager();
            uint pid;
            if (mgr.ActivateApplication(ChatGptAppId, null, 0, out pid) < 0) throw new Exception("activation failed");
            return;
        }
        catch (Exception) { }
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", "shell:AppsFolder\\" + ChatGptAppId) { UseShellExecute = true });
        }
        catch (Exception) { }
    }

    void Finish()
    {
        if (finishing) return;
        finishing = true;
        StartChatGpt();
        keepFront.Stop();
        var fade = new DoubleAnimation(0, TimeSpan.FromSeconds(0.7));
        fade.Completed += (s, e) => { video.Stop(); HandOver(); };
        var steps = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        steps.Tick += (s, e) =>
        {
            video.Volume = Math.Max(0, video.Volume - 0.08);
            if (video.Volume <= 0) steps.Stop();
        };
        steps.Start();
        root.BeginAnimation(OpacityProperty, fade);
    }

    // ---- bring the ChatGPT window to the front, launching it again if it is hidden in the tray ----
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr l);
    delegate bool EnumProc(IntPtr h, IntPtr l);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
    [DllImport("user32.dll")] static extern int GetWindowTextLength(IntPtr h);
    [DllImport("user32.dll")] static extern IntPtr GetWindow(IntPtr h, uint cmd);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr h, int cmd);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr h);
    [DllImport("user32.dll")] static extern bool AllowSetForegroundWindow(int pid);

    static IntPtr FindChatGptWindow()
    {
        IntPtr found = IntPtr.Zero;
        EnumWindows((h, l) =>
        {
            if (!IsWindowVisible(h) || GetWindowTextLength(h) == 0 || GetWindow(h, 4) != IntPtr.Zero) return true; // 4 = GW_OWNER
            uint pid;
            GetWindowThreadProcessId(h, out pid);
            try
            {
                if (Process.GetProcessById((int)pid).ProcessName.Equals("ChatGPT", StringComparison.OrdinalIgnoreCase))
                {
                    found = h;
                    return false;
                }
            }
            catch (Exception) { }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    static void Bring(IntPtr h)
    {
        ShowWindow(h, IsIconic(h) ? 9 : 5); // SW_RESTORE : SW_SHOW
        BringWindowToTop(h);
        SetForegroundWindow(h);
    }

    void HandOver()
    {
        Topmost = false;
        AllowSetForegroundWindow(-1);
        var h = FindChatGptWindow();
        if (h != IntPtr.Zero)
        {
            Bring(h);
            Close();
            return;
        }
        // hidden in the tray or still starting: ask the shell again and wait for its window
        chatGptStarted = false;
        StartChatGpt();
        var started = DateTime.Now;
        var poll = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        poll.Tick += (s, e) =>
        {
            var w = FindChatGptWindow();
            if (w != IntPtr.Zero || (DateTime.Now - started).TotalSeconds > 15)
            {
                poll.Stop();
                if (w != IntPtr.Zero) Bring(w);
                Close();
            }
        };
        poll.Start();
    }
}

[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IApplicationActivationManager
{
    [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
                                          [MarshalAs(UnmanagedType.LPWStr)] string arguments, int options, out uint processId);
    [PreserveSig] int ActivateForFile(IntPtr a, IntPtr b, [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
    [PreserveSig] int ActivateForProtocol(IntPtr a, IntPtr b, out uint processId);
}

[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
class ApplicationActivationManager { }
