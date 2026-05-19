namespace WordToPdfTool;

/// <summary>
/// 衝突處理動作。
/// </summary>
public enum ConflictAction
{
    Overwrite,
    Rename,
    Skip
}

/// <summary>
/// 使用者在 UI 上選擇的衝突處理模式。
/// </summary>
public enum ConflictMode
{
    ForceOverwrite,
    AutoRename,
    AskEveryTime
}

/// <summary>
/// 路徑衝突解析結果。
/// </summary>
public sealed class ResolveResult
{
    public string FinalPath { get; init; } = string.Empty;
    public ConflictAction Action { get; init; }
}

/// <summary>
/// 詢問視窗回傳資料。
/// </summary>
public sealed class DialogResultData
{
    public ConflictAction Action { get; init; }
    public bool ApplyToAll { get; init; }
}
