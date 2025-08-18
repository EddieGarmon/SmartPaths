using System.Diagnostics;

namespace SmartPaths.Storage;

/// <summary>Represents a file within the RAM-based file system, providing functionality for file
///     manipulation, caching, and metadata management. This class is designed to handle in-memory file
///     operations and integrates with the <see cref="RedFileSystem" />.</summary>
/// <remarks>The <see cref="RedFile" /> class supports operations such as reading, writing, deleting,
///     and moving files. It also provides caching capabilities to optimize file access and tracks
///     metadata such as the last write time.</remarks>
[DebuggerDisplay("[XFile:{IsCached}] {Path}")]
public sealed class RedFile : SmartFile<RedFolder, RedFile>
{

    private readonly RedFileSystem _fileSystem;

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path)
        : base(path) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;

        //don't do this until necessary
        DiskFile? onDisk = fileSystem.Disk.GetFile(path).Result;
        LastWrite = onDisk is not null ? onDisk.GetLastWriteTime().Result : DateTimeOffset.MinValue;
    }

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite)
        : base(path, data, lastWrite) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
    }

    public override RedFolder Folder { get; }

    public bool IsCached => Data is not null;

    private byte[] CachedData {
        get {
            if (!IsCached) {
                CacheData().Wait();
            }
            return Data!;
        }
        set {
            ArgumentNullException.ThrowIfNull(value);
            Data = value;
            LastWrite = DateTimeOffset.Now;
        }
    }

    public override Task<bool> Exists() {
        return IsCached ? Task.FromResult(!WasDeleted) : _fileSystem.Disk.FileExists(Path);
    }

    public override async Task<DateTimeOffset> GetLastWriteTime() {
        if (!IsCached) {
            DiskFile? onDisk = await _fileSystem.Disk.GetFile(Path);
            LastWrite = onDisk is not null ? onDisk.GetLastWriteTime().Result : DateTimeOffset.MinValue;
        }
        return LastWrite;
    }

    public override async Task<RedFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(newPath);
        if (WasDeleted) {
            throw new Exception($"Cannot move a deleted file. {Path}");
        }
        if (!IsCached) {
            await CacheData();
        }
        if (Data is null) {
            throw StorageExceptions.FileMissing(Path);
        }
        RedFile? newFile = await _fileSystem.GetFile(newPath);
        if (newFile is not null) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newFile = null;
                    newPath = await _fileSystem.MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    newFile.WasDeleted = false;
                    newFile.CachedData = CachedData;
                    newFile.LastWrite = DateTimeOffset.Now;
                    break;
                case CollisionStrategy.FailIfExists:
                    throw StorageExceptions.FileExists(newPath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
            }
        }

        if (newFile is null) {
            //Find the new parent folder, and register it there
            RedFolder newParent = await _fileSystem.CreateFolder(newPath.Parent);
            newFile = new RedFile(_fileSystem, newPath, CachedData, DateTime.Now);
            newParent.Register(newFile, false);
        }

        //delete this record
        await Delete(false);

        _fileSystem.ProcessStorageEvent(new RenamedEventArgs(WatcherChangeTypes.Changed, Path.Folder, newPath, Path));

        return newFile;
    }

    public override Task Touch() {
        return Task.Run(() => {
                            LastWrite = DateTimeOffset.Now;
                            _fileSystem.ProcessStorageEvent(new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.Folder, Path.FileName));
                        });
    }

    internal void ReCreateEmpty() {
        WasDeleted = false;
        Data = [];
        _fileSystem.ProcessStorageEvent(new FileSystemEventArgs(WatcherChangeTypes.Created, Path.Folder, Path.FileName));
    }

    private async Task CacheData() {
        DiskFile? onDisk = await _fileSystem.Disk.GetFile(Path);
        if (onDisk is not null) {
            Data = await onDisk.ReadAllBytes();
            LastWrite = await onDisk.GetLastWriteTime();
        } else {
            Data = [];
            LastWrite = DateTimeOffset.MinValue;
        }
    }

}