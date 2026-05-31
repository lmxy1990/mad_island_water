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
        UpdateOperationModeUi();
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
        SetStatus(restoreRadioButton.Checked ? "还原中..." : "处理中...");
        logTextBox.Clear();

        try
        {
            if (restoreRadioButton.Checked)
            {
                if (!await RestoreResourcesAsync())
                {
                    return;
                }
            }
            else
            {
                await ApplyPatchAsync();
            }

            SetStatus("完成");
            MessageBox.Show(this, restoreRadioButton.Checked ? "还原完成。" : "处理完成。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("");
            AppendLog(restoreRadioButton.Checked ? "还原失败：" : "处理失败：");
            AppendLog(ex.Message);
            SetStatus("失败");
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private async Task ApplyPatchAsync()
    {
        if (!installDlcCheckBox.Checked && !patchMosaicCheckBox.Checked)
        {
            throw new InvalidOperationException("请至少选择安装 DLC 或去除马赛克。");
        }

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
    }

    private async Task<bool> RestoreResourcesAsync()
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
            SetStatus("已取消");
            AppendLog("已取消还原。");
            return false;
        }

        await Task.Run(() => patcher.Restore(gamePathTextBox.Text.Trim()));
        AppendLog("");
        AppendLog("还原完成。");
        return true;
    }

    private void operationModeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        UpdateOperationModeUi();
    }

    private void patchOptionCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        UpdateOperationModeUi();
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
        applyPatchRadioButton.Enabled = enabled;
        restoreRadioButton.Enabled = enabled;
        UpdateOperationModeUi(enabled);
    }

    private void UpdateOperationModeUi(bool actionsEnabled = true)
    {
        var patchMode = applyPatchRadioButton.Checked;

        dlcPathTextBox.Enabled = actionsEnabled && patchMode && installDlcCheckBox.Checked;
        browseDlcButton.Enabled = actionsEnabled && patchMode && installDlcCheckBox.Checked;
        installDlcCheckBox.Enabled = actionsEnabled && patchMode;
        patchMosaicCheckBox.Enabled = actionsEnabled && patchMode;
        backupCheckBox.Enabled = actionsEnabled && patchMode && patchMosaicCheckBox.Checked;
        pathIdTextBox.Enabled = actionsEnabled && patchMode && patchMosaicCheckBox.Checked;
        pathIdLabel.Enabled = actionsEnabled && patchMode && patchMosaicCheckBox.Checked;
        pathIdHintLabel.Enabled = actionsEnabled && patchMode && patchMosaicCheckBox.Checked;
        runButton.Text = patchMode ? "开始处理" : "开始还原";
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
