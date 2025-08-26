namespace SmartPaths.Storage;

public class FolderEventRecord : ILedgerRecord
{

    public FolderEventRecord(LedgerAction action, AbsoluteFolderPath? resultPath, AbsoluteFolderPath? initialPath) {
        if (resultPath is null && initialPath is null) {
            throw new Exception("Folder event needs a result or initial path to be defined.");
        }
        Action = action;
        ResultPath = resultPath;
        InitialPath = initialPath;
        Timestamp = DateTime.Now;
    }

    public LedgerAction Action { get; }

    public AbsoluteFolderPath? InitialPath { get; }

    public AbsoluteFolderPath? ResultPath { get; }

    public DateTime Timestamp { get; }

    IPath? ILedgerRecord.InitialPath => InitialPath;

    IPath? ILedgerRecord.ResultPath => ResultPath;

}