using System.Windows;

namespace M3U8Downloader
{
    public partial class SetupWindow : Window
    {
        public SetupWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Check if tools already exist
            var (ytdlpInPath, ffmpegInPath) = ToolsManager.CheckPath();
            var localOk = ToolsManager.AllToolsExist();

            if (localOk || (ytdlpInPath && ffmpegInPath))
            {
                AppendLog("yt-dlp: topildi ✓");
                AppendLog("ffmpeg: topildi ✓");
                AppendLog("Hammasi tayyor.");
                ProgressBar.Value = 100;
                PctText.Text = "100%";
                StatusText.Text = "Barcha vositalar tayyor.";
                InstallBtn.Content = "Boshlash";
                InstallBtn.IsEnabled = true;
                SkipBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (ytdlpInPath) AppendLog("yt-dlp PATH da topildi ✓");
                if (ffmpegInPath) AppendLog("ffmpeg PATH da topildi ✓");
                if (!ytdlpInPath) AppendLog("yt-dlp topilmadi — yuklanadi");
                if (!ffmpegInPath) AppendLog("ffmpeg topilmadi — yuklanadi (~115 MB)");
            }
        }

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ToolsManager.AllToolsExist() || ProgressBar.Value == 100)
            {
                DialogResult = true;
                return;
            }

            InstallBtn.IsEnabled = false;
            SkipBtn.IsEnabled    = false;
            InstallBtn.Content   = "Yuklanmoqda...";

            var progress = new Progress<(string msg, double pct)>(rep =>
            {
                AppendLog(rep.msg);
                ProgressBar.Value = rep.pct;
                PctText.Text = $"{rep.pct:F0}%";
            });

            try
            {
                if (!ToolsManager.YtDlpExists)
                    await ToolsManager.DownloadYtDlpAsync(progress);

                if (!ToolsManager.FfmpegExists)
                    await ToolsManager.DownloadFfmpegAsync(progress);

                AppendLog("O'rnatish tugadi ✓");
                StatusText.Text = "Hammasi tayyor!";
                InstallBtn.Content   = "Boshlash";
                InstallBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                AppendLog($"XATO: {ex.Message}");
                StatusText.Text = "Xato yuz berdi. Qayta urinib ko'ring.";
                InstallBtn.Content   = "Qayta urinish";
                InstallBtn.IsEnabled = true;
                SkipBtn.IsEnabled    = true;
            }
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            // Allow skipping if user has tools in PATH
            DialogResult = true;
        }

        private void AppendLog(string line)
        {
            LogText.Text += line + "\n";
            LogScroll.ScrollToBottom();
        }
    }
}
