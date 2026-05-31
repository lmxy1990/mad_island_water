namespace MadIslandWater;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        if (TryRunCommandLine(Environment.GetCommandLineArgs()[1..]))
        {
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }

    private static bool TryRunCommandLine(string[] args)
    {
        if (args.Length == 0)
        {
            return false;
        }

        if (!args.Contains("--cli", StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var gameDirectory = GetOption(args, "--game") ?? throw new InvalidOperationException("缺少 --game 参数。");
            var dlcFile = GetOption(args, "--dlc") ?? string.Empty;
            var pathId = args.Contains("--scan", StringComparer.OrdinalIgnoreCase)
                ? null
                : ParsePathId(GetOption(args, "--pathid"));
            var patcher = new MadIslandPatcher(Console.WriteLine);
            var options = new PatchOptions(
                gameDirectory,
                dlcFile,
                InstallDlc: !args.Contains("--no-dlc", StringComparer.OrdinalIgnoreCase),
                ApplyMosaicPatch: !args.Contains("--no-mosaic", StringComparer.OrdinalIgnoreCase),
                BackupFiles: !args.Contains("--no-backup", StringComparer.OrdinalIgnoreCase),
                MosaicShaderPathId: pathId);

            var result = patcher.Apply(options);
            Console.WriteLine("处理完成。");

            foreach (var backupFile in result.BackupFiles)
            {
                Console.WriteLine($"备份文件：{backupFile}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.ExitCode = 1;
            return true;
        }
    }

    private static string? GetOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static long? ParsePathId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return MadIslandPatcher.CurrentMosaicShaderPathId;
        }

        if (long.TryParse(value, out var pathId) && pathId > 0)
        {
            return pathId;
        }

        throw new InvalidOperationException("--pathid 必须是正整数。");
    }
}
