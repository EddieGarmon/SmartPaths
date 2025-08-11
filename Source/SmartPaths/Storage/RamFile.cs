using System.Diagnostics;
using System.Text;

namespace SmartPaths.Storage;

[DebuggerDisplay("[File] {Path}")]
public sealed class RamFile : IRamFile
{

    private readonly RamFileSystem _fileSystem;
    private byte[] _data;
    private DateTimeOffset _lastWrite;

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        _data = [];
        _lastWrite = lastWrite;
    }

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, string contents, DateTimeOffset lastWrite)
        : this(fileSystem, path, Encoding.UTF8.GetBytes(contents), lastWrite) { }

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
        Path = path;
        _data = data;
        _lastWrite = lastWrite;
    }

    public RamFolder Folder { get; }

    public string Name => Path.FileName;

    public AbsoluteFilePath Path { get; }

    public bool WasDeleted { get; private set; }

    private byte[] Data {
        get => _data;
        set {
            _data = value;
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
        return Task.FromResult(!WasDeleted);
    }

    public Task<DateTimeOffset> GetLastWriteTime() {
        return Task.FromResult(_lastWrite);
    }

    public async Task<RamFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(newPath);
        if (Data is null) {
            throw StorageExceptions.FileMissing(Path);
        }
        if (await _fileSystem.GetFile(newPath) is not null) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newPath = _fileSystem.MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    await _fileSystem.DeleteFile(newPath);
                    break;
                case CollisionStrategy.FailIfExists:
                    throw StorageExceptions.FileExists(newPath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
            }
        }

        //Find the new parent folder, and register it there
        RamFolder newFolder = await _fileSystem.CreateFolder(newPath.Parent);
        RamFile newFile = new(_fileSystem, newPath, Data, DateTime.Now);
        newFolder.Register(newFile, false);

        //delete this record
        await Delete(false);

        _fileSystem.ProcessStorageEvent(new RenamedEventArgs(WatcherChangeTypes.Changed, Path.Folder, newPath, Path));

        return newFile;
    }

    public Task<Stream> OpenToAppend() {
        return Task.Run(() => {
                            Stream stream = new SmartStream<RamFile>(this, true);
                            stream.Seek(0, SeekOrigin.End);
                            return stream;
                        });
    }

    public Task<Stream> OpenToRead() {
        return Task.Run(Stream () => new SmartStream<RamFile>(this, false));
    }

    public Task<Stream> OpenToWrite() {
        return Task.Run<Stream>(() => new SmartStream<RamFile>(this, true));
    }

    public Task Touch() {
        return Task.Run(() => {
                            _lastWrite = DateTimeOffset.Now;
                            _fileSystem.ProcessStorageEvent(new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.Folder, Path.FileName));
                        });
    }

    internal void ReCreateEmpty() {
        Data = [];
        _fileSystem.ProcessStorageEvent(new FileSystemEventArgs(WatcherChangeTypes.Created, Path.Folder, Path.FileName));
    }

    private Task Delete(bool notify) {
        WasDeleted = true;
        Data = [];
        Folder.Expunge(this, notify);
        return Task.CompletedTask;
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

}