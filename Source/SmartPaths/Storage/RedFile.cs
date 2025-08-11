using System.Diagnostics;

namespace SmartPaths.Storage;

/// <summary>Represents a file within the RAM-based file system, providing functionality for file
///     manipulation, caching, and metadata management. This class is designed to handle in-memory file
///     operations and integrates with the <see cref="RedFileSystem" />.</summary>
/// <remarks>The <see cref="RedFile" /> class supports operations such as reading, writing, deleting,
///     and moving files. It also provides caching capabilities to optimize file access and tracks
///     metadata such as the last write time.</remarks>
[DebuggerDisplay("[RedFile:{IsCached}] {Path}")]
public class RedFile : IRamFile
{

    private readonly RedFileSystem _fileSystem;
    private byte[]? _data;
    private DateTimeOffset _lastWrite;

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        _data = null;
        DiskFile? onDisk = fileSystem.Disk.GetFile(path).Result;
        _lastWrite = onDisk is not null ? onDisk.GetLastWriteTime().Result : DateTimeOffset.MinValue;
    }

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        Data = data;
        IsCached = true;
        _lastWrite = lastWrite;
    }

    public RedFolder Folder { get; }

    public bool IsCached { get; private set; }

    public string Name => Path.FileName;

    public AbsoluteFilePath Path { get; }

    public bool WasDeleted { get; private set; }

    private byte[] Data {
        get {
            if (!IsCached) {
                CacheData().Wait();
            }
            return _data!;
        }
        set {
            _data = value;
            IsCached = true;
            _lastWrite = DateTimeOffset.Now;
        }
    }

    byte[]? IRamFile.Data {
        get => Data;
        set => Data = value ?? [];
    }

    IFolder IFile.Parent => Folder;

    public Task Delete() {
        return Delete(true);
    }

    public Task<bool> Exists() {
        return IsCached ? Task.FromResult(!WasDeleted) : _fileSystem.Disk.FileExists(Path);
    }

    public async Task<DateTimeOffset> GetLastWriteTime() {
        if (!IsCached) {
            DiskFile? onDisk = await _fileSystem.Disk.GetFile(Path);
            _lastWrite = onDisk is not null ? onDisk.GetLastWriteTime().Result : DateTimeOffset.MinValue;
        }
        return _lastWrite;
    }

    public async Task<RedFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(newPath);
        if (WasDeleted) {
            throw new Exception($"Cannot move a deleted file. {Path}");
        }
        if (!IsCached) {
            await CacheData();
        }
        RedFile? newFile = await _fileSystem.GetFile(newPath);
        if (newFile is not null) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newPath = await _fileSystem.MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    await newFile.Delete();
                    break;
                case CollisionStrategy.FailIfExists:
                    throw StorageExceptions.FileExists(newPath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
            }
        }

        //Find the new parent folder, and register it there
        RedFolder newParent = await _fileSystem.CreateFolder(newPath.Parent);
        newFile = new RedFile(_fileSystem, newPath, Data!, DateTime.Now);
        newParent.Register(newFile, false);

        //delete this record
        await Delete(false);

        _fileSystem.ProcessStorageEvent(new RenamedEventArgs(WatcherChangeTypes.Changed, Path.Folder, newPath, Path));

        return newFile;
    }

    public Task<Stream> OpenToAppend() {
        return Task.Run(() => {
                            Stream stream = new SmartStream<RedFile>(this, true);
                            stream.Seek(0, SeekOrigin.End);
                            return stream;
                        });
    }

    public Task<Stream> OpenToRead() {
        return Task.Run(Stream () => new SmartStream<RedFile>(this, false));
    }

    public Task<Stream> OpenToWrite() {
        return Task.Run<Stream>(() => new SmartStream<RedFile>(this, true));
    }

    public Task Touch() {
        return Task.Run(() => {
                            _lastWrite = DateTimeOffset.Now;
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
            _data = await onDisk.ReadAllBytes();
            _lastWrite = await onDisk.GetLastWriteTime();
        } else {
            _data = [];
            _lastWrite = DateTimeOffset.MinValue;
        }
        IsCached = true;
    }

    private Task Delete(bool notify) {
        WasDeleted = true;
        Data = [];
        Folder.Expunge(this, notify);
        return Task.CompletedTask;
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(IFile (task) => task.Result);
    }

}