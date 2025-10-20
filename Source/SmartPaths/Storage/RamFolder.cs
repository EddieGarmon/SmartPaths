using System.Collections.Concurrent;
using System.Diagnostics;

namespace SmartPaths.Storage;

/// <summary>Represents a folder within the RAM-based file system, providing functionality for managing
///     files and subfolders.</summary>
[DebuggerDisplay("[Folder] {Path}")]
public sealed class RamFolder : SmartFolder<RamFolder, RamFile>
{

    private readonly ConcurrentDictionary<AbsoluteFilePath, RamFile> _files = [];
    private readonly RamFileSystem _fileSystem;
    private readonly ConcurrentDictionary<AbsoluteFolderPath, RamFolder> _folders = [];

    internal RamFolder(RamFileSystem fileSystem, AbsoluteFolderPath path)
        : base(path) {
        _fileSystem = fileSystem;
        if (IsRoot) {
            Parent = null;
        } else {
            Parent = _fileSystem.GetFolder(path.Parent).Result ?? throw new Exception($"Can not find parent {path.Parent}.");
            Parent._folders[path] = this;
        }
    }

    public override async Task Delete() {
        if (Parent is null) {
            throw new Exception("Can not delete the root node");
        }
        //NB: use ToList() to copy the data as the collections will be changing under us
        foreach (RamFile file in _files.Values.ToList()) {
            //delete files on the way down
            await file.Delete();
        }
        foreach (RamFolder child in _folders.Values.ToList()) {
            await child.Delete();
        }
        //delete folders on the way back up
        Parent.Expunge(this);
    }

    public override Task<bool> Exists() {
        return Task.FromResult(true);
    }

    public override Task<IReadOnlyList<RamFile>> GetFiles() {
        return Task.FromResult<IReadOnlyList<RamFile>>(_files.Values.ToList());
    }

    public override Task<IReadOnlyList<RamFolder>> GetFolders() {
        return Task.FromResult<IReadOnlyList<RamFolder>>(_folders.Values.ToList());
    }

    public override Task<IFileSystemWatcher> GetWatcher(string filter = "*", bool includeSubFolders = false) {
        return _fileSystem.GetWatcher(Path, filter, includeSubFolders).ContinueWith(IFileSystemWatcher (task) => task.Result);
    }

    internal override Task<RamFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
        if (filePath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew File: {filePath}");
        }
        if (!_files.TryGetValue(filePath, out RamFile? file)) {
            file = new RamFile(_fileSystem, filePath, [], DateTimeOffset.Now);
            Register(file);
            return Task.FromResult(file);
        }
        switch (collisionStrategy) {
            case CollisionStrategy.GenerateUniqueName:
                filePath = _fileSystem.MakeUnique(filePath);
                file = new RamFile(_fileSystem, filePath, [], DateTimeOffset.Now);
                Register(file);
                return Task.FromResult(file);

            case CollisionStrategy.ReplaceExisting:
                file.ReCreateEmpty();
                return Task.FromResult(file);

            case CollisionStrategy.FailIfExists:
                return Task.FromException<RamFile>(new Exception($"A file already exists at: {filePath}"));

            default:
                throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
        }
    }

    internal override Task<RamFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        if (folderPath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew Folder: {folderPath}");
        }
        RamFolder folder = _folders.GetOrAdd(folderPath,
                                             path => {
                                                 RamFolder newFolder = new(_fileSystem, path);
                                                 _fileSystem.Folders[newFolder.Path] = newFolder;
                                                 _fileSystem.ProcessStorageEvent(new FolderEventRecord(LedgerAction.FolderCreated, newFolder.Path, null));
                                                 return newFolder;
                                             });
        return Task.FromResult(folder);
    }

    internal async Task<RamFolder> EnsureFolderTree(AbsoluteFolderPath folderPath) {
        //match path parts, then create missing.
        PathCore core = Path.Core.ComputeRelative(false, folderPath.Core);
        if (core[1] != ".") {
            throw new Exception($"The specified path ({folderPath}) is not a child path of ({Path}).");
        }

        RamFolder folder = this;
        foreach (string name in core.Parts.Skip(2)) {
            folder = await folder.CreateFolder(name);
        }
        return folder;
    }

    internal override void Expunge(SmartFile<RamFolder, RamFile> file, bool notify) {
        _files.TryRemove(file.Path, out _);
        _fileSystem.Files.TryRemove(file.Path, out _);
        if (notify) {
            _fileSystem.ProcessStorageEvent(new FileEventRecord(LedgerAction.FileDeleted, null, file.Path));
        }
    }

    internal override Task<RamFile?> GetFile(AbsoluteFilePath filePath) {
        _files.TryGetValue(filePath, out RamFile? file);
        return Task.FromResult(file);
    }

    internal override Task<RamFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        _folders.TryGetValue(folderPath, out RamFolder? folder);
        return Task.FromResult(folder);
    }

    internal void Register(RamFile newFile, bool notify = true) {
        _files[newFile.Path] = newFile;
        _fileSystem.Files[newFile.Path] = newFile;
        if (notify) {
            _fileSystem.ProcessStorageEvent(new FileEventRecord(LedgerAction.FileCreated, newFile.Path, null));
        }
    }

    private void Expunge(RamFolder folder) {
        _folders.TryRemove(folder.Path, out _);
        _fileSystem.Folders.TryRemove(folder.Path, out _);
        _fileSystem.ProcessStorageEvent(new FolderEventRecord(LedgerAction.FolderDeleted, null, folder.Path));
    }

}