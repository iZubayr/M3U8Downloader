using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace M3U8Downloader
{
    public static partial class DownloadEngine
    {
        public static async Task DownloadAsync(DownloadItem item, string outputFolder)
        {
            item.Status   = DownloadStatus.Downloading;
            item.Progress = 0;

            var safeTitle = SanitizeFileName(item.Title);
            if (string.IsNullOrWhiteSpace(safeTitle))
                safeTitle = $"video_{DateTime.Now:yyyyMMdd_HHmmss}";

            var outTemplate = Path.Combine(outputFolder, safeTitle + ".%(ext)s");

            var args = $"-f \"bv*+ba/best\" " +
                       $"--merge-output-format mp4 " +
                       $"--newline " +
                       $"--ffmpeg-location \"{ToolsManager.GetFfmpegExe()}\" " +
                       $"-o \"{outTemplate}\" " +
                       $"\"{item.Link}\"";

            var psi = new ProcessStartInfo
            {
                FileName               = ToolsManager.GetYtDlpExe(),
                Arguments              = args,
                UseShellExecute        = false,
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
            };

            using var proc = new Process { StartInfo = psi };

            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                ParseLine(e.Data, item);
            };
            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                if (e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                    item.SpeedText = e.Data;
            };

            try
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                using (item.Cts.Token.Register(() =>
                {
                    try { proc.Kill(entireProcessTree: true); } catch { }
                }))
                {
                    await proc.WaitForExitAsync(item.Cts.Token);
                }

                if (item.Cts.Token.IsCancellationRequested)
                {
                    item.Status = DownloadStatus.Cancelled;
                }
                else if (proc.ExitCode == 0)
                {
                    item.Progress  = 100;
                    item.SpeedText = "";
                    item.Status    = DownloadStatus.Done;
                }
                else
                {
                    item.Status    = DownloadStatus.Error;
                    item.SpeedText = $"ExitCode: {proc.ExitCode}";
                }
            }
            catch (OperationCanceledException)
            {
                item.Status = DownloadStatus.Cancelled;
            }
            catch (Exception ex)
            {
                item.Status    = DownloadStatus.Error;
                item.SpeedText = ex.Message;
            }
        }

        private static void ParseLine(string line, DownloadItem item)
        {
            // [download]  45.3% of 123.45MiB at 2.50MiB/s ETA 00:30
            var m = ProgressRegex().Match(line);
            if (m.Success)
            {
                if (double.TryParse(m.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var pct))
                {
                    item.Progress  = Math.Clamp(pct, 0, 100);
                    var speed = m.Groups[2].Success ? m.Groups[2].Value.Trim() : "";
                    var eta   = m.Groups[3].Success ? m.Groups[3].Value.Trim() : "";
                    item.SpeedText = string.IsNullOrEmpty(speed) ? "" : $"{speed}  ETA {eta}";
                }
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Trim('_', ' ');
        }

        [GeneratedRegex(
            @"\[download\]\s+([\d.]+)%\s+of\s+[\d.]+\S+\s+at\s+([\d.]+\s*\S+/s)\s+ETA\s+([\d:]+)",
            RegexOptions.IgnoreCase)]
        private static partial Regex ProgressRegex();
    }
}
