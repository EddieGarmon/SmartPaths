using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[RamFile] {Path}")]
public sealed class RamFile : SmartFile<RamFolder, RamFile>
{

    private readonly RamFileSystem _fileSystem;
    private byte[] _data = [];

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path)
        : base(path) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
    }

    internal RamFile(RamFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite)
        : this(fileSystem, path) {
        ArgumentNullException.ThrowIfNull(data);
        SetData(data);
        LastWrite = lastWrite;
    }

    public override RamFolder Folder { get; }

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
        RamFile? newFile = await _fileSystem.GetFile(newPath);
        if (newFile is not null) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newFile = null;
                    newPath = _fileSystem.MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    newFile.WasDeleted = false;
                    newFile.SetData(GetData());
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
            RamFolder newParent = await _fileSystem.CreateFolder(newPath.Parent);
            newFile = new RamFile(_fileSystem, newPath, GetData(), DateTimeOffset.Now);
            newParent.Register(newFile, false);
        }

        //delete this record
        await Delete(false);

        _fileSystem.ProcessStorageEvent(new FileEventRecord(LedgerAction.FileMoved, newPath, Path));

        return newFile;
    }

    public override Task Touch() {
        return Task.Run(() => {
                            LastWrite = DateTimeOffset.Now;
                            _fileSystem.ProcessStorageEvent(new FileEventRecord(LedgerAction.FileEdited, Path, Path));
                        });
    }

    internal override byte[] GetData() {
        return _data;
    }

    internal void ReCreateEmpty() {
        WasDeleted = false;
        _data = [];
        _fileSystem.ProcessStorageEvent(new FileEventRecord(LedgerAction.FileCreated, Path, null));
    }

    internal override void SetData(byte[] value) {
        ArgumentNullException.ThrowIfNull(value);
        _data = value;
        LastWrite = DateTimeOffset.Now;
    }

}