using System.Runtime.InteropServices;

[DllImport("user32.dll", CharSet = CharSet.Unicode)]
static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

if (args.Length == 0)
{
    MessageBox(IntPtr.Zero,
        "Usage: FolderColor <color> <folder> | --clear <folder> | --install | --uninstall",
        "FolderColor", 0);
    return;
}

switch (args[0])
{
    case "--install":
        Registrar.Install();
        MessageBox(IntPtr.Zero, "Installed. Restart Explorer to see the menu.", "FolderColor", 0x40);
        break;
    case "--uninstall":
        Registrar.Uninstall();
        MessageBox(IntPtr.Zero, "Uninstalled.", "FolderColor", 0x40);
        break;
    case "--clear" when args.Length > 1:
        FolderIcon.Clear(args[1]);
        break;
    default:
        if (args.Length > 1) FolderIcon.Apply(args[1], args[0]);
        else Console.WriteLine("Missing folder path.");
        break;
}

