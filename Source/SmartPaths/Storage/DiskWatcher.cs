namespace SmartPaths.Storage;

public class DiskWatcher : IFileSystemWatcher
{

    private readonly FileSystemWatcher _systemWatcher;
    private AbsoluteFolderPath _path;

    internal DiskWatcher(AbsoluteFolderPath folderPath,
                         string filter,
                         bool includeSubFolders,
                         NotifyFilters notifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.CreationTime) {
        string[] logicalDrives = Environment.GetLogicalDrives();

        _path = folderPath;
        _systemWatcher = new FileSystemWatcher(folderPath, filter);
        _systemWatcher.IncludeSubdirectories = includeSubFolders;
        _systemWatcher.NotifyFilter = notifyFilter;

        _systemWatcher.Created += (sender, e) => {
                                      if (e.ChangeType != WatcherChangeTypes.Created) {
                                          //todo: log and leave
                                          return;
                                      }
                                      IPath path = SmartPath.Parse(e.FullPath);
                                      switch (path) {
                                          case AbsoluteFolderPath absoluteFolderPath:
                                              FolderEventRecord folderRecord = new(LedgerAction.FolderCreated, absoluteFolderPath, null);
                                              FolderCreated?.Invoke(folderRecord);
                                              break;
                                          case AbsoluteFilePath absoluteFilePath:
                                              FileEventRecord fileRecord = new(LedgerAction.FileCreated, absoluteFilePath, null);
                                              FileCreated?.Invoke(fileRecord);
                                              break;
                                          default:
                                              throw new ArgumentOutOfRangeException(nameof(path));
                                      }
                                  };
        _systemWatcher.Deleted += (sender, e) => {
                                      if (e.ChangeType != WatcherChangeTypes.Deleted) {
                                          //todo: log and leave
                                          return;
                                      }
                                      IPath path = SmartPath.Parse(e.FullPath);
                                      switch (path) {
                                          case AbsoluteFolderPath absoluteFolderPath:
                                              FolderEventRecord folderRecord = new(LedgerAction.FolderDeleted, null, absoluteFolderPath);
                                              FolderDeleted?.Invoke(folderRecord);
                                              break;
                                          case AbsoluteFilePath absoluteFilePath:
                                              FileEventRecord fileRecord = new(LedgerAction.FileDeleted, null, absoluteFilePath);
                                              FileDeleted?.Invoke(fileRecord);
                                              break;
                                          default:
                                              throw new ArgumentOutOfRangeException(nameof(path));
                                      }
                                      ;
                                  };
        _systemWatcher.Changed += (sender, e) => {
                                      if (e.ChangeType != WatcherChangeTypes.Changed) {
                                          //todo: log and leave
                                          return;
                                      }
                                      IPath path = SmartPath.Parse(e.FullPath);
                                      if (path is not AbsoluteFilePath filePath) {
                                          throw new ArgumentOutOfRangeException(nameof(path));
                                      }
                                      FileEventRecord fileEventRecord = new(LedgerAction.FileEdited, filePath, filePath);
                                      FileEdited?.Invoke(fileEventRecord);
                                  };
        _systemWatcher.Renamed += (sender, e) => {
                                      if (e.ChangeType != WatcherChangeTypes.Renamed) {
                                          //todo: log and leave
                                          return;
                                      }
                                      IPath oldPath = SmartPath.Parse(e.FullPath);
                                      IPath newPath = SmartPath.Parse(e.FullPath);
                                      if (oldPath.GetType() != newPath.GetType()) {
                                          throw new InvalidOperationException(
                                              $"Renamed event for path [{oldPath}] resulted in a change of path type to [{newPath}]. This is not supported.");
                                      }
                                      switch (oldPath) {
                                          case AbsoluteFolderPath absoluteFolderPath:
                                              FolderEventRecord folderRecord = new(LedgerAction.FolderMoved, newPath as AbsoluteFolderPath, absoluteFolderPath);
                                              FolderMoved?.Invoke(folderRecord);
                                              break;
                                          case AbsoluteFilePath absoluteFilePath:
                                              FileEventRecord fileRecord = new(LedgerAction.FileMoved, newPath as AbsoluteFilePath, absoluteFilePath);
                                              FileMoved?.Invoke(fileRecord);
                                              break;
                                          default:
                                              throw new ArgumentOutOfRangeException(nameof(oldPath));
                                      }
                                  };
        _systemWatcher.Error += (sender, e) => {
                                    //TODO: Add logging here?
                                };
    }

    public bool EnableRaisingEvents {
        get => _systemWatcher.EnableRaisingEvents;
        set => _systemWatcher.EnableRaisingEvents = value;
    }

    public string Filter {
        get => _systemWatcher.Filter;
        set => _systemWatcher.Filter = value;
    }

    public bool IncludeSubdirectories {
        get => _systemWatcher.IncludeSubdirectories;
        set => _systemWatcher.IncludeSubdirectories = value;
    }

    public NotifyFilters NotifyFilter {
        get => _systemWatcher.NotifyFilter;
        set => _systemWatcher.NotifyFilter = value;
    }

    public AbsoluteFolderPath Path {
        get => _path;
        set {
            _path = value;
            _systemWatcher.Path = value;
        }
    }

    public event Action<FileEventRecord>? FileCreated;

    public event Action<FileEventRecord>? FileDeleted;

    public event Action<FileEventRecord>? FileEdited;

    public event Action<FileEventRecord>? FileMoved;

    public event Action<FolderEventRecord>? FolderCreated;

    public event Action<FolderEventRecord>? FolderDeleted;

    public event Action<FolderEventRecord>? FolderMoved;

}