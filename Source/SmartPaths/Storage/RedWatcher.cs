using System.Collections.ObjectModel;

namespace SmartPaths.Storage;

public sealed class RedWatcher : IFileSystemWatcher
{

    private readonly FilterCollection _filters = [];

    internal RedWatcher(AbsoluteFolderPath folderPath, string? filter, bool includeSubFolders, NotifyFilters notifyFilter) {
        ArgumentNullException.ThrowIfNull(folderPath);

        Path = folderPath;
        Filter = filter ?? "*";
        IncludeSubdirectories = includeSubFolders;
        NotifyFilter = notifyFilter;
    }

    public bool EnableRaisingEvents { get; set; }

    /// <summary>Used to specify a single name pattern. for more than one filter use the Filters</summary>
    public string Filter {
        get => Filters.Count == 0 ? DefaultFilter : Filters[0];
        set {
            Filters.Clear();
            if (!string.IsNullOrEmpty(value)) {
                Filters.Add(value);
            }
        }
    }

    public Collection<string> Filters => _filters;

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
                //todo: maybe: if (NotifyFilter.HasFlag)
                Changed?.Invoke(this, args);
                break;
            case WatcherChangeTypes.Created:
                Created?.Invoke(this, args);
                break;
            case WatcherChangeTypes.Deleted:
                Deleted?.Invoke(this, args);
                break;
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

    private bool ShouldHandle(string name) {
        string filter = "*";
        //todo FileSystemName.MatchesSimpleExpression(filter, name);
        return false;
    }

    public const string DefaultFilter = "*";

}