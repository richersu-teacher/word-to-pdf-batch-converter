namespace WordToPdfTool;

public partial class ConflictDialog : Form
{
    public DialogResultData? ResultData { get; private set; }

    public ConflictDialog(string targetPath, DateTime oldTime, DateTime newTime)
    {
        InitializeComponent();

        lblFile.Text = $"檔案：{targetPath}";
        lblOldTime.Text = $"舊檔修改時間：{oldTime:yyyy-MM-dd HH:mm:ss}";
        lblNewTime.Text = $"新檔修改時間：{newTime:yyyy-MM-dd HH:mm:ss}";
    }

    private void btnOverwrite_Click(object sender, EventArgs e)
    {
        ResultData = new DialogResultData
        {
            Action = ConflictAction.Overwrite,
            ApplyToAll = chkApplyToAll.Checked
        };
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnKeepBoth_Click(object sender, EventArgs e)
    {
        ResultData = new DialogResultData
        {
            Action = ConflictAction.Rename,
            ApplyToAll = chkApplyToAll.Checked
        };
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnSkip_Click(object sender, EventArgs e)
    {
        ResultData = new DialogResultData
        {
            Action = ConflictAction.Skip,
            ApplyToAll = chkApplyToAll.Checked
        };
        DialogResult = DialogResult.OK;
        Close();
    }
}
