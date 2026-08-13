using Microsoft.Win32;

static class Registrar
{
    const string Root = @"Software\Classes\Directory\shell\ColorFolder";

    static readonly (string SortKey, string Color, string Label)[] Colors =
    {
        ("01", "blue",  "Blue"),
        ("02", "green", "Green"),
        ("03", "red",   "Red"),
    };

    public static void Install()
    {
        Uninstall();   // clear the broken layout from last time

        string exe = Environment.ProcessPath!;

        using (var root = Registry.CurrentUser.CreateSubKey(Root)!)
        {
            root.SetValue("MUIVerb", "Color folder");
            root.SetValue("SubCommands", "");
            root.SetValue("Icon", exe);
        }

        foreach (var (sort, color, label) in Colors)
        {
            using var verb = Registry.CurrentUser.CreateSubKey($@"{Root}\shell\{sort}{color}")!;
            verb.SetValue("", label);
            verb.SetValue("Icon", Path.Combine(FolderIcon.IconDir, color + ".ico"));
            using var cmd = verb.CreateSubKey("command")!;
            cmd.SetValue("", $"\"{exe}\" {color} \"%1\"");
        }

        using var clear = Registry.CurrentUser.CreateSubKey($@"{Root}\shell\99clear")!;
        clear.SetValue("", "Default color");
        using var clearCmd = clear.CreateSubKey("command")!;
        clearCmd.SetValue("", $"\"{exe}\" --clear \"%1\"");
    }

    public static void Uninstall()
        => Registry.CurrentUser.DeleteSubKeyTree(Root, throwOnMissingSubKey: false);
}