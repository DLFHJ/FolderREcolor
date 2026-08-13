using System.Runtime.InteropServices;
using System.Text;

static class FolderIcon
{
    const int SHCNE_UPDATEDIR = 0x00001000;
    const int SHCNE_UPDATEITEM = 0x00002000;
    const int SHCNF_PATHW = 0x0005;
    const int SHCNF_FLUSH = 0x1000;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    static extern void SHChangeNotify(int eventId, int flags, string item1, IntPtr item2);

    [DllImport("shell32.dll", EntryPoint = "SHChangeNotify")]
    static extern void SHChangeNotifyPtr(int eventId, int flags, IntPtr item1, IntPtr item2);

    public static string IconDir => Path.Combine(AppContext.BaseDirectory, "icons");

    static void Log(string msg)
    {
        Console.WriteLine(msg);
        File.AppendAllText(Path.Combine(Path.GetTempPath(), "foldercolor.log"),
                           $"{DateTime.Now:HH:mm:ss}  {msg}\r\n");
    }

    static void Notify(string folder)
    {
        const int flags = SHCNF_PATHW | SHCNF_FLUSH;

        SHChangeNotify(SHCNE_UPDATEITEM, flags, folder, IntPtr.Zero);
        SHChangeNotify(SHCNE_UPDATEDIR, flags, folder, IntPtr.Zero);

        string? parent = Path.GetDirectoryName(folder.TrimEnd('\\'));
        if (parent != null)
        {
            SHChangeNotify(SHCNE_UPDATEDIR, flags, parent, IntPtr.Zero);
            SHChangeNotify(SHCNE_UPDATEITEM, flags, parent, IntPtr.Zero);
        }

        // Not broadcasting SHCNE_ASSOCCHANGED: it disturbs every open Explorer window and made
        // no measurable difference to refresh latency in testing. The ~30s delay before Explorer
        // picks up the new icon is unexplained and appears to be environmental — notifications
        // are delivered in ~80ms with SHCNF_FLUSH, Explorer just doesn't act on them promptly.
    }

    public static void Apply(string folder, string color)
    {
        try
        {
            string ico = Path.Combine(IconDir, color + ".ico");

            if (!Directory.Exists(folder)) { Log($"No such folder: {folder}"); return; }
            if (!File.Exists(ico)) { Log($"No such icon: {ico}"); return; }

            string ini = Path.Combine(folder, "desktop.ini");
            if (File.Exists(ini)) File.SetAttributes(ini, FileAttributes.Normal);

            File.WriteAllText(ini, $"[.ShellClassInfo]\r\nIconResource={ico},0\r\n",
                              Encoding.Unicode);

            File.SetAttributes(ini, FileAttributes.Hidden | FileAttributes.System);

            // FILE_ATTRIBUTE_READONLY on a directory doesn't make it read-only — it's the flag
            // that tells Explorer to consult desktop.ini. FILE_ATTRIBUTE_SYSTEM works identically.
            File.SetAttributes(folder, File.GetAttributes(folder) | FileAttributes.ReadOnly);

            Notify(folder);
            Log($"Applied {color} to {folder}");
        }
        catch (Exception ex)
        {
            Log($"FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static void Clear(string folder)
    {
        try
        {
            string ini = Path.Combine(folder, "desktop.ini");
            if (File.Exists(ini))
            {
                File.SetAttributes(ini, FileAttributes.Normal);
                File.Delete(ini);
            }

            // Strip both: earlier versions of this tool marked folders with System instead.
            if (Directory.Exists(folder))
                File.SetAttributes(folder,
                    File.GetAttributes(folder) & ~FileAttributes.ReadOnly & ~FileAttributes.System);

            Notify(folder);
            Log($"Cleared {folder}");
        }
        catch (Exception ex)
        {
            Log($"FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }
}