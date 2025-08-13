using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[RamFile] {Path}")]
public sealed class RamFile : SmartFile<RamFolder, RamFile>
{

    private readonly RamFileSystem _fileSystem;

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path)
        : base(path) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
    }

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite)
        : base(path, data, lastWrite) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
    }

    public override Task<bool> Exists() {
        return Task.FromResult(!WasDeleted);
    }

    public override Task<DateTimeOffset> GetLastWriteTime() {
        return Task.FromResult(LastWrite);
    }

    public override async Task<RamFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(newPath);
        if (WasDeleted) {
            throw new Exception($"Cannot move a deleted file. {Path}");
        }
        if (Data is null) {
            throw StorageExceptions.FileMissing(Path);
        }
        RamFile? newFile = await _fileSystem.GetFile(newPath);
        if (newFile is not null) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newFile = null;
                    newPath = _fileSystem.MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    newFile.WasDeleted = false;
                    newFile.Data = Data;
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
            RamFolder newFolder = await _fileSystem.CreateFolder(newPath.Parent);
            newFile = new RamFile(_fileSystem, newPath, Data, DateTime.Now);
            newFolder.Register(newFile, false);
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

}