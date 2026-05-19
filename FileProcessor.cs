using System.Runtime.InteropServices;
using System.Linq;

namespace WordToPdfTool;

/// <summary>
/// 處理進度資訊，用於回報 UI。
/// </summary>
public sealed class ProcessingProgressInfo
{
    public int CurrentIndex { get; init; }
    public int TotalCount { get; init; }
    public string CurrentFileName { get; init; } = "";
}

/// <summary>
/// 核心處理類別：遞迴走訪、Word 轉 PDF、其他檔案複製、錯誤紀錄。
/// </summary>
public sealed class FileProcessor
{
    private readonly string _logPath = Path.Combine(AppContext.BaseDirectory, "log.txt");
    private ConflictAction? _globalChoice;
    private bool _hasErrors;

    private Func<string, DateTime, DateTime, DialogResultData>? _askConflict;
    private ConflictMode _conflictMode;

    /// <summary>
    /// 執行整體流程（同步方法，會由 UI 端以 Task.Run 包起來）。
    /// </summary>
    public void Process(
        string sourceRoot,
        string targetRoot,
        IProgress<ProcessingProgressInfo>? progress,
        ConflictMode conflictMode,
        Func<string, DateTime, DateTime, DialogResultData> askConflict)
    {
        _hasErrors = false;
        sourceRoot = ValidateAndNormalizeRootPath(sourceRoot, nameof(sourceRoot));
        targetRoot = ValidateAndNormalizeRootPath(targetRoot, nameof(targetRoot));

        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException($"來源資料夾不存在：{sourceRoot}");
        }

        _conflictMode = conflictMode;
        _askConflict = askConflict;

        // 依 UI 選擇初始化全域策略：
        // - 強制覆蓋  -> 預設 Overwrite
        // - 並存改名  -> 預設 Rename
        // - 每次詢問  -> 先設 null，首次衝突才跳 Dialog
        _globalChoice = conflictMode switch
        {
            ConflictMode.ForceOverwrite => ConflictAction.Overwrite,
            ConflictMode.AutoRename => ConflictAction.Rename,
            _ => null
        };

        Directory.CreateDirectory(targetRoot);

        // ===== 極限效能重構：整批只建立一個 Word.Application（單執行緒共用） =====
        object? wordApp = null;

        try
        {
            // 建立單一 Word COM 執行個體（僅建立一次）
            // 若 Word 無法建立，仍允許非 Word 檔案繼續複製，避免整批中斷。
            try
            {
                var wordType = Type.GetTypeFromProgID("Word.Application")
                    ?? throw new InvalidOperationException("找不到 Word COM（Word.Application）。請確認本機已安裝 Microsoft Word。");

                wordApp = Activator.CreateInstance(wordType)
                    ?? throw new InvalidOperationException("無法建立 Word COM 執行個體。");

                // 靜默與加速設定
                dynamic app = wordApp;
                app.Visible = false;
                app.ScreenUpdating = false;
                app.DisplayAlerts = 0;

                // 資安強化：停用所有 Word 巨集（3 = ForceDisable）
                app.AutomationSecurity = 3;
            }
            catch (Exception ex)
            {
                LogError("[Word.Application 初始化]", ex);
                wordApp = null;
            }

            // 先收集所有檔案供進度條使用
            var files = Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories);
            var totalCount = files.Length;
            var currentIndex = 0;

