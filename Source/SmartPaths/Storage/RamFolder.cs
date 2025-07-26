using System.Collections.Concurrent;
using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[Folder] {Path}")]
public sealed class RamFolder : BaseFolder<RamFolder, RamFile>
{

    private readonly ConcurrentDictionary<AbsoluteFilePath, RamFile> _files = [];
    private readonly RamFileSystem _fileSystem;
    private readonly ConcurrentDictionary<AbsoluteFolderPath, RamFolder> _folders = [];

    internal RamFolder(RamFileSystem fileSystem, AbsoluteFolderPath path)
        : base(path) {
        _fileSystem = fileSystem;
        Parent = path.IsRoot ? null : _fileSystem.GetFolder(path.Parent).Result ?? throw new Exception($"Can not find parent {path.Parent}.");
    }

    public override RamFolder? Parent { get; }

    public override Task Delete() {
        return Task.Run(() => DeleteFolderAndChildren(this));
    }

    public override Task DeleteFile(string fileName) {
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        if (_files.TryRemove(filePath, out _)) {
            _fileSystem.Files.TryRemove(filePath, out _);
        }
        return Task.CompletedTask;
    }

    public override Task DeleteFolder(string folderName) {
        AbsoluteFolderPath folderPath = Path.GetChildFolderPath(folderName);
        return _folders.TryGetValue(folderPath, out RamFolder? folder) ? DeleteFolderAndChildren(folder) : Task.CompletedTask;
    }

    public override Task<bool> Exists() {
        return Task.FromResult(true);
    }

    public override Task<RamFile?> GetFile(string fileName) {
        _files.TryGetValue(Path.GetChildFilePath(fileName), out RamFile? file);
        return Task.FromResult<RamFile?>(file);
    }

    public override Task<IReadOnlyList<RamFile>> GetFiles() {
        return Task.FromResult<IReadOnlyList<RamFile>>(_files.Values.ToList());
    }

    public override Task<RamFolder?> GetFolder(string folderName) {
        _folders.TryGetValue(Path.GetChildFolderPath(folderName), out RamFolder? folder);
        return Task.FromResult<RamFolder?>(folder);
    }

    public override Task<IReadOnlyList<RamFolder>> GetFolders() {
        return Task.FromResult<IReadOnlyList<RamFolder>>(_folders.Values.ToList());
    }

    internal override Task<RamFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
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
                file.ZeroOutContent();
                return Task.FromResult(file);

            case CollisionStrategy.FailIfExists:
                return Task.FromException<RamFile>(new Exception($"A file already exists at: {filePath}"));

            default:
                throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
        }
    }

    internal override Task<RamFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        RamFolder folder = _folders.GetOrAdd(folderPath,
                                             path => {
                                                 RamFolder newFolder = new(_fileSystem, path);
                                                 _fileSystem.Folders[newFolder.Path] = newFolder;
                                                 return newFolder;
                                             });
        return Task.FromResult(folder);
    }

    internal async Task<RamFolder> EnsureFolderTree(AbsoluteFolderPath folderPath) {
        //match path parts, then create missing.
        LinkedList<string> relative = PathHelper.MakeRelative(Path, folderPath);
        if (relative.First!.Next!.Value != ".") {
            throw new Exception($"The specified path ({folderPath}) is not a child path of ({Path}).");
        }
        RamFolder folder = this;
        foreach (string name in relative.Skip(2)) {
            folder = await folder.CreateFolder(name);
        }
        return folder;
    }

    internal void ExpungeFile(RamFile file) {
        _files.TryRemove(file.Path, out _);
        _fileSystem.Files.TryRemove(file.Path, out _);
    }

    internal void ExpungeFolder(RamFolder folder) {
        _folders.TryRemove(folder.Path, out _);
        _fileSystem.Folders.TryRemove(folder.Path, out _);
    }

    internal void Register(RamFile newFile) {
        _files[newFile.Path] = newFile;
        _fileSystem.Files[newFile.Path] = newFile;
    }

    private static async Task DeleteFolderAndChildren(RamFolder folder) {
        if (folder.Parent is null) {
            throw new Exception("Can not delete the root node");
        }
        //NB: use ToList() to copy the data as the collections will be changing under us
        //delete files on the way down
        foreach (RamFile file in folder._files.Values.ToList()) {
            folder.ExpungeFile(file);
        }
        //recurse
        foreach (RamFolder child in folder._folders.Values.ToList()) {
            await DeleteFolderAndChildren(child);
        }
        //delete folders on the way back up
        folder.Parent.ExpungeFolder(folder);
    }

}