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

    private readonly RedFileSystem _fileSystem;

    internal RedFolder(RedFileSystem fileSystem, AbsoluteFolderPath path)
        : base(path) {
        _fileSystem = fileSystem;
        if (path.IsRoot) {
            Parent = null;
        } else {
            RedFolder? result = _fileSystem.GetFolder(path.Parent).Result;
            if (result != null) {
                result.Folders[path] = this;
                Parent = result;
            } else {
                throw new Exception($"Can not find parent {path.Parent}.");
            }
        }
    }

    public bool IsCached { get; private set; }

    public override RedFolder? Parent { get; }

    public bool WasDeleted { get; private set; }

    internal ConcurrentDictionary<AbsoluteFilePath, RedFile> Files { get; } = [];

    internal ConcurrentDictionary<AbsoluteFolderPath, RedFolder> Folders { get; } = [];

    public override async Task Delete() {
        if (WasDeleted) {
            return;
        }
        //todo: should we ever build out cache now?
        //await BuildCacheInfo();
        foreach (RedFile file in Files.Values) {
            await file.Delete();
        }
        foreach (RedFolder folder in Folders.Values) {
            await folder.Delete();
        }
        WasDeleted = true;
    }

    public override Task<bool> Exists() {
        return Task.FromResult(!WasDeleted);
    }

    public override async Task<IReadOnlyList<RedFile>> GetFiles() {
        await BuildCacheInfo();
        return Files.Values.Where(file => !file.WasDeleted).ToList();
    }

    public override async Task<IReadOnlyList<RedFolder>> GetFolders() {
        await BuildCacheInfo();
        return Folders.Values.Where(folder => !folder.WasDeleted).ToList();
    }

    internal async Task BuildCacheInfo() {
        if (IsCached) {
            return;
        }
        //Cache Files
        IEnumerable<IFile> files = await _fileSystem.Disk.GetFiles(Path);
        foreach (IFile fileOnDisk in files) {
            if (Files.ContainsKey(fileOnDisk.Path)) {
                continue;
            }
            RedFile file = new(_fileSystem, fileOnDisk.Path);
            Files[file.Path] = file;
            _fileSystem.Files[file.Path] = file;
        }
        //Cache Folders
        IEnumerable<IFolder> folders = await _fileSystem.Disk.GetFolders(Path);
        foreach (IFolder folderOnDisk in folders) {
            if (Folders.ContainsKey(folderOnDisk.Path)) {
                continue;
            }
            RedFolder folder = new(_fileSystem, folderOnDisk.Path);
            Folders[folder.Path] = folder;
            _fileSystem.Folders[folder.Path] = folder;
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
        if (!Files.TryGetValue(filePath, out RedFile? file)) {
            file = new RedFile(_fileSystem, filePath, [], DateTimeOffset.Now);
            Files[file.Path] = file;
            _fileSystem.Files[file.Path] = file;
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
                Files[file.Path] = file;
                _fileSystem.Files[file.Path] = file;
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
        if (Folders.TryGetValue(folderPath, out RedFolder? folder)) {
            if (folder.WasDeleted) {
                folder.WasDeleted = false;
            }
            return folder;
        }
        //Build a new folder
        RedFolder newFolder = new(_fileSystem, folderPath);
        Folders[newFolder.Path] = newFolder;
        _fileSystem.Folders[newFolder.Path] = newFolder;
        return newFolder;
    }

    internal override async Task<RedFile?> GetFile(AbsoluteFilePath filePath) {
        if (filePath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew File: {filePath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        Files.TryGetValue(filePath, out RedFile? file);
        return file;
    }

    internal override async Task<RedFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        if (folderPath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nGet Folder: {folderPath}");
        }
        if (!IsCached) {
            await BuildCacheInfo();
        }
        Folders.TryGetValue(folderPath, out RedFolder? folder);
        return folder;
    }

}