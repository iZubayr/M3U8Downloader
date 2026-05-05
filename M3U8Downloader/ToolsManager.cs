using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace M3U8Downloader
{
    public static class ToolsManager
    {
        private static readonly string ToolsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "M3U8Tools");

        public static string YtDlpPath  => Path.Combine(ToolsDir, "yt-dlp.exe");
        public static string FfmpegPath => Path.Combine(ToolsDir, "ffmpeg.exe");

        public static bool YtDlpExists  => File.Exists(YtDlpPath);
        public static bool FfmpegExists => File.Exists(FfmpegPath);
        public static bool AllToolsExist() => YtDlpExists && FfmpegExists;

        /// <summary>
        /// Downloads yt-dlp.exe from GitHub latest release.
        /// </summary>
        public static async Task DownloadYtDlpAsync(IProgress<(string msg, double pct)> progress)
        {
            Directory.CreateDirectory(ToolsDir);
            progress.Report(("yt-dlp yuklanmoqda...", 0));

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "M3U8Downloader/1.0");
            http.Timeout = TimeSpan.FromMinutes(5);

            var json = await http.GetStringAsync(
                "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest");
            using var doc = JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "latest";
            var url = $"https://github.com/yt-dlp/yt-dlp/releases/download/{tag}/yt-dlp.exe";

            await DownloadFileAsync(http, url, YtDlpPath, progress, 0, 40);
            progress.Report(("yt-dlp tayyor ✓", 40));
        }

        /// <summary>
        /// Downloads ffmpeg.exe from BtbN builds.
        /// </summary>
        public static async Task DownloadFfmpegAsync(IProgress<(string msg, double pct)> progress)
        {
            Directory.CreateDirectory(ToolsDir);
            progress.Report(("ffmpeg yuklanmoqda...", 40));

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "M3U8Downloader/1.0");
            http.Timeout = TimeSpan.FromMinutes(10);

            const string url = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
            var tmpZip = Path.Combine(ToolsDir, "_ffmpeg.zip");

            await DownloadFileAsync(http, url, tmpZip, progress, 40, 90);

            progress.Report(("ffmpeg chiqarilmoqda...", 90));
            using (var zip = System.IO.Compression.ZipFile.OpenRead(tmpZip))
            {
                var entry = zip.Entries.FirstOrDefault(e => e.Name == "ffmpeg.exe");
                if (entry != null)
                {
                    using var s = entry.Open();
                    using var fs = File.Create(FfmpegPath);
                    await s.CopyToAsync(fs);
                }
            }
            File.Delete(tmpZip);
            progress.Report(("ffmpeg tayyor ✓", 100));
        }

        private static async Task DownloadFileAsync(HttpClient http, string url, string dest,
            IProgress<(string msg, double pct)> progress, double pctStart, double pctEnd)
        {
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var total = response.Content.Headers.ContentLength ?? -1L;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fs = File.Create(dest);
            var buffer = new byte[81920];
            long downloaded = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, read));
                downloaded += read;
                if (total > 0)
                {
                    var pct = pctStart + (pctEnd - pctStart) * downloaded / total;
                    var mb = downloaded / 1024.0 / 1024.0;
                    var totalMb = total / 1024.0 / 1024.0;
                    progress.Report(($"{mb:F1} / {totalMb:F1} MB", pct));
                }
            }
        }

        /// <summary>
        /// Checks PATH for yt-dlp and ffmpeg (in case user has them installed globally).
        /// </summary>
        public static (bool ytdlp, bool ffmpeg) CheckPath()
        {
            bool ytdlp  = IsInPath("yt-dlp");
            bool ffmpeg = IsInPath("ffmpeg");
            return (ytdlp, ffmpeg);
        }

        private static bool IsInPath(string name)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = name,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                using var p = Process.Start(psi)!;
                p.WaitForExit(2000);
                return p.ExitCode == 0;
            }
            catch { return false; }
        }

        /// <summary>Returns path to use for yt-dlp (local tools dir preferred).</summary>
        public static string GetYtDlpExe() =>
            YtDlpExists ? YtDlpPath : "yt-dlp";

        /// <summary>Returns path to use for ffmpeg (local tools dir preferred).</summary>
        public static string GetFfmpegExe() =>
            FfmpegExists ? FfmpegPath : "ffmpeg";
    }
}
