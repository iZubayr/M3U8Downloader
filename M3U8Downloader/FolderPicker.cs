using System.Runtime.InteropServices;

namespace M3U8Downloader
{
    public static class FolderPicker
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, System.Text.StringBuilder pszPath);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr pv);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        private const uint BIF_RETURNONLYFSDIRS   = 0x0001;
        private const uint BIF_NEWDIALOGSTYLE      = 0x0040;
        private const uint BIF_EDITBOX             = 0x0010;

        public static string? Pick(string title, string initialPath)
        {
            var bi = new BROWSEINFO
            {
                lpszTitle      = title,
                ulFlags        = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE | BIF_EDITBOX,
                pszDisplayName = new string(' ', 260)
            };

            var pidl = SHBrowseForFolder(ref bi);
            if (pidl == IntPtr.Zero) return null;

            var path = new System.Text.StringBuilder(260);
            SHGetPathFromIDList(pidl, path);
            CoTaskMemFree(pidl);

            return path.Length > 0 ? path.ToString() : null;
        }
    }
}
