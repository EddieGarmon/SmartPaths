namespace SmartPaths.Storage;

public class DiskWatcher : IFileSystemWatcher
{

    private readonly FileSystemWatcher _systemWatcher;
    private AbsoluteFolderPath _path;

    internal DiskWatcher(AbsoluteFolderPath folderPath, string filter, bool includeSubFolders, NotifyFilters notifyFilter) {
        _path = folderPath;
        _systemWatcher = new FileSystemWatcher(folderPath, filter);
        _systemWatcher.IncludeSubdirectories = includeSubFolders;
        _systemWatcher.NotifyFilter = notifyFilter;
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

    public event FileSystemEventHandler? Changed {
        add => _systemWatcher.Changed += value;
        remove => _systemWatcher.Changed -= value;
    }

    public event FileSystemEventHandler? Created {
        add => _systemWatcher.Created += value;
        remove => _systemWatcher.Created -= value;
    }

    public event FileSystemEventHandler? Deleted {
        add => _systemWatcher.Deleted += value;
        remove => _systemWatcher.Deleted -= value;
    }

    public event ErrorEventHandler? Error {
        add => _systemWatcher.Error += value;
        remove => _systemWatcher.Error -= value;
    }

    public event RenamedEventHandler? Renamed {
        add => _systemWatcher.Renamed += value;
        remove => _systemWatcher.Renamed -= value;
    }

}