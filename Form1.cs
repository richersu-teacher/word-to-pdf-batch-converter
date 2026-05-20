namespace WordToPdfTool;

public partial class Form1 : Form
{
    private readonly FileProcessor _fileProcessor;

    public Form1()
    {
        InitializeComponent();
        _fileProcessor = new FileProcessor();
    }

    /// <summary>
    /// 選擇來源資料夾。
    /// </summary>
    private void btnBrowseSource_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "請選擇來源資料夾"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtSource.Text = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// 選擇目的資料夾。
    /// </summary>
    private void btnBrowseTarget_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "請選擇目的資料夾"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtTarget.Text = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// 開始按鈕事件：做防呆檢查，並以非同步方式執行處理，避免 UI 卡死。
    /// </summary>
    private async void btnStart_Click(object sender, EventArgs e)
    {
        var sourcePath = txtSource.Text.Trim();
        var targetPath = txtTarget.Text.Trim();

        // 防呆檢查：必須選擇來源與目的資料夾
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath))
        {
            MessageBox.Show(this, "請先選擇來源與目的資料夾。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 前置防呆警告：來源/目的相同，或目的資料夾不為空時，需二次確認
        var isSamePath = string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase);
        var targetHasAnyFile = Directory.Exists(targetPath) &&
            Directory.EnumerateFiles(targetPath, "*", SearchOption.AllDirectories).Any();

        if (isSamePath || targetHasAnyFile)
        {
            var confirm = MessageBox.Show(
                this,
                "目的資料夾與來源資料夾相同，或目的資料夾內已有檔案。繼續執行可能會覆蓋現有檔案，確定要繼續嗎？",
                "警告",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.No)
            {
                return;
            }
        }

        try
        {
            ToggleControls(false);
            lblStatus.Text = "狀態：正在掃描檔案...";
            progressBar.Value = 0;

            var progress = new Progress<ProcessingProgressInfo>(info =>
            {
                lblStatus.Text = $"狀態：{info.CurrentFileName}";

                if (info.TotalCount > 0)
                {
                    progressBar.Maximum = info.TotalCount;
                    progressBar.Value = Math.Min(info.CurrentIndex, info.TotalCount);
                }
                else
                {
                    progressBar.Maximum = 1;
                    progressBar.Value = 0;
                }
            });

            var conflictMode = GetSelectedConflictMode();

            // 使用 Task.Run 將重工作業移至背景執行緒，確保 UI 流暢
            await Task.Run(() =>
                _fileProcessor.Process(
                    sourcePath,
                    targetPath,
                    progress,
                    conflictMode,
                    ShowConflictDialogThreadSafe));

            // 即使目前通常已在 UI 執行緒，仍透過 Invoke 統一回到 UI 執行緒更新控制項，
            // 可確保未來流程調整時仍符合 thread-safe 要求。
            this.Invoke(() =>
            {
                // 完成後恢復 UI 可操作狀態：重新啟用來源/目的/開始按鈕
                ToggleControls(true);

                // 完成狀態顯示
                lblStatus.Text = "狀態：處理完成";

                // ProgressBar 採一致策略：完成後重設為 0，避免殘留上一批次數值
                progressBar.Value = 0;

                // 加分需求：顯示完成提示前先把主視窗帶回前景
                this.Activate();

                // 指定 owner，避免提示視窗跑到其他螢幕或被主視窗蓋住
                MessageBox.Show(this, "轉換完成\n已跳過隱藏或系統檔案", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        finally
        {
            // 例外情況下仍需還原 UI，且必須在 UI 執行緒執行
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke(() =>
                {
                    ToggleControls(true);
                });
            }
        }
    }

    /// <summary>
    /// 統一控制 UI 是否可操作。
    /// </summary>
    private void ToggleControls(bool enabled)
    {
        btnBrowseSource.Enabled = enabled;
        btnBrowseTarget.Enabled = enabled;
        btnStart.Enabled = enabled;
    }

    private void rdoForceOverwrite_CheckedChanged(object sender, EventArgs e)
    {
        if (rdoForceOverwrite.Checked)
        {
            pnlOverwriteSubOptions.Enabled = true;

            if (!rdoOverwriteUnconditional.Checked && !rdoOverwriteByTime.Checked)
            {
                rdoOverwriteUnconditional.Checked = true;
            }

            return;
        }

        pnlOverwriteSubOptions.Enabled = false;
        rdoOverwriteUnconditional.Checked = false;
        rdoOverwriteByTime.Checked = false;
    }

    /// <summary>
    /// 取得目前 UI 所選擇的衝突處理模式。
    /// </summary>
    private ConflictMode GetSelectedConflictMode()
    {
        if (rdoForceOverwrite.Checked)
        {
            if (rdoOverwriteByTime.Checked)
            {
                return ConflictMode.ForceOverwriteByModifiedTime;
            }

            return ConflictMode.ForceOverwrite;
        }

        if (rdoAskEveryTime.Checked)
        {
            return ConflictMode.AskEveryTime;
        }

        return ConflictMode.AutoRename;
    }

    /// <summary>
    /// 背景執行緒呼叫時，使用 Invoke 切回 UI 執行緒顯示衝突視窗（必須使用 ShowDialog）。
    /// </summary>
    private DialogResultData ShowConflictDialogThreadSafe(string targetPath, DateTime oldTime, DateTime newTime)
    {
        if (InvokeRequired)
        {
            return (DialogResultData)Invoke(new Func<DialogResultData>(() =>
                ShowConflictDialogThreadSafe(targetPath, oldTime, newTime)));
        }

        using var dialog = new ConflictDialog(targetPath, oldTime, newTime);
        var result = dialog.ShowDialog(this);

        if (result == DialogResult.OK && dialog.ResultData != null)
        {
            return dialog.ResultData;
        }

        // 關閉視窗或未明確選擇時，預設略過
        return new DialogResultData
        {
            Action = ConflictAction.Skip,
            ApplyToAll = false
        };
    }
}
