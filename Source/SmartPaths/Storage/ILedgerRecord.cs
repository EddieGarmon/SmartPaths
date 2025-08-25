namespace SmartPaths.Storage;

public interface ILedgerRecord
{

    LedgerAction Action { get; }

    public IPath? InitialPath { get; }

    public IPath? ResultPath { get; }

    public DateTime Timestamp { get; }

}