            // 依資料夾逐層處理，並於每個資料夾內分三階段：非 Word -> .doc -> .docx
            var directories = Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories)
                .Prepend(sourceRoot)
                .ToArray();

            foreach (var currentDir in directories)
            {
                var dirFiles = Directory.GetFiles(currentDir, "*", SearchOption.TopDirectoryOnly)
                    // 資安強化：限制檔案類型、禁止隱藏/系統檔複製、停用 Word 巨集
                    // 這裡先排除系統暫存與垃圾檔案（Word 暫存、Mac 系統檔、Windows 快取）
                    .Where(f =>
                    {
                        var fileName = Path.GetFileName(f);

                        if (fileName.StartsWith("~$", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        if (fileName.Equals(".DS_Store", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        if (fileName.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        // 隱藏/系統檔案一律跳過：不複製、不處理
                        var attr = File.GetAttributes(f);
                        if ((attr & FileAttributes.Hidden) != 0 ||
                            (attr & FileAttributes.System) != 0)
                        {
                            return false;
                        }

                        return true;
                    })
                    .ToArray();

                var nonWordFiles = dirFiles
                    .Where(f =>
                    {
                        var ext = Path.GetExtension(f);
                        return !ext.Equals(".doc", StringComparison.OrdinalIgnoreCase) &&
                               !ext.Equals(".docx", StringComparison.OrdinalIgnoreCase);
                    })
                    .ToArray();

                var docFiles = dirFiles
                    .Where(f => Path.GetExtension(f).Equals(".doc", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                var docxFiles = dirFiles
                    .Where(f => Path.GetExtension(f).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                // 階段 1：先處理非 Word 檔
                foreach (var sourceFile in nonWordFiles)
                {
                    currentIndex++;
                    ProcessSingleFile(sourceRoot, targetRoot, sourceFile, false, progress, totalCount, currentIndex, wordApp);
                }

                // 階段 2：再處理 .doc
                foreach (var sourceFile in docFiles)
                {
                    currentIndex++;
                    ProcessSingleFile(sourceRoot, targetRoot, sourceFile, true, progress, totalCount, currentIndex, wordApp);
                }

                // 階段 3：最後處理 .docx
                foreach (var sourceFile in docxFiles)
                {
                    currentIndex++;
                    ProcessSingleFile(sourceRoot, targetRoot, sourceFile, true, progress, totalCount, currentIndex, wordApp);
                }
            }
        }
        finally
        {
            // 無論成功或失敗，最外層都必須確保 Quit + Release
            if (wordApp != null)
            {
                try
                {
                    ((dynamic)wordApp).Quit(false);
                }
                catch
                {
                }

                try
                {
                    Marshal.ReleaseComObject(wordApp);
                }
                catch
                {
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    /// <summary>
    /// 單一檔案處理（包含進度回報與錯誤處理）。
    /// </summary>
    private void ProcessSingleFile(
        string sourceRoot,
        string targetRoot,
        string sourceFile,
        bool isWordFile,
        IProgress<ProcessingProgressInfo>? progress,
        int totalCount,
        int currentIndex,
        object? wordApp)
    {
        var relativePath = Path.GetRelativePath(sourceRoot, sourceFile);
        var targetFile = Path.Combine(targetRoot, relativePath);
        var targetDir = Path.GetDirectoryName(targetFile) ?? targetRoot;

        Directory.CreateDirectory(targetDir);

        progress?.Report(new ProcessingProgressInfo
        {
            CurrentIndex = currentIndex,
            TotalCount = totalCount,
            CurrentFileName = Path.GetFileName(sourceFile)
        });

        try
        {
            if (isWordFile)
            {
                if (wordApp == null)
                {
                    LogError(sourceFile, new InvalidOperationException("Word COM 不可用，已略過 Word 轉檔。"));
                    return;
                }

                // Word 轉檔流程：
                // 1) 先計算預設 PDF 路徑
                var defaultPdfPath = Path.Combine(
                    targetDir,
                    Path.GetFileNameWithoutExtension(sourceFile) + ".pdf");

                // 2) 無條件呼叫 ResolveOutputPath（由 Resolve 內部判斷是否衝突）
                var resolve = ResolveOutputPath(defaultPdfPath, DateTime.Now);
                var finalPath = resolve.FinalPath;

                LogInfo($"[Word衝突解析] 來源={sourceFile} | 預設={defaultPdfPath} | 動作={resolve.Action} | 最終={finalPath}");

                // 3) 若略過，直接 return
                if (resolve.Action == ConflictAction.Skip)
                {
                    return;
                }

                // 4) 直接輸出到 FinalPath
                // 寫入點必須使用 ResolveOutputPath 回傳的 FinalPath
                ConvertWordToPdf(sourceFile, finalPath, wordApp);
            }
            else
            {
                var sourceLastWriteTime = File.GetLastWriteTime(sourceFile);

                // 非 Word 檔也無條件呼叫 ResolveOutputPath
                var resolve = ResolveOutputPath(targetFile, sourceLastWriteTime);
                var finalPath = resolve.FinalPath;

                LogInfo($"[檔案衝突解析] 來源={sourceFile} | 預設={targetFile} | 動作={resolve.Action} | 最終={finalPath}");

                if (resolve.Action == ConflictAction.Skip)
                {
                    return;
                }

                // 寫入點必須使用 ResolveOutputPath 回傳的 FinalPath
                File.Copy(sourceFile, finalPath, overwrite: resolve.Action == ConflictAction.Overwrite);
            }
        }
        catch (Exception ex)
        {
            LogError(sourceFile, ex);
            // 單一檔案失敗不影響整體流程
        }
    }

    /// <summary>
    /// 統一的輸出路徑衝突解析入口。
    /// </summary>
    public ResolveResult ResolveOutputPath(string targetPath, DateTime sourceLastWriteTime)
    {
        if (!File.Exists(targetPath))
        {
            return new ResolveResult
            {
                FinalPath = targetPath,
                Action = ConflictAction.Overwrite
            };
        }

        var action = DecideConflictAction(targetPath, sourceLastWriteTime);

        if (action == ConflictAction.Skip)
        {
            return new ResolveResult
            {
                FinalPath = targetPath,
                Action = ConflictAction.Skip
            };
        }

        if (action == ConflictAction.Overwrite)
        {
            return new ResolveResult
            {
                FinalPath = targetPath,
                Action = ConflictAction.Overwrite
            };
        }

        // Rename 規則：只取原始 baseName + ext，再加 (n)
        // 例如 123.pdf -> 123(1).pdf、123(2).pdf，避免 123(1)(1).pdf
        var directory = Path.GetDirectoryName(targetPath) ?? string.Empty;
        var baseName = Path.GetFileNameWithoutExtension(targetPath);
        var ext = Path.GetExtension(targetPath);
        var index = 1;

        string candidatePath;
        do
        {
            var candidateName = $"{baseName}({index}){ext}";
            candidatePath = Path.Combine(directory, candidateName);
            index++;
        } while (File.Exists(candidatePath));

        return new ResolveResult
        {
            FinalPath = candidatePath,
            Action = ConflictAction.Rename
        };
    }

    /// <summary>
    /// 依據模式 / 全域記憶 / 對話框決定衝突動作。
    /// </summary>
    private ConflictAction DecideConflictAction(string targetPath, DateTime sourceLastWriteTime)
    {
        // 若已有預設或「套用至全部」結果，直接使用
        if (_globalChoice.HasValue)
        {
            return _globalChoice.Value;
        }

        // Ask 模式才會走到此處（globalChoice 為 null）
        if (_conflictMode != ConflictMode.AskEveryTime)
        {
            // 正常情況不應進入，保底回傳覆蓋
            return ConflictAction.Overwrite;
        }

        if (_askConflict == null)
        {
            // Ask 模式衝突時禁止靜默降級：缺少 UI 回呼視為程式邏輯錯誤
            throw new InvalidOperationException("AskEveryTime 模式需要可用的衝突對話框回呼。請確認 Form1 已正確傳入 Invoke + ShowDialog 委派。");
        }

        var oldTime = File.GetLastWriteTime(targetPath);
        var dialogData = _askConflict(targetPath, oldTime, sourceLastWriteTime);

        if (dialogData.ApplyToAll)
        {
            _globalChoice = dialogData.Action;
        }

        return dialogData.Action;
    }

    /// <summary>
    /// 使用 Word Interop 將單一 Word 檔轉為 PDF。
    /// </summary>
    private void ConvertWordToPdf(string sourcePath, string targetPdfPath, object? wordApp)
    {
        object? documents = null;
        object? document = null;
        var docClosed = false;

        try
        {
            if (wordApp == null)
            {
                throw new InvalidOperationException("Word COM 執行個體不存在，無法進行轉檔。");
            }

            dynamic app = wordApp;
            app.Visible = false;
            app.DisplayAlerts = 0; // wdAlertsNone

            documents = app.Documents;
            dynamic docs = documents;
            document = docs.Open(sourcePath, ReadOnly: true, Visible: false);

            dynamic doc = document;
            doc.ExportAsFixedFormat(targetPdfPath, 17); // 17 = wdExportFormatPDF

            LogInfo($"[Word輸出完成] 來源={sourcePath} | 輸出={targetPdfPath}");

            doc.Close(false);
            docClosed = true;
        }
        catch (Exception ex)
        {
            LogError(sourcePath, ex);
            // 單一文件失敗不影響整體流程，交由上層繼續處理下一個檔案
        }
        finally
        {
            // 先關閉文件，再釋放 COM，避免殭屍程序
            if (document != null)
            {
                if (!docClosed)
                {
                    try
                    {
                        ((dynamic)document).Close(false);
                    }
                    catch
                    {
                    }
                }

                try
                {
                    Marshal.ReleaseComObject(document);
                }
                catch
                {
                    // 釋放失敗不拋出，避免中斷流程
                }

                document = null;
            }

            if (documents != null)
            {
                try
                {
                    Marshal.ReleaseComObject(documents);
                }
                catch
                {
                }

                documents = null;
            }
        }
    }

    /// <summary>
    /// 寫入錯誤記錄至執行檔同層的 log.txt。
    /// </summary>
    private void LogError(string filePath, Exception ex)
    {
        _hasErrors = true;
        var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 檔案：{filePath}{Environment.NewLine}錯誤：{ex.Message}{Environment.NewLine}{Environment.NewLine}";

        try
        {
            File.AppendAllText(_logPath, message);
        }
        catch
        {
            // 記錄失敗時不再拋錯，避免影響主流程
        }
    }

    /// <summary>
    /// 提供 UI 判斷是否需顯示統一錯誤訊息：
    /// 「部分檔案處理失敗，請查看 log.txt」
    /// </summary>
    public bool HasErrors => _hasErrors;

    /// <summary>
    /// 路徑安全檢查：
    /// 1) 不接受空白路徑
    /// 2) 必須為絕對路徑
    /// 3) 正規化後不可包含相對跳脫（..）
    /// </summary>
    private static string ValidateAndNormalizeRootPath(string inputPath, string paramName)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("路徑不可為空白。", paramName);
        }

        if (!Path.IsPathRooted(inputPath))
        {
            throw new ArgumentException("僅允許絕對路徑。", paramName);
        }

        var fullPath = Path.GetFullPath(inputPath);
        var segments = inputPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (segments.Any(s => s == ".."))
        {
            throw new ArgumentException("不允許使用相對路徑跳脫（..）。", paramName);
        }

        return fullPath;
    }

    /// <summary>
    /// 一般資訊記錄（用於除錯衝突決策與最終寫入路徑）。
    /// </summary>
    private void LogInfo(string message)
    {
        var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

        try
        {
            File.AppendAllText(_logPath, text);
        }
        catch
        {
            // 記錄失敗時不拋錯，避免影響主流程
        }
    }
}
