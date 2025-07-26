using System.Collections.Concurrent;
using System.Diagnostics;

namespace SmartPaths.Storage;

/// <summary>Represents a folder within the RAM-based file system, providing functionality for managing
///     files and subfolders.</summary>
/// <remarks>This class is designed to work with the <see cref="RedFileSystem" /> and provides caching
///     mechanisms, support for deletion, and methods for creating, retrieving, and managing files and
///     folders.</remarks>
[DebuggerDisplay("[XFolder:{IsCached}] {Path}")]
public class RedFolder : BaseFolder<RedFolder, RedFile>
{

    private readonly ConcurrentDictionary<AbsoluteFilePath, RedFile> _files = [];
    private readonly RedFileSystem _fileSystem;
    private readonly ConcurrentDictionary<AbsoluteFolderPath, RedFolder> _folders = [];

    internal RedFolder(RedFileSystem fileSystem, AbsoluteFolderPath path)
        : base(path) {
        _fileSystem = fileSystem;
        Parent = path.IsRoot ? null : _fileSystem.GetFolder(path.Parent).Result ?? throw new Exception($"Can not find parent {path.Parent}.");
    }

    public bool IsCached { get; private set; }

    public override RedFolder? Parent { get; }

    public bool WasDeleted { get; private set; }

    public override async Task Delete() {
        if (WasDeleted) {
            return;
        }
        //todo: should we always build out cache?
        //await BuildCacheInfo();
        foreach (RedFile file in _files.Values) {
            await file.Delete();
        }
        foreach (RedFolder folder in _folders.Values) {
            await folder.Delete();
        }
        WasDeleted = true;
    }

    public override Task<bool> Exists() {
        return Task.FromResult(!WasDeleted);
    }

    public override async Task<IReadOnlyList<RedFile>> GetFiles() {
        await BuildCacheInfo();
        return _files.Values.ToList();
    }

    public override async Task<IReadOnlyList<RedFolder>> GetFolders() {
        await BuildCacheInfo();
        return _folders.Values.ToList();
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
            RedFile file = new(_fileSystem, fileOnDisk.Path);
            Register(file);
        }
        //Cache Folders
        IEnumerable<IFolder> folders = await _fileSystem.Disk.GetFolders(Path);
        foreach (IFolder folderOnDisk in folders) {
            if (_folders.ContainsKey(folderOnDisk.Path)) {
                continue;
            }
            RedFolder folder = new(_fileSystem, folderOnDisk.Path);
            Register(folder);
        }
        //Mark as cached
        IsCached = true;
    }

    internal override async Task<RedFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
        if (filePath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew File: {filePath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        //Check the cache
        if (!_files.TryGetValue(filePath, out RedFile? file)) {
            file = new RedFile(_fileSystem, filePath, [], DateTimeOffset.Now);
            Register(file);
            return file;
        }
        if (file.WasDeleted) {
            file.Restore();
            return file;
        }
        switch (collisionStrategy) {
            case CollisionStrategy.GenerateUniqueName:
                filePath = await _fileSystem.MakeUnique(filePath);
                file = new RedFile(_fileSystem, filePath, [], DateTimeOffset.Now);
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

    internal override async Task<RedFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        if (folderPath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew Folder: {folderPath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        //Check the cache
        if (_folders.TryGetValue(folderPath, out RedFolder? folder)) {
            if (folder.WasDeleted) {
                folder.WasDeleted = false;
            }
            return folder;
        }
        //Build a new folder
        RedFolder newFolder = new(_fileSystem, folderPath);
        Register(newFolder);
        return newFolder;
    }

    internal override async Task<RedFile?> GetFile(AbsoluteFilePath filePath) {
        if (filePath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew File: {filePath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        _files.TryGetValue(filePath, out RedFile? file);
        return file;
    }

    internal override async Task<RedFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        if (folderPath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nGet Folder: {folderPath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        _folders.TryGetValue(folderPath, out RedFolder? folder);
        return folder;
    }

    private void Register(RedFolder newFolder) {
        _folders[newFolder.Path] = newFolder;
        _fileSystem.Folders[newFolder.Path] = newFolder;
    }

    private void Register(RedFile newFile) {
        _files[newFile.Path] = newFile;
        _fileSystem.Files[newFile.Path] = newFile;
    }

}