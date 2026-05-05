using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace M3U8Downloader
{
    public static partial class BrowserHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static readonly string[] BrowserNames =
            { "chrome", "msedge", "firefox", "brave", "opera" };

        public static string? GetActiveBrowserTitle()
        {
            string? found = null;
            EnumWindows((hWnd, _) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                try
                {
                    var proc = Process.GetProcessById((int)pid);
                    if (BrowserNames.Any(b => proc.ProcessName.ToLower().Contains(b)))
                    {
                        var sb = new StringBuilder(512);
                        GetWindowTextW(hWnd, sb, 512);
                        var title = sb.ToString();
                        if (!string.IsNullOrWhiteSpace(title) && title.Length > 3)
                        {
                            title = Regex.Replace(title,
                                @"\s*[-–]\s*(Google Chrome|Microsoft Edge|Mozilla Firefox|Brave|Opera).*$",
                                "").Trim();
                            if (!string.IsNullOrWhiteSpace(title))
                            {
                                found = title;
                                return false;
                            }
                        }
                    }
                }
                catch { }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        public static async Task<string?> FindM3u8LinkAsync()
        {
            var cdp = await TryGetM3u8FromCdpAsync();
            if (!string.IsNullOrEmpty(cdp)) return cdp;

            var clip = System.Windows.Clipboard.GetText();
            if (IsM3u8Link(clip)) return clip;

            return null;
        }

        private static async Task<string?> TryGetM3u8FromCdpAsync()
        {
            int[] ports = { 9222, 9229, 9223 };
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            foreach (var port in ports)
            {
                try
                {
                    var json = await http.GetStringAsync($"http://localhost:{port}/json");
                    using var doc = JsonDocument.Parse(json);
                    foreach (var tab in doc.RootElement.EnumerateArray())
                    {
                        if (tab.TryGetProperty("url", out var u) && IsM3u8Link(u.GetString()))
                            return u.GetString();
                    }
                }
                catch { }
            }
            return null;
        }

        public static List<string> ScanBrowserHistory()
        {
            var results = new List<string>();
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dirs = new[]
            {
                Path.Combine(local, "Google",       "Chrome",       "User Data", "Default"),
                Path.Combine(local, "Microsoft",    "Edge",         "User Data", "Default"),
                Path.Combine(local, "BraveSoftware", "Brave-Browser","User Data", "Default"),
            };
            foreach (var dir in dirs)
            {
                var hist = Path.Combine(dir, "History");
                if (!File.Exists(hist)) continue;
                try
                {
                    var tmp = Path.GetTempFileName();
                    File.Copy(hist, tmp, true);
                    var content = File.ReadAllText(tmp, Encoding.Latin1);
                    foreach (Match m in M3u8Regex().Matches(content))
                        if (!results.Contains(m.Value)) results.Add(m.Value);
                    File.Delete(tmp);
                }
                catch { }
            }
            return results;
        }

        public static bool IsM3u8Link(string? text) =>
            !string.IsNullOrWhiteSpace(text) &&
            text.Contains("m3u8", StringComparison.OrdinalIgnoreCase) &&
            (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             text.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

        [GeneratedRegex(@"https?://[^\x00-\x1F\s""'<>]{5,}\.m3u8[^\x00-\x1F\s""'<>]*")]
        private static partial Regex M3u8Regex();
    }
}
