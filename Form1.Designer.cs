namespace WordToPdfTool;

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
        txtSource = new TextBox();
        btnBrowseSource = new Button();
        txtTarget = new TextBox();
        btnBrowseTarget = new Button();
        btnStart = new Button();
        lblStatus = new Label();
        progressBar = new ProgressBar();
        lblSource = new Label();
        lblTarget = new Label();
        grpConflict = new GroupBox();
        rdoAskEveryTime = new RadioButton();
        rdoAutoRename = new RadioButton();
        rdoForceOverwrite = new RadioButton();
        grpConflict.SuspendLayout();
        SuspendLayout();
        // 
        // txtSource
        // 
        txtSource.Location = new Point(113, 22);
        txtSource.Name = "txtSource";
        txtSource.ReadOnly = true;
        txtSource.Size = new Size(525, 27);
        txtSource.TabIndex = 0;
        // 
        // btnBrowseSource
        // 
        btnBrowseSource.Location = new Point(654, 21);
        btnBrowseSource.Name = "btnBrowseSource";
        btnBrowseSource.Size = new Size(94, 29);
        btnBrowseSource.TabIndex = 1;
        btnBrowseSource.Text = "選擇來源";
        btnBrowseSource.UseVisualStyleBackColor = true;
        btnBrowseSource.Click += btnBrowseSource_Click;
        // 
        // txtTarget
        // 
        txtTarget.Location = new Point(113, 68);
        txtTarget.Name = "txtTarget";
        txtTarget.ReadOnly = true;
        txtTarget.Size = new Size(525, 27);
        txtTarget.TabIndex = 2;
        // 
        // btnBrowseTarget
        // 
        btnBrowseTarget.Location = new Point(654, 67);
        btnBrowseTarget.Name = "btnBrowseTarget";
        btnBrowseTarget.Size = new Size(94, 29);
        btnBrowseTarget.TabIndex = 3;
        btnBrowseTarget.Text = "選擇目的";
        btnBrowseTarget.UseVisualStyleBackColor = true;
        btnBrowseTarget.Click += btnBrowseTarget_Click;
        // 
        // btnStart
        // 
        btnStart.Location = new Point(654, 113);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(94, 32);
        btnStart.TabIndex = 4;
        btnStart.Text = "開始處理";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoEllipsis = true;
        lblStatus.BorderStyle = BorderStyle.FixedSingle;
        lblStatus.Location = new Point(23, 264);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(725, 29);
        lblStatus.TabIndex = 5;
        lblStatus.Text = "狀態：待命中";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(23, 310);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(725, 29);
        progressBar.TabIndex = 6;
        // 
        // 
        // grpConflict
        // 
        grpConflict.Controls.Add(rdoAskEveryTime);
        grpConflict.Controls.Add(rdoAutoRename);
        grpConflict.Controls.Add(rdoForceOverwrite);
        grpConflict.Location = new Point(23, 113);
        grpConflict.Name = "grpConflict";
        grpConflict.Size = new Size(615, 132);
        grpConflict.TabIndex = 9;
        grpConflict.TabStop = false;
        grpConflict.Text = "遇到同名檔案衝突時的處理方式";
        // 
        // rdoAskEveryTime
        // 
        rdoAskEveryTime.AutoSize = true;
        rdoAskEveryTime.Location = new Point(21, 96);
        rdoAskEveryTime.Name = "rdoAskEveryTime";
        rdoAskEveryTime.Size = new Size(88, 23);
        rdoAskEveryTime.TabIndex = 2;
        rdoAskEveryTime.Text = "每次詢問";
        rdoAskEveryTime.UseVisualStyleBackColor = true;
        // 
        // rdoAutoRename
        // 
        rdoAutoRename.AutoSize = true;
        rdoAutoRename.Checked = true;
        rdoAutoRename.Location = new Point(21, 62);
        rdoAutoRename.Name = "rdoAutoRename";
        rdoAutoRename.Size = new Size(168, 23);
        rdoAutoRename.TabIndex = 1;
        rdoAutoRename.TabStop = true;
        rdoAutoRename.Text = "並存自動加序號（預設）";
        rdoAutoRename.UseVisualStyleBackColor = true;
        // 
        // rdoForceOverwrite
        // 
        rdoForceOverwrite.AutoSize = true;
        rdoForceOverwrite.Location = new Point(21, 30);
        rdoForceOverwrite.Name = "rdoForceOverwrite";
        rdoForceOverwrite.Size = new Size(88, 23);
        rdoForceOverwrite.TabIndex = 0;
        rdoForceOverwrite.Text = "強制覆蓋";
        rdoForceOverwrite.UseVisualStyleBackColor = true;
        // 
        // lblSource
        // 
        lblSource.AutoSize = true;
        lblSource.Location = new Point(23, 25);
        lblSource.Name = "lblSource";
        lblSource.Size = new Size(84, 19);
        lblSource.TabIndex = 7;
        lblSource.Text = "來源資料夾";
        // 
        // lblTarget
        // 
        lblTarget.AutoSize = true;
        lblTarget.Location = new Point(23, 71);
        lblTarget.Name = "lblTarget";
        lblTarget.Size = new Size(84, 19);
        lblTarget.TabIndex = 8;
        lblTarget.Text = "目的資料夾";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(9F, 19F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(774, 357);
        Controls.Add(grpConflict);
        Controls.Add(lblTarget);
        Controls.Add(lblSource);
        Controls.Add(progressBar);
        Controls.Add(lblStatus);
        Controls.Add(btnStart);
        Controls.Add(btnBrowseTarget);
        Controls.Add(txtTarget);
        Controls.Add(btnBrowseSource);
        Controls.Add(txtSource);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Word 轉 PDF 工具";
        grpConflict.ResumeLayout(false);
        grpConflict.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TextBox txtSource;
    private Button btnBrowseSource;
    private TextBox txtTarget;
    private Button btnBrowseTarget;
    private Button btnStart;
    private Label lblStatus;
    private ProgressBar progressBar;
    private Label lblSource;
    private Label lblTarget;
    private GroupBox grpConflict;
    private RadioButton rdoAskEveryTime;
    private RadioButton rdoAutoRename;
    private RadioButton rdoForceOverwrite;
}
