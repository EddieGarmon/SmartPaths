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
        Parent = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        Data = null;
        if (File.Exists(path)) {
            _lastWrite = File.GetLastWriteTime(path);
        } else {
            _lastWrite = DateTimeOffset.MinValue;
        }
    }

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Parent = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        Data = data;
        IsCached = true;
        _lastWrite = lastWrite;
    }

    public bool IsCached { get; private set; }

    public string Name => Path.FileName;

    public RedFolder Parent { get; }

    public AbsoluteFilePath Path { get; }

    public bool WasDeleted { get; private set; }

    internal byte[]? Data {
        get {
            if (!IsCached) {
                CacheData();
            }
            return _data;
        }
        set {
            _data = value;
            IsCached = true;
            _lastWrite = DateTimeOffset.Now;
        }
    }

    byte[]? IRamFile.Data {
        get => Data;
        set => Data = value;
    }

    IFolder IFile.Parent => Parent;

    public Task Delete() {
        WasDeleted = true;
        IsCached = true;
        Data = null;
        _lastWrite = DateTimeOffset.Now;
        return Task.CompletedTask;
    }

    public Task<bool> Exists() {
        return IsCached ? Task.FromResult(!WasDeleted) : Task.FromResult(File.Exists(Path));
    }

    public Task<DateTimeOffset> GetLastWriteTime() {
        if (_lastWrite == DateTimeOffset.MinValue) {
            _lastWrite = File.GetLastWriteTime(Path);
        }
        return Task.FromResult(_lastWrite);
    }

    public async Task<RedFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        if (WasDeleted) {
            throw new Exception($"Cannot move a deleted file. {Path}");
        }
        if (!IsCached) {
            CacheData();
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
        Parent.Files.TryRemove(Path, out RedFile? _);
        _fileSystem.Files.TryRemove(Path, out RedFile? _);

        //Find the new parent folder, and register it there
        RedFolder newParent = await _fileSystem.CreateFolder(newPath.Parent);
        newFile = new RedFile(_fileSystem, newPath, Data!, DateTime.Now);
        newParent.Files[newFile.Path] = newFile;
        _fileSystem.Files[newFile.Path] = newFile;
        return newFile;
    }

    public Task<Stream> OpenToAppend() {
        return Task.Run(() => {
                            Stream stream = new RamStream<RedFile>(this, true);
                            stream.Seek(0, SeekOrigin.End);
                            return stream;
                        });
    }

    public Task<Stream> OpenToRead() {
        return Task.Run(Stream () => new RamStream<RedFile>(this, false));
    }

    public Task<Stream> OpenToWrite() {
        return Task.Run<Stream>(() => new RamStream<RedFile>(this, true));
    }

    public Task Touch() {
        return Task.Run(() => _lastWrite = DateTimeOffset.Now);
    }

    internal void Restore() {
        WasDeleted = false;
        _lastWrite = DateTimeOffset.Now;
    }

    internal void ZeroOutContent() {
        using RamStream<RedFile> stream = new(this, true);
        stream.SetLength(0);
        _lastWrite = DateTimeOffset.Now;
    }

    private void CacheData() {
        _data = File.ReadAllBytes(Path);
        _lastWrite = File.GetLastWriteTime(Path);
        IsCached = true;
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(IFile (task) => task.Result);
    }

}