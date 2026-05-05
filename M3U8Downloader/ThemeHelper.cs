using System.Windows;
using System.Windows.Media;

namespace M3U8Downloader
{
    public static class ThemeHelper
    {
        public static void ApplyTheme(ResourceDictionary res)
        {
            Set(res, "BgMain",        "#FF141414");
            Set(res, "BgCard",        "#FF1E1E1E");
            Set(res, "BgInput",       "#FF252525");
            Set(res, "AccentColor",   "#FF3B8BEE");
            Set(res, "TextPrimary",   "#FFEEEEEE");
            Set(res, "TextSecondary", "#FF888888");
            Set(res, "BorderColor",   "#FF333333");
            Set(res, "SuccessColor",  "#FF4CAF50");
            Set(res, "ErrorColor",    "#FFCF6679");
            Set(res, "ProgressBg",    "#FF333333");
        }

        private static void Set(ResourceDictionary res, string key, string hex)
        {
            var color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            res[key]        = color;
            res["Br" + key] = new SolidColorBrush(color);
        }
    }
}
