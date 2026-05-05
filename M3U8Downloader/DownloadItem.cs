using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace M3U8Downloader
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private string _title  = "";
        private string _link   = "";
        private double _progress;
        private string _statusText = "";
        private string _speedText  = "";
        private DownloadStatus _status = DownloadStatus.Queued;
        private bool _isUzbek = true;
        private CancellationTokenSource? _cts;

        public bool IsUzbek
        {
            get => _isUzbek;
            set { _isUzbek = value; RefreshAll(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; Notify(); }
        }

        public string Link
        {
            get => _link;
            set { _link = value; Notify(); Notify(nameof(LinkShort)); }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; Notify(); Notify(nameof(ProgressText)); Notify(nameof(ProgressVisible)); }
        }

        public string ProgressText => $"{_progress:F1}%";

        public string SpeedText
        {
            get => _speedText;
            set { _speedText = value; Notify(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; Notify(); }
        }

        public DownloadStatus Status
        {
            get => _status;
            set { _status = value; RefreshAll(); }
        }

        private void RefreshAll()
        {
            StatusText = _status switch
            {
                DownloadStatus.Queued      => _isUzbek ? "Navbatda"       : "Queued",
                DownloadStatus.Downloading => _isUzbek ? "Yuklanmoqda..." : "Downloading...",
                DownloadStatus.Done        => "✓ " + (_isUzbek ? "Tayyor" : "Done"),
                DownloadStatus.Error       => "✗ " + (_isUzbek ? "Xato"   : "Error"),
                DownloadStatus.Cancelled   => _isUzbek ? "Bekor qilindi"  : "Cancelled",
                _ => ""
            };
            Notify(nameof(StatusColor));
            Notify(nameof(CancelVisible));
            Notify(nameof(CancelTooltip));
            Notify(nameof(ProgressVisible));
        }

        public Brush StatusColor => _status switch
        {
            DownloadStatus.Done        => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
            DownloadStatus.Error       => new SolidColorBrush(Color.FromRgb(0xCF, 0x66, 0x79)),
            DownloadStatus.Downloading => new SolidColorBrush(Color.FromRgb(0x3B, 0x8B, 0xEE)),
            _                          => new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
        };

        public Visibility ProgressVisible =>
            _status == DownloadStatus.Downloading ? Visibility.Visible : Visibility.Collapsed;

        public Visibility CancelVisible =>
            _status is DownloadStatus.Downloading or DownloadStatus.Queued
            ? Visibility.Visible : Visibility.Collapsed;

        public string CancelTooltip => _isUzbek
            ? "⚠ Bekor qilsangiz yuklanma to'xtaydi va qayta tiklanmaydi"
            : "⚠ This will permanently stop the download";

        public string LinkShort => _link.Length > 58 ? _link[..55] + "..." : _link;

        public CancellationTokenSource Cts
        {
            get { _cts ??= new CancellationTokenSource(); return _cts; }
        }

        public ICommand CancelCommand { get; set; } = new RelayCommand(() => { });
    }

    public enum DownloadStatus { Queued, Downloading, Done, Error, Cancelled }

    public class RelayCommand : ICommand
    {
        private readonly Action _action;
        public RelayCommand(Action a) => _action = a;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
        public bool CanExecute(object? p) => true;
        public void Execute(object? p) => _action();
    }
}
