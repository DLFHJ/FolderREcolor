if (args.Length == 0)
{
    Console.WriteLine("Usage: FolderColor <color> <folder> | --clear <folder> | --install | --uninstall");
    return;
}

switch (args[0])
{
    case "--install":
        Registrar.Install();
        Console.WriteLine("Installed. Restart Explorer to see the menu.");
        break;
    case "--uninstall":
        Registrar.Uninstall();
        Console.WriteLine("Removed.");
        break;
    case "--clear" when args.Length > 1:
        FolderIcon.Clear(args[1]);
        break;
    default:
        if (args.Length > 1) FolderIcon.Apply(args[1], args[0]);
        else Console.WriteLine("Missing folder path.");
        break;
}