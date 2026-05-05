using System.Windows;

namespace M3U8Downloader
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeHelper.ApplyTheme(Current.Resources);

            var config = ConfigManager.Load();
            if (!config.SetupDone || !ToolsManager.AllToolsExist())
            {
                var setup = new SetupWindow();
                var result = setup.ShowDialog();
                if (result != true)
                {
                    Shutdown();
                    return;
                }
                config.SetupDone = true;
                ConfigManager.Save(config);
            }

            var main = new MainWindow();
            main.Show();
        }
    }
}
