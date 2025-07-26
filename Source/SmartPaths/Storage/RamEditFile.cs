using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[XFile:{IsCached}] {Path}")]
public class RamEditFile : IRamFile
{

    private readonly RamEditFileSystem _fileSystem;
    private byte[]? _data;
    private DateTimeOffset _lastWrite;

    internal RamEditFile(RamEditFileSystem fileSystem, AbsoluteFilePath path) {
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

    internal RamEditFile(RamEditFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Parent = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        Data = data;
        IsCached = true;
        _lastWrite = lastWrite;
    }

    public bool IsCached { get; private set; }

    public string Name => Path.FileName;

    public RamEditFolder Parent { get; }

    public AbsoluteFilePath Path { get; }

    public bool WasDeleted { get; private set; }

    internal byte[]? Data {
        get {
            if (IsCached) {
                return _data;
            }
            _data = File.ReadAllBytes(Path);
            IsCached = true;
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

    public Task<RamEditFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenToAppend() {
        return Task.Run(() => {
                            Stream stream = new RamStream<RamEditFile>(this, true);
                            stream.Seek(0, SeekOrigin.End);
                            return stream;
                        });
    }

    public Task<Stream> OpenToRead() {
        return Task.Run(Stream () => new RamStream<RamEditFile>(this, false));
    }

    public Task<Stream> OpenToWrite() {
        return Task.Run<Stream>(() => new RamStream<RamEditFile>(this, true));
    }

    public Task Touch() {
        return Task.Run(() => _lastWrite = DateTimeOffset.Now);
    }

    internal void ZeroOutContent() {
        using RamStream<RamEditFile> stream = new(this, true);
        stream.SetLength(0);
        _lastWrite = DateTimeOffset.Now;
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(IFile (task) => task.Result);
    }

}