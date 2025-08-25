namespace SmartPaths.Storage;

public interface IFileSystemWatcher
{

    bool EnableRaisingEvents { get; set; }

    string Filter { get; set; }

    bool IncludeSubdirectories { get; set; }

    AbsoluteFolderPath Path { get; set; }

    event Action<FileEventRecord>? FileCreated;

    event Action<FileEventRecord>? FileDeleted;

    event Action<FileEventRecord>? FileEdited;

    event Action<FileEventRecord>? FileMoved;

    event Action<FolderEventRecord>? FolderCreated;

    event Action<FolderEventRecord>? FolderDeleted;

    event Action<FolderEventRecord>? FolderMoved;

}