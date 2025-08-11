namespace SmartPaths.Storage;

public class RamWatcher : IFileSystemWatcher
{

    internal RamWatcher(AbsoluteFolderPath folderPath, string? filter, bool includeSubFolders, NotifyFilters notifyFilter) {
        ArgumentNullException.ThrowIfNull(folderPath);

        Path = folderPath;
        Filter = filter ?? "*";
        IncludeSubdirectories = includeSubFolders;
        NotifyFilter = notifyFilter;
    }

    public bool EnableRaisingEvents { get; set; }

    public string Filter { get; set; }

    public bool IncludeSubdirectories { get; set; }

    public NotifyFilters NotifyFilter { get; set; }

    public AbsoluteFolderPath Path { get; set; }

    public event FileSystemEventHandler? Changed;

    public event FileSystemEventHandler? Created;

    public event FileSystemEventHandler? Deleted;

    public event ErrorEventHandler? Error;

    public event RenamedEventHandler? Renamed;

    internal void ProcessErrorEvent(ErrorEventArgs args) {
        if (!EnableRaisingEvents) {
            return;
        }
        Error?.Invoke(this, args);
    }

    internal void ProcessStorageEvent(FileSystemEventArgs args) {
        if (!EnableRaisingEvents) {
            return;
        }
        if (!ShouldHandle(args.FullPath)) {
            return;
        }
        switch (args.ChangeType) {
            case WatcherChangeTypes.Changed:
                Changed?.Invoke(this, args);
                break;
            case WatcherChangeTypes.Created:
                Created?.Invoke(this, args);
                break;
            case WatcherChangeTypes.Deleted:
                Deleted?.Invoke(this, args);
                break;
            //case WatcherChangeTypes.Renamed:
            //    //todo: ensure handled below
            //    //if (args is RenamedEventArgs renamed) {
            //    //    Renamed?.Invoke(this, renamed);
            //    //} else {
            //    //    Renamed?.Invoke(this, new RenamedEventArgs(args.ChangeType, args.FullPath, args.Name, null));
            //    //}
            //    break;
            //default:
            //    throw new ArgumentOutOfRangeException();
        }
    }

    internal void ProcessStorageEvent(RenamedEventArgs args) {
        if (!EnableRaisingEvents) {
            return;
        }
        if (ShouldHandle(args.FullPath) || ShouldHandle(args.OldFullPath)) {
            Renamed?.Invoke(this, args);
        }
    }

    private bool ShouldHandle(string fullPath) {
        //todo: if this path is the same, or if matching children is a descendant of
        return false;
    }

}