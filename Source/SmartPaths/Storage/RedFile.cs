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
    private byte[]? _data;

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path)
        : base(path) {
        _fileSystem = fileSystem;
        Folder = _fileSystem.GetFolder(path.Parent).Result!;
    }

    internal RedFile(RedFileSystem fileSystem, AbsoluteFilePath path, byte[] data, DateTimeOffset lastWrite)
        : this(fileSystem, path) {
        ArgumentNullException.ThrowIfNull(data);
        SetData(data);
        LastWrite = lastWrite;
    }

    public override RedFolder Folder { get; }

    public override Task<bool> Exists() {
        return _data is not null ? Task.FromResult(!WasDeleted) : _fileSystem.Disk.FileExists(Path);
    }

    public override async Task<DateTimeOffset> GetLastWriteTime() {
        if (_data is null) {
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
        RedFile? newFile = await _fileSystem.GetFile(newPath);
        if (newFile is not null) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newFile = null;
                    newPath = await _fileSystem.MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    newFile.WasDeleted = false;
                    newFile.LastWrite = DateTimeOffset.Now;
                    newFile.SetData(GetData());
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
            newFile = new RedFile(_fileSystem, newPath, GetData(), DateTimeOffset.Now);
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
        if (_data is not null) {
            return _data;
        }

        DiskFile? onDisk = _fileSystem.Disk.GetFile(Path).Result;
        if (onDisk is not null) {
            _data = onDisk.ReadAllBytes().Result;
            LastWrite = onDisk.GetLastWriteTime().Result;
        } else {
            throw StorageExceptions.FileMissing(Path);
        }

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