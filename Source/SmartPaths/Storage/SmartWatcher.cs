using System.Collections.ObjectModel;

namespace SmartPaths.Storage;

public class SmartWatcher : IFileSystemWatcher
{

    private readonly FilterCollection _filters = [];

    internal SmartWatcher(AbsoluteFolderPath path, string? filter, bool includeSubdirectories) {
        ArgumentNullException.ThrowIfNull(path);

        Path = path;
        Filter = filter ?? DefaultFilter;
        IncludeSubdirectories = includeSubdirectories;
    }

    public bool EnableRaisingEvents { get; set; }

    /// <summary>Used to specify a single name pattern. for more than one filter use <see cref="Filters" />
    ///     .</summary>
    public string Filter {
        get => _filters.Count == 0 ? DefaultFilter : _filters[0];
        set {
            _filters.Clear();
            if (!string.IsNullOrEmpty(value)) {
                _filters.Add(value);
            }
        }
    }

    public Collection<string> Filters => _filters;

    public bool IncludeSubdirectories { get; set; }

    public AbsoluteFolderPath Path { get; set; }

    public event Action<FileEventRecord>? FileCreated;

    public event Action<FileEventRecord>? FileDeleted;

    public event Action<FileEventRecord>? FileEdited;

    public event Action<FileEventRecord>? FileMoved;

    public event Action<FolderEventRecord>? FolderCreated;

    public event Action<FolderEventRecord>? FolderDeleted;

    public event Action<FolderEventRecord>? FolderMoved;

    internal void ProcessStorageEvent(FolderEventRecord record) {
        ArgumentNullException.ThrowIfNull(record);
        if (!EnableRaisingEvents) {
            return;
        }
        //need to check if path is a direct child of watch path
        if (!IsWatchedFolder(record.InitialPath) && !IsWatchedFolder(record.ResultPath)) {
            return;
        }
        //paths match, now check Filters
        if (!_filters.IsMatch(record.InitialPath?.FolderName) && !_filters.IsMatch(record.ResultPath?.FolderName)) {
            return;
        }
        //pump the appropriate events
        switch (record.Action) {
            case LedgerAction.FolderCreated:
                FolderCreated?.Invoke(record);
                break;
            case LedgerAction.FolderMoved:
                FolderMoved?.Invoke(record);
                break;
            case LedgerAction.FolderDeleted:
                FolderDeleted?.Invoke(record);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal void ProcessStorageEvent(FileEventRecord record) {
        ArgumentNullException.ThrowIfNull(record);
        if (!EnableRaisingEvents) {
            return;
        }
        //need to check if path is a direct child of watch path
        if (!IsWatchedFolder(record.InitialPath?.Folder) && !IsWatchedFolder(record.ResultPath?.Folder)) {
            return;
        }
        //paths match, now check Filters
        if (!_filters.IsMatch(record.InitialPath?.FileName) && !_filters.IsMatch(record.ResultPath?.FileName)) {
            return;
        }
        //pump the appropriate events
        switch (record.Action) {
            case LedgerAction.FileCreated:
                FileCreated?.Invoke(record);
                break;
            case LedgerAction.FileEdited:
                FileEdited?.Invoke(record);
                break;
            case LedgerAction.FileMoved:
                FileMoved?.Invoke(record);
                break;
            case LedgerAction.FileDeleted:
                FileDeleted?.Invoke(record);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool IsWatchedFolder(AbsoluteFolderPath? folder) {
        if (folder is null) {
            return false;
        }
        if (folder == Path) {
            return true;
        }
        return IncludeSubdirectories && folder.ToString().StartsWith(Path.ToString());
    }

    public const string DefaultFilter = "*";

}