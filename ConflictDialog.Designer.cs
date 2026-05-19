namespace WordToPdfTool;

partial class ConflictDialog
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblFile = new Label();
        lblOldTime = new Label();
        lblNewTime = new Label();
        btnOverwrite = new Button();
        btnKeepBoth = new Button();
        btnSkip = new Button();
        chkApplyToAll = new CheckBox();
        SuspendLayout();
        // 
        // lblFile
        // 
        lblFile.AutoEllipsis = true;
        lblFile.BorderStyle = BorderStyle.FixedSingle;
        lblFile.Location = new Point(20, 20);
        lblFile.Name = "lblFile";
        lblFile.Size = new Size(560, 48);
        lblFile.TabIndex = 0;
        lblFile.Text = "檔案：";
        // 
        // lblOldTime
        // 
        lblOldTime.AutoSize = true;
        lblOldTime.Location = new Point(20, 84);
        lblOldTime.Name = "lblOldTime";
        lblOldTime.Size = new Size(108, 19);
        lblOldTime.TabIndex = 1;
        lblOldTime.Text = "舊檔修改時間：";
        // 
        // lblNewTime
        // 
        lblNewTime.AutoSize = true;
        lblNewTime.Location = new Point(20, 114);
        lblNewTime.Name = "lblNewTime";
        lblNewTime.Size = new Size(108, 19);
        lblNewTime.TabIndex = 2;
        lblNewTime.Text = "新檔修改時間：";
        // 
        // btnOverwrite
        // 
        btnOverwrite.Location = new Point(20, 184);
        btnOverwrite.Name = "btnOverwrite";
        btnOverwrite.Size = new Size(120, 32);
        btnOverwrite.TabIndex = 4;
        btnOverwrite.Text = "覆蓋";
        btnOverwrite.UseVisualStyleBackColor = true;
        btnOverwrite.Click += btnOverwrite_Click;
        // 
        // btnKeepBoth
        // 
        btnKeepBoth.Location = new Point(154, 184);
        btnKeepBoth.Name = "btnKeepBoth";
        btnKeepBoth.Size = new Size(170, 32);
        btnKeepBoth.TabIndex = 5;
        btnKeepBoth.Text = "保留兩者(加序號)";
        btnKeepBoth.UseVisualStyleBackColor = true;
        btnKeepBoth.Click += btnKeepBoth_Click;
        // 
        // btnSkip
        // 
        btnSkip.Location = new Point(338, 184);
        btnSkip.Name = "btnSkip";
        btnSkip.Size = new Size(120, 32);
        btnSkip.TabIndex = 6;
        btnSkip.Text = "略過";
        btnSkip.UseVisualStyleBackColor = true;
        btnSkip.Click += btnSkip_Click;
        // 
        // chkApplyToAll
        // 
        chkApplyToAll.AutoSize = true;
        chkApplyToAll.Location = new Point(20, 150);
        chkApplyToAll.Name = "chkApplyToAll";
        chkApplyToAll.Size = new Size(92, 23);
        chkApplyToAll.TabIndex = 3;
        chkApplyToAll.Text = "套用至全部";
        chkApplyToAll.UseVisualStyleBackColor = true;
        // 
        // ConflictDialog
        // 
        AcceptButton = btnKeepBoth;
        AutoScaleDimensions = new SizeF(9F, 19F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnSkip;
        ClientSize = new Size(602, 236);
        Controls.Add(chkApplyToAll);
        Controls.Add(btnSkip);
        Controls.Add(btnKeepBoth);
        Controls.Add(btnOverwrite);
        Controls.Add(lblNewTime);
        Controls.Add(lblOldTime);
        Controls.Add(lblFile);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ConflictDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "檔案衝突處理";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblFile;
    private Label lblOldTime;
    private Label lblNewTime;
    private Button btnOverwrite;
    private Button btnKeepBoth;
    private Button btnSkip;
    private CheckBox chkApplyToAll;
}
