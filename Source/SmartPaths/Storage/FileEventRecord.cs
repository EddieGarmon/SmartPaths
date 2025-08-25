namespace SmartPaths.Storage;

public class FileEventRecord : ILedgerRecord
{

    public FileEventRecord(LedgerAction action, AbsoluteFilePath? resultPath, AbsoluteFilePath? initialPath) {
        if (resultPath is null && initialPath is null) {
            throw new Exception("File event needs a result or initial path to be defined.");
        }
        Action = action;
        ResultPath = resultPath;
        InitialPath = initialPath;
        Timestamp = DateTime.Now;
    }

    public LedgerAction Action { get; }

    public AbsoluteFilePath? InitialPath { get; }

    public AbsoluteFilePath? ResultPath { get; }

    public DateTime Timestamp { get; }

    IPath? ILedgerRecord.InitialPath => InitialPath;

    IPath? ILedgerRecord.ResultPath => ResultPath;

}