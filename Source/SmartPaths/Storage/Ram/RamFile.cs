using System.Diagnostics;
using System.Text;

namespace SmartPaths.Storage.Ram;

[DebuggerDisplay("{Path.ToString()}")]
public class RamFile : IFile
{

    private readonly RamFileSystem _fileSystem;

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Path = path;
        Data = [];
        LastWrite = lastWrite;
        Parent = _fileSystem.GetFolder(path.Parent).Result!;
    }

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, string contents, DateTimeOffset lastWrite)
        : this(fileSystem, path, Encoding.UTF8.GetBytes(contents), lastWrite) { }

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite) {
        _fileSystem = fileSystem;
        Path = path;
        Data = data;
        LastWrite = lastWrite;
        Parent = _fileSystem.GetFolder(path.Parent).Result!;
    }

    public string Name => Path.FileName;

    public RamFolder Parent { get; }

    public AbsoluteFilePath Path { get; }

    internal byte[]? Data { get; set; }

    internal DateTimeOffset LastWrite { get; private set; }

    IFolder IFile.Parent => Parent;

    public Task Delete() {
        Data = null;
        LastWrite = DateTimeOffset.Now;
        Parent.ExpungeFile(this);
        return Task.CompletedTask;
    }

    public Task<bool> Exists() {
        return Task.FromResult(Data is not null);
    }

    public Task<DateTimeOffset> GetLastWriteTime() {
        return Task.FromResult(LastWrite);
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
        Parent.ExpungeFile(this);

        //Find the new parent folder, and register it there
        RamFolder newFolder = await _fileSystem.CreateFolder(newPath.Parent);
        RamFile newFile = new(_fileSystem, newPath, Data, DateTime.Now);
        newFolder.Register(newFile);
        return newFile;
    }

    public Task<Stream> OpenToAppend() {
        return Task.Run(() => {
                            Stream stream = new RamStream(this, true);
                            stream.Seek(0, SeekOrigin.End);
                            return stream;
                        });
    }

    public Task<Stream> OpenToRead() {
        return Task.Run(() => (Stream)new RamStream(this, false));
    }

    public Task<Stream> OpenToWrite() {
        return Task.Run<Stream>(() => new RamStream(this, true));
    }

    public Task Touch() {
        return Task.Run(() => LastWrite = DateTimeOffset.Now);
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

}