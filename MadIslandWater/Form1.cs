namespace MadIslandWater;

public partial class Form1 : Form
{
    private readonly MadIslandPatcher patcher;

    public Form1()
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;
        patcher = new MadIslandPatcher(AppendLog);
        gamePathTextBox.Text = @"D:\Program Files (x86)\Steam\steamapps\common\Mad Island";
        dlcPathTextBox.Text = FindDefaultDlcPath();
        pathIdTextBox.Text = MadIslandPatcher.CurrentMosaicShaderPathId.ToString();
    }

    private void browseGameButton_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择 Mad Island 游戏根目录",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(gamePathTextBox.Text) ? gamePathTextBox.Text : string.Empty
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            gamePathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void browseDlcButton_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择官网 DLC 文件",
            Filter = "DLC 文件|dlc_00;dlc_00.zip|所有文件|*.*",
            FileName = File.Exists(dlcPathTextBox.Text) ? dlcPathTextBox.Text : "dlc_00.zip",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            dlcPathTextBox.Text = dialog.FileName;
        }
    }

    private async void runButton_Click(object sender, EventArgs e)
    {
        SetActionsEnabled(false);
        SetStatus("处理中...");
        logTextBox.Clear();

        try
        {
            var options = new PatchOptions(
                gamePathTextBox.Text.Trim(),
                dlcPathTextBox.Text.Trim(),
                installDlcCheckBox.Checked,
                patchMosaicCheckBox.Checked,
                backupCheckBox.Checked,
                ParsePathId());

            var result = await Task.Run(() => patcher.Apply(options));
            AppendLog("");
            AppendLog("处理完成。");

            foreach (var backupFile in result.BackupFiles)
            {
                AppendLog($"备份文件：{backupFile}");
            }

            SetStatus("完成");
            MessageBox.Show(this, "处理完成。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("");
            AppendLog("处理失败：");
            AppendLog(ex.Message);
            SetStatus("失败");
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private async void restoreButton_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            this,
            "将恢复原 data.unity3d，并删除解包出来的资源文件。DLC 文件不会删除。是否继续？",
            Text,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        SetActionsEnabled(false);
        SetStatus("还原中...");
        logTextBox.Clear();

        try
        {
            await Task.Run(() => patcher.Restore(gamePathTextBox.Text.Trim()));
            AppendLog("");
            AppendLog("还原完成。");
            SetStatus("完成");
            MessageBox.Show(this, "还原完成。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("");
            AppendLog("还原失败：");
            AppendLog(ex.Message);
            SetStatus("失败");
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(AppendLog), message);
            return;
        }

        logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void SetStatus(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(SetStatus), message);
            return;
        }

        statusLabel.Text = message;
    }

    private void SetActionsEnabled(bool enabled)
    {
        runButton.Enabled = enabled;
        restoreButton.Enabled = enabled;
    }

    private long? ParsePathId()
    {
        var text = pathIdTextBox.Text.Trim();
        if (text.Length == 0)
        {
            return null;
        }

        if (long.TryParse(text, out var pathId) && pathId > 0)
        {
            return pathId;
        }

        throw new InvalidOperationException("PathID 必须是正整数，或留空使用自动扫描。");
    }

    private static string FindDefaultDlcPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "dlc", "dlc_00.zip"),
            Path.Combine(AppContext.BaseDirectory, "dlc_00.zip"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "dlc", "dlc_00.zip")),
            Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "dlc", "dlc_00.zip"))
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }
}
