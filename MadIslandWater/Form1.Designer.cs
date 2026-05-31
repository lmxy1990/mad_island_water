namespace MadIslandWater;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        gamePathTextBox = new TextBox();
        browseGameButton = new Button();
        dlcPathTextBox = new TextBox();
        browseDlcButton = new Button();
        installDlcCheckBox = new CheckBox();
        patchMosaicCheckBox = new CheckBox();
        legacyDecodeCheckBox = new CheckBox();
        backupCheckBox = new CheckBox();
        pathIdTextBox = new TextBox();
        pathIdLabel = new Label();
        pathIdHintLabel = new Label();
        runButton = new Button();
        statusLabel = new Label();
        logTextBox = new TextBox();
        gamePathLabel = new Label();
        dlcPathLabel = new Label();
        SuspendLayout();
        // 
        // gamePathTextBox
        // 
        gamePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        gamePathTextBox.Location = new Point(24, 48);
        gamePathTextBox.Name = "gamePathTextBox";
        gamePathTextBox.Size = new Size(640, 31);
        gamePathTextBox.TabIndex = 1;
        // 
        // browseGameButton
        // 
        browseGameButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        browseGameButton.Location = new Point(676, 47);
        browseGameButton.Name = "browseGameButton";
        browseGameButton.Size = new Size(112, 34);
        browseGameButton.TabIndex = 2;
        browseGameButton.Text = "选择...";
        browseGameButton.UseVisualStyleBackColor = true;
        browseGameButton.Click += browseGameButton_Click;
        // 
        // dlcPathTextBox
        // 
        dlcPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        dlcPathTextBox.Location = new Point(24, 124);
        dlcPathTextBox.Name = "dlcPathTextBox";
        dlcPathTextBox.Size = new Size(640, 31);
        dlcPathTextBox.TabIndex = 4;
        // 
        // browseDlcButton
        // 
        browseDlcButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        browseDlcButton.Location = new Point(676, 123);
        browseDlcButton.Name = "browseDlcButton";
        browseDlcButton.Size = new Size(112, 34);
        browseDlcButton.TabIndex = 5;
        browseDlcButton.Text = "选择...";
        browseDlcButton.UseVisualStyleBackColor = true;
        browseDlcButton.Click += browseDlcButton_Click;
        // 
        // installDlcCheckBox
        // 
        installDlcCheckBox.AutoSize = true;
        installDlcCheckBox.Checked = true;
        installDlcCheckBox.CheckState = CheckState.Checked;
        installDlcCheckBox.Location = new Point(24, 180);
        installDlcCheckBox.Name = "installDlcCheckBox";
        installDlcCheckBox.Size = new Size(144, 28);
        installDlcCheckBox.TabIndex = 6;
        installDlcCheckBox.Text = "安装官方 DLC";
        installDlcCheckBox.UseVisualStyleBackColor = true;
        // 
        // patchMosaicCheckBox
        // 
        patchMosaicCheckBox.AutoSize = true;
        patchMosaicCheckBox.Checked = true;
        patchMosaicCheckBox.CheckState = CheckState.Checked;
        patchMosaicCheckBox.Location = new Point(198, 180);
        patchMosaicCheckBox.Name = "patchMosaicCheckBox";
        patchMosaicCheckBox.Size = new Size(144, 28);
        patchMosaicCheckBox.TabIndex = 7;
        patchMosaicCheckBox.Text = "去除马赛克";
        patchMosaicCheckBox.UseVisualStyleBackColor = true;
        // 
        // legacyDecodeCheckBox
        // 
        legacyDecodeCheckBox.AutoSize = true;
        legacyDecodeCheckBox.Checked = true;
        legacyDecodeCheckBox.CheckState = CheckState.Checked;
        legacyDecodeCheckBox.Location = new Point(372, 180);
        legacyDecodeCheckBox.Name = "legacyDecodeCheckBox";
        legacyDecodeCheckBox.Size = new Size(144, 28);
        legacyDecodeCheckBox.TabIndex = 8;
        legacyDecodeCheckBox.Text = "应用老解码";
        legacyDecodeCheckBox.UseVisualStyleBackColor = true;
        // 
        // backupCheckBox
        // 
        backupCheckBox.AutoSize = true;
        backupCheckBox.Checked = true;
        backupCheckBox.CheckState = CheckState.Checked;
        backupCheckBox.Location = new Point(546, 180);
        backupCheckBox.Name = "backupCheckBox";
        backupCheckBox.Size = new Size(162, 28);
        backupCheckBox.TabIndex = 9;
        backupCheckBox.Text = "修改前自动备份";
        backupCheckBox.UseVisualStyleBackColor = true;
        // 
        // pathIdTextBox
        // 
        pathIdTextBox.Location = new Point(24, 236);
        pathIdTextBox.Name = "pathIdTextBox";
        pathIdTextBox.Size = new Size(180, 31);
        pathIdTextBox.TabIndex = 11;
        // 
        // pathIdLabel
        // 
        pathIdLabel.AutoSize = true;
        pathIdLabel.Location = new Point(24, 209);
        pathIdLabel.Name = "pathIdLabel";
        pathIdLabel.Size = new Size(174, 24);
        pathIdLabel.TabIndex = 10;
        pathIdLabel.Text = "马赛克 Shader PathID";
        // 
        // pathIdHintLabel
        // 
        pathIdHintLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pathIdHintLabel.AutoEllipsis = true;
        pathIdHintLabel.Location = new Point(224, 239);
        pathIdHintLabel.Name = "pathIdHintLabel";
        pathIdHintLabel.Size = new Size(564, 24);
        pathIdHintLabel.TabIndex = 12;
        pathIdHintLabel.Text = "新版默认 1964；留空会自动扫描。";
        // 
        // runButton
        // 
        runButton.Location = new Point(24, 296);
        runButton.Name = "runButton";
        runButton.Size = new Size(180, 42);
        runButton.TabIndex = 13;
        runButton.Text = "开始处理";
        runButton.UseVisualStyleBackColor = true;
        runButton.Click += runButton_Click;
        // 
        // statusLabel
        // 
        statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        statusLabel.AutoEllipsis = true;
        statusLabel.Location = new Point(224, 305);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(564, 24);
        statusLabel.TabIndex = 14;
        statusLabel.Text = "就绪";
        // 
        // logTextBox
        // 
        logTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        logTextBox.Location = new Point(24, 360);
        logTextBox.Multiline = true;
        logTextBox.Name = "logTextBox";
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.Size = new Size(764, 190);
        logTextBox.TabIndex = 15;
        // 
        // gamePathLabel
        // 
        gamePathLabel.AutoSize = true;
        gamePathLabel.Location = new Point(24, 21);
        gamePathLabel.Name = "gamePathLabel";
        gamePathLabel.Size = new Size(118, 24);
        gamePathLabel.TabIndex = 0;
        gamePathLabel.Text = "游戏根目录";
        // 
        // dlcPathLabel
        // 
        dlcPathLabel.AutoSize = true;
        dlcPathLabel.Location = new Point(24, 97);
        dlcPathLabel.Name = "dlcPathLabel";
        dlcPathLabel.Size = new Size(95, 24);
        dlcPathLabel.TabIndex = 3;
        dlcPathLabel.Text = "DLC 文件";
        // 
        // Form1
        // 
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(812, 574);
        Controls.Add(pathIdHintLabel);
        Controls.Add(pathIdLabel);
        Controls.Add(pathIdTextBox);
        Controls.Add(dlcPathLabel);
        Controls.Add(gamePathLabel);
        Controls.Add(logTextBox);
        Controls.Add(statusLabel);
        Controls.Add(runButton);
        Controls.Add(backupCheckBox);
        Controls.Add(legacyDecodeCheckBox);
        Controls.Add(patchMosaicCheckBox);
        Controls.Add(installDlcCheckBox);
        Controls.Add(browseDlcButton);
        Controls.Add(dlcPathTextBox);
        Controls.Add(browseGameButton);
        Controls.Add(gamePathTextBox);
        MinimumSize = new Size(720, 560);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Mad Island DLC 与去马赛克工具";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TextBox gamePathTextBox;
    private Button browseGameButton;
    private TextBox dlcPathTextBox;
    private Button browseDlcButton;
    private CheckBox installDlcCheckBox;
    private CheckBox patchMosaicCheckBox;
    private CheckBox legacyDecodeCheckBox;
    private CheckBox backupCheckBox;
    private TextBox pathIdTextBox;
    private Label pathIdLabel;
    private Label pathIdHintLabel;
    private Button runButton;
    private Label statusLabel;
    private TextBox logTextBox;
    private Label gamePathLabel;
    private Label dlcPathLabel;
}
