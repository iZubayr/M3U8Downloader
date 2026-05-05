using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace M3U8Downloader
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<DownloadItem> _queue = [];
        private AppConfig _config = new();
        private DispatcherTimer? _clipTimer;
        private string _lastClip = "";
        private bool _isUzbek   = true;
        private bool _autoPaste = true;
        private bool _autoTitle = true;

        private string UZ(string uz, string en) => _isUzbek ? uz : en;

        public MainWindow()
        {
            InitializeComponent();
            _config    = ConfigManager.Load();
            _autoPaste = _config.AutoPaste;
            _autoTitle = _config.AutoTitle;

            QueueList.ItemsSource = _queue;
            FolderBox.Text = _config.OutputFolder;

            SetCheck(AutoPasteBorder, AutoPasteMark, _autoPaste);
            SetCheck(AutoTitleBorder, AutoTitleMark, _autoTitle);

            Loaded  += OnLoaded;
            Closing += (_, _) => SaveConfig();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyLang();
            CheckToolsBadge();
            if (_autoPaste) StartClipWatch();
        }

        // ── Custom title bar ──────────────────────────────────────────────────
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        // ── Tools badge ───────────────────────────────────────────────────────
        private void CheckToolsBadge()
        {
            bool ok = ToolsManager.AllToolsExist();
            if (!ok)
            {
                var (y, f) = ToolsManager.CheckPath();
                ok = y && f;
            }
            if (!ok)
                SetStatus(UZ("⚠ yt-dlp yoki ffmpeg topilmadi", "⚠ yt-dlp or ffmpeg not found"));
        }

        // ── Language ──────────────────────────────────────────────────────────
        private void LangBtn_Click(object sender, RoutedEventArgs e)
        {
            _isUzbek        = !_isUzbek;
            LangBtn.Content = _isUzbek ? "EN" : "UZ";
            ApplyLang();
            foreach (var item in _queue) item.IsUzbek = _isUzbek;
            UpdateCount();
        }

        private void ApplyLang()
        {
            TitleD.Text       = UZ("Yuklovchi",        "Downloader");
            LblLink.Text      = "M3U8 link";
            LblTitle.Text     = UZ("Video nomi",       "Video title");
            LblFolder.Text    = UZ("Saqlash joyi",     "Save folder");
            LblAutoPaste.Text = UZ("Clipboard kuzatish","Clipboard watch");
            LblAutoTitle.Text = UZ("Sarlavha avtomatik","Auto title");
            LblQueue.Text     = UZ("Yuklanma navbati", "Download queue");
            PasteBtn.Content  = UZ("Joylash",          "Paste");
            BrowserBtn.Content= UZ("🌐",       "🌐");
            ClearAllBtn.Content = UZ("Tozalash",       "Clear done");
            AddBtn.Content    = UZ("Navbatga qo'shish  +", "Add to queue  +");
            OpenFolderBtn.ToolTip = UZ("Papkani ochish","Open folder");
            FolderBtn.ToolTip     = UZ("Papka tanlash", "Select folder");
            StatusBar.Text    = UZ("Tayyor",           "Ready");
            UpdateCount();
        }

        // ── Custom checkboxes ─────────────────────────────────────────────────
        private static void SetCheck(System.Windows.Controls.Border border,
            System.Windows.Controls.TextBlock mark, bool val)
        {
            mark.Text          = val ? "✓" : "";
            border.BorderBrush = val
                ? new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3B, 0x8B, 0xEE))
                : new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x33, 0x33, 0x33));
        }

        private void AutoPasteToggle(object sender, MouseButtonEventArgs e)
        {
            _autoPaste = !_autoPaste;
            SetCheck(AutoPasteBorder, AutoPasteMark, _autoPaste);
            if (_autoPaste) StartClipWatch(); else StopClipWatch();
            SaveConfig();
        }

        private void AutoTitleToggle(object sender, MouseButtonEventArgs e)
        {
            _autoTitle = !_autoTitle;
            SetCheck(AutoTitleBorder, AutoTitleMark, _autoTitle);
            SaveConfig();
        }

        // ── Clipboard watch ───────────────────────────────────────────────────
        private void StartClipWatch()
        {
            _clipTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _clipTimer.Tick += (_, _) =>
            {
                try
                {
                    var txt = System.Windows.Clipboard.GetText();
                    if (txt == _lastClip) return;
                    _lastClip = txt;
                    if (!BrowserHelper.IsM3u8Link(txt)) return;
                    LinkBox.Text = txt;
                    LinkBox.SelectAll();
                    SetStatus(UZ("M3U8 link topildi — clipboard ✓", "M3U8 link detected ✓"));
                    TryAutoTitle();
                }
                catch { }
            };
            _clipTimer.Start();
        }

        private void StopClipWatch() => _clipTimer?.Stop();

        // ── Buttons ───────────────────────────────────────────────────────────
        private void PasteBtn_Click(object sender, RoutedEventArgs e)
        {
            var txt = System.Windows.Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(txt)) return;
            LinkBox.Text = txt;
            LinkBox.SelectAll();
            TryAutoTitle();
        }

        private async void BrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            SetStatus(UZ("Brauzer tekshirilmoqda...", "Checking browser..."));
            var link = await BrowserHelper.FindM3u8LinkAsync();
            if (link != null)
            {
                LinkBox.Text = link;
                LinkBox.SelectAll();
                SetStatus(UZ("M3U8 link topildi ✓", "M3U8 link found ✓"));
                TryAutoTitle();
                return;
            }
            var hist = BrowserHelper.ScanBrowserHistory();
            if (hist.Count > 0)
            {
                LinkBox.Text = hist[^1];
                LinkBox.SelectAll();
                SetStatus(UZ($"Tarixdan topildi ({hist.Count}) ✓",
                             $"Found in history ({hist.Count}) ✓"));
                TryAutoTitle();
            }
            else
                SetStatus(UZ("M3U8 link topilmadi", "No M3U8 link found"));
        }

        private void FolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var path = FolderPicker.Pick(
                UZ("Yuklanma papkasini tanlang", "Select download folder"),
                FolderBox.Text);
            if (path != null)
            {
                FolderBox.Text       = path;
                _config.OutputFolder = path;
                SaveConfig();
            }
        }

        private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var folder = FolderBox.Text;
            if (Directory.Exists(folder))
                Process.Start("explorer.exe", folder);
            else
                SetStatus(UZ("Papka topilmadi", "Folder not found"));
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var link   = LinkBox.Text.Trim();
            var title  = TitleBox.Text.Trim();
            var folder = FolderBox.Text.Trim();

            if (!BrowserHelper.IsM3u8Link(link))
            {
                SetStatus(UZ("⚠ To'g'ri m3u8 link kiriting", "⚠ Enter a valid m3u8 link"));
                return;
            }
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                SetStatus(UZ("⚠ Avval papka tanlang", "⚠ Select a folder first"));
                return;
            }
            if (!ToolsManager.AllToolsExist())
            {
                var (y, f) = ToolsManager.CheckPath();
                if (!y || !f)
                {
                    SetStatus(UZ("⚠ yt-dlp yoki ffmpeg topilmadi",
                                 "⚠ yt-dlp or ffmpeg not found"));
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(title))
                title = $"video_{DateTime.Now:yyyyMMdd_HHmmss}";

            var item = new DownloadItem { Title = title, Link = link, IsUzbek = _isUzbek };
            item.CancelCommand = new RelayCommand(() =>
            {
                item.Cts.Cancel();
                if (item.Status == DownloadStatus.Queued)
                    item.Status = DownloadStatus.Cancelled;
            });

            _queue.Add(item);
            LinkBox.Text  = "";
            TitleBox.Text = "";
            UpdateCount();

            _ = Task.Run(async () =>
            {
                await DownloadEngine.DownloadAsync(item, folder);
                Dispatcher.Invoke(UpdateCount);
            });
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            var done = _queue.Where(i => i.Status is
                DownloadStatus.Done or DownloadStatus.Error or DownloadStatus.Cancelled)
                .ToList();
            foreach (var i in done) _queue.Remove(i);
            UpdateCount();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void TryAutoTitle()
        {
            if (!_autoTitle) return;
            var t = BrowserHelper.GetActiveBrowserTitle();
            if (!string.IsNullOrWhiteSpace(t)) TitleBox.Text = t;
        }

        private void SetStatus(string msg) =>
            Dispatcher.Invoke(() => StatusBar.Text = msg);

        private void UpdateCount()
        {
            var active = _queue.Count(i => i.Status == DownloadStatus.Downloading);
            var total  = _queue.Count;
            QueueCountText.Text = UZ(
                $"{active} aktiv / {total} jami",
                $"{active} active / {total} total");
        }

        private void SaveConfig()
        {
            _config.OutputFolder = FolderBox.Text;
            _config.AutoPaste    = _autoPaste;
            _config.AutoTitle    = _autoTitle;
            ConfigManager.Save(_config);
        }
private void GitHub_Click(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
{
    Process.Start(new ProcessStartInfo
    {
        FileName = e.Uri.AbsoluteUri,
        UseShellExecute = true
    });
}
    }

}
