using System.Collections.Concurrent;
using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[XFolder:{IsCached}] {Path}")]
public class RamEditFolder : BaseFolder<RamEditFolder, RamEditFile>
{

    private readonly ConcurrentDictionary<AbsoluteFilePath, RamEditFile> _files = [];
    private readonly RamEditFileSystem _fileSystem;
    private readonly ConcurrentDictionary<AbsoluteFolderPath, RamEditFolder> _folders = [];

    internal RamEditFolder(RamEditFileSystem fileSystem, AbsoluteFolderPath path)
        : base(path) {
        _fileSystem = fileSystem;
        Parent = path.IsRoot ? null : _fileSystem.GetFolder(path.Parent).Result ?? throw new Exception($"Can not find parent {path.Parent}.");
    }

    public bool IsCached { get; private set; }

    public override RamEditFolder? Parent { get; }

    public bool WasDeleted { get; private set; }

    public override Task Delete() {
        WasDeleted = true;
        throw new NotImplementedException();
    }

    public override Task DeleteFile(string fileName) {
        throw new NotImplementedException();
    }

    public override Task DeleteFolder(string folderName) {
        throw new NotImplementedException();
    }

    public override Task<bool> Exists() {
        return Task.FromResult(!WasDeleted);
    }

    public override Task<RamEditFile?> GetFile(string filename) {
        throw new NotImplementedException("RamEditFolder.GetFile");
    }

    public override Task<IReadOnlyList<RamEditFile>> GetFiles() {
        throw new NotImplementedException();
    }

    public override Task<RamEditFolder?> GetFolder(string folderName) {
        throw new NotImplementedException("RamEditFolder.GetFolder");
    }

    public override Task<IReadOnlyList<RamEditFolder>> GetFolders() {
        throw new NotImplementedException("RamEditFolder.GetFolders");
    }

    internal async Task BuildCacheInfo() {
        if (IsCached) {
            return;
        }
        //Cache Files
        IEnumerable<IFile> files = await _fileSystem.Disk.GetFiles(Path);
        foreach (IFile fileOnDisk in files) {
            if (_files.ContainsKey(fileOnDisk.Path)) {
                continue;
            }
            RamEditFile file = new(_fileSystem, fileOnDisk.Path);
            Register(file);
        }
        //Cache Folders
        IEnumerable<IFolder> folders = await _fileSystem.Disk.GetFolders(Path);
        foreach (IFolder folderOnDisk in folders) {
            if (_folders.ContainsKey(folderOnDisk.Path)) {
                continue;
            }
            RamEditFolder folder = new(_fileSystem, folderOnDisk.Path);
            Register(folder);
        }
        //Mark as cached
        IsCached = true;
    }

    internal override async Task<RamEditFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
        if (filePath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew File: {filePath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        //Check the cache
        if (!_files.TryGetValue(filePath, out RamEditFile? file)) {
            file = new RamEditFile(_fileSystem, filePath, [], DateTimeOffset.Now);
            Register(file);
            return file;
        }
        switch (collisionStrategy) {
            case CollisionStrategy.GenerateUniqueName:
                filePath = await _fileSystem.MakeUnique(filePath);
                file = new RamEditFile(_fileSystem, filePath, [], DateTimeOffset.Now);
                Register(file);
                return file;

            case CollisionStrategy.ReplaceExisting:
                file.ZeroOutContent();
                return file;

            case CollisionStrategy.FailIfExists:
                throw new Exception($"A file already exists at: {filePath}");

            default:
                throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
        }
    }

    internal override async Task<RamEditFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        if (folderPath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew Folder: {folderPath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        //Check the cache
        if (_folders.TryGetValue(folderPath, out RamEditFolder? folder)) {
            return folder;
        }
        //Build a new folder
        RamEditFolder newFolder = new(_fileSystem, folderPath);
        Register(newFolder);
        return newFolder;
    }

    private void Register(RamEditFolder newFolder) {
        _folders[newFolder.Path] = newFolder;
        _fileSystem.Folders[newFolder.Path] = newFolder;
    }

    private void Register(RamEditFile newFile) {
        _files[newFile.Path] = newFile;
        _fileSystem.Files[newFile.Path] = newFile;
    }

}