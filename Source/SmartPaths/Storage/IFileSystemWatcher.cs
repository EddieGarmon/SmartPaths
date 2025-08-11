namespace SmartPaths.Storage;

public interface IFileSystemWatcher
{

    bool EnableRaisingEvents { get; set; }

    string Filter { get; set; }

    bool IncludeSubdirectories { get; set; }

    NotifyFilters NotifyFilter { get; set; }

    AbsoluteFolderPath Path { get; set; }

    event FileSystemEventHandler? Changed;

    event FileSystemEventHandler? Created;

    event FileSystemEventHandler? Deleted;

    event ErrorEventHandler? Error;

    event RenamedEventHandler? Renamed;

}