namespace SmartPaths.Storage;

public class Ledger : IDisposable
{

    private readonly IFileSystem _fileSystem;
    private readonly List<ILedgerRecord> _records = [];
    private readonly IFileSystemWatcher _watcher;

    internal Ledger(IFileSystem fileSystem, AbsoluteFolderPath ledgerRoot) {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(ledgerRoot);

        _fileSystem = fileSystem;
        _watcher = _fileSystem.GetWatcher(ledgerRoot, includeSubFolders: true).Result;

        _watcher.FileCreated += LogFileRecord;
        _watcher.FileEdited += LogFileRecord;
        _watcher.FileMoved += LogFileRecord;
        _watcher.FileDeleted += LogFileRecord;

        _watcher.FolderCreated += LogFolderRecord;
        _watcher.FolderMoved += LogFolderRecord;
        _watcher.FolderDeleted += LogFolderRecord;

        _watcher.EnableRaisingEvents = true;
    }

    public IEnumerable<ILedgerRecord> AllEvents => _records.ToList();

    public IEnumerable<FileEventRecord> FileEvents => _records.Where(record => record is FileEventRecord).Cast<FileEventRecord>().ToList();

    public IEnumerable<FolderEventRecord> FolderEvents => _records.Where(record => record is FolderEventRecord).Cast<FolderEventRecord>().ToList();

    public int RecordCount => _records.Count;

    public async Task<string> GetAllText(AbsoluteFilePath path) {
        ArgumentNullException.ThrowIfNull(path);
        IFile? file = await _fileSystem.GetFile(path);
        if (file is null) {
            return string.Empty;
        }
        using Stream stream = await file.OpenToRead();
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }

    public async Task<Stream> GetFileStream(AbsoluteFilePath path) {
        ArgumentNullException.ThrowIfNull(path);
        IFile? file = await _fileSystem.GetFile(path);
        if (file is null) {
            return Stream.Null;
        }
        return await file.OpenToRead();
    }

    void IDisposable.Dispose() {
        _watcher.EnableRaisingEvents = false;
        GC.SuppressFinalize(this);
    }

    private void LogFileRecord(FileEventRecord record) {
        _records.Add(record);
    }

    private void LogFolderRecord(FolderEventRecord record) {
        _records.Add(record);
    }

}