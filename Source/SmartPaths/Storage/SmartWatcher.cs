using System.Collections.ObjectModel;

namespace SmartPaths.Storage;

public class SmartWatcher : IFileSystemWatcher
{

    private readonly FilterCollection _filters = [];

    internal SmartWatcher(AbsoluteFolderPath path, string? filter, bool includeSubdirectories, NotifyFilters notifyFilter) {
        ArgumentNullException.ThrowIfNull(path);

        Path = path;
        Filter = filter ?? DefaultFilter;
        IncludeSubdirectories = includeSubdirectories;
        NotifyFilter = notifyFilter;
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
        IPath path = SmartPath.Parse(args.FullPath);
        AbsoluteFolderPath parent;
        string name;
        switch (path) {
            case AbsoluteFolderPath folderPath:
                parent = folderPath.Parent;
                name = folderPath.FolderName;
                break;

            case AbsoluteFilePath filePath:
                parent = filePath.Folder;
                name = filePath.FileName;
                break;

            case RelativeFolderPath:
            case RelativeFilePath:
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(path));
        }
        //need to check if path is a direct child of watch path
        if (parent != Path) {
            //also check subdirectories
            if (!IncludeSubdirectories || !path.ToString()!.StartsWith(Path.ToString())) {
                return;
            }
        }
        //paths match, now check Filters
        if (!_filters.IsMatch(name)) {
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
        }
    }

    internal void ProcessStorageEvent(RenamedEventArgs args) {
        if (!EnableRaisingEvents) {
            return;
        }
        //todo: finish
        if (_filters.IsMatch(args.FullPath) || _filters.IsMatch(args.OldFullPath)) {
            Renamed?.Invoke(this, args);
        }
    }

    public const string DefaultFilter = "*";

}