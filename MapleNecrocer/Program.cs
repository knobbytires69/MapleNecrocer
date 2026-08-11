global using MonoGame.SpriteEngine;
global using WzComparerR2.Common;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MapleNecrocer;
internal static class Program
{
    public static string MaplePath = "";

    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        Application.SetHighDpiMode(HighDpiMode.PerMonitor);
               
        ApplicationConfiguration.Initialize();

        AppSettings.Load();
        MaplePath = ResolveMaplePath(args);

        Application.Run(new MainForm(MaplePath));
    }

    static string ResolveMaplePath(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--maplePath" && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                var path = args[i + 1];
                if (!string.IsNullOrWhiteSpace(path) && System.IO.Directory.Exists(path))
                    return path;
            }
        }

        var envPath = System.Environment.GetEnvironmentVariable("MAPLESTORY_PATH");
        if (!string.IsNullOrWhiteSpace(envPath))
            return envPath;

        if (!string.IsNullOrWhiteSpace(AppSettings.MaplePath))
            return AppSettings.MaplePath;

        return "";
    }
}

internal static class AppSettings
{
    private const string FileName = "settings.json";
    private static string FilePath => System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, FileName);

    public static bool IsMute { get; set; }
    public static string MaplePath { get; set; } = "";

    public static void Load()
    {
        try
        {
            if (!System.IO.File.Exists(FilePath)) return;
            string json = System.IO.File.ReadAllText(FilePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            if (data == null) return;
            IsMute = data.IsMute;
            MaplePath = data.MaplePath ?? "";
        }
        catch (Exception ex)
        {
            MainForm.WriteError($"AppSettings.Load failed: {ex}");
        }
    }

    public static void Save()
    {
        try
        {
            var data = new SettingsData { IsMute = IsMute, MaplePath = MaplePath };
            string json = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            MainForm.WriteError($"AppSettings.Save failed: {ex}");
        }
    }

    private class SettingsData
    {
        public bool IsMute { get; set; }
        public string MaplePath { get; set; }
    }
}