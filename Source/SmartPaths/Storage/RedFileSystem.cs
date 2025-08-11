using System.Collections.Concurrent;

namespace SmartPaths.Storage;

//todo: implement a way to export the delta state (file, folder, and fileSystem)
//todo: implement a way to persist state?

/// <summary>Represents an in-memory file system that supports editing operations.</summary>
/// <remarks>RED stands for Ram Edited Disk, allowing a non-permanent write back file system.<br /> The
///     <see cref="RedFileSystem" /> provides an editable, in-memory representation of a file system.
///     It allows creating, deleting, and querying files and folders, while maintaining a cache of the
///     current state. This class is particularly useful for scenarios where a temporary or mock file
///     system is needed, such as testing or prototyping.</remarks>
public sealed class RedFileSystem : BaseFileSystem<RedFolder, RedFile, RedWatcher>
{

    private readonly WeakCollection<RedWatcher> _watchers = [];

    public override AbsoluteFolderPath AppLocalStoragePath => Disk.AppLocalStoragePath;

    public override AbsoluteFolderPath AppRoamingStoragePath => Disk.AppRoamingStoragePath;

    public override AbsoluteFolderPath TempStoragePath => Disk.TempStoragePath;

    public override AbsoluteFolderPath WorkingDirectory {
        get => Disk.WorkingDirectory;
        set => Disk.WorkingDirectory = value;
    }

    internal DiskFileSystem Disk { get; } = new();

    internal ConcurrentDictionary<AbsoluteFilePath, RedFile> Files { get; } = [];

    internal ConcurrentDictionary<AbsoluteFolderPath, RedFolder> Folders { get; } = [];

    public override async Task<RedFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        RedFolder folder = await CreateFolder(filePath.Folder);
        return await folder.CreateFile(filePath.FileName, collisionStrategy);
    }

    public override async Task<RedFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        //Check cache
        if (Folders.TryGetValue(folderPath, out RedFolder? folder)) {
            return folder;
        }
        //Create folders
        return await EnsureFolderTree(folderPath);
    }

    public override async Task DeleteFile(AbsoluteFilePath filePath) {
        //Call GetFile() to ensure the disk state is observed
        RedFile? file = await GetFile(filePath);
        if (file is not null) {
            //Delete in memory
            await file.Delete();
        }
    }

    public override async Task DeleteFolder(AbsoluteFolderPath folderPath) {
        //Call GetFolder() to ensure the disk state is observed
        RedFolder? folder = await GetFolder(folderPath);
        if (folder is not null) {
            //Delete in memory
            await folder.Delete();
        }
    }

    public override async Task<bool> FileExists(AbsoluteFilePath filePath) {
        //Call GetFile() to ensure the disk state is observed
        RedFile? file = await GetFile(filePath);
        return file is not null && !file.WasDeleted;
    }

    public override async Task<bool> FolderExists(AbsoluteFolderPath folderPath) {
        //Call GetFolder() to ensure the disk state is observed
        RedFolder? folder = await GetFolder(folderPath);
        return folder is not null && !folder.WasDeleted;
    }

    public override Task<RedFolder> GetAppLocalStorage() {
        return CreateFolder(AppLocalStoragePath);
    }

    public override Task<RedFolder> GetAppRoamingStorage() {
        return CreateFolder(AppRoamingStoragePath);
    }

    public override async Task<RedFile?> GetFile(AbsoluteFilePath filePath) {
        //1. check the cache,
        if (Files.TryGetValue(filePath, out RedFile? file)) {
            return file.WasDeleted ? null : file;
        }
        //2. check disk
        RedFolder? folder = await GetFolder(filePath.Parent);
        if (folder is null) {
            return null;
        }
        //3. return
        return await folder.GetFile(filePath.FileName);
    }

    public override async Task<RedFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        //1. check the cache,
        if (Folders.TryGetValue(folderPath, out RedFolder? folder)) {
            return folder.WasDeleted ? null : folder;
        }
        //2. check disk
        if (await Disk.FolderExists(folderPath)) {
            folder = await EnsureFolderTree(folderPath);
        }
        //3. return
        return folder;
    }

    public override Task<RedFolder> GetTempStorage() {
        return CreateFolder(TempStoragePath);
    }

    public override Task<RedWatcher> GetWatcher(AbsoluteFolderPath folderPath,
                                                string filter = "*",
                                                bool includeSubFolders = false,
                                                NotifyFilters notifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite) {
        RedWatcher watcher = new(folderPath, filter, includeSubFolders, notifyFilter);
        _watchers.Add(watcher);
        return Task.FromResult(watcher);
    }

    internal async Task<AbsoluteFilePath> MakeUnique(AbsoluteFilePath path) {
        RedFolder? folder = await GetFolder(path.Parent);
        if (folder is null) {
            throw new Exception($"Can not find folder: {path}");
        }
        await folder.BuildCacheInfo();

        int extra = 2;
        while (true) {
            AbsoluteFilePath alternatePath = UpdateName(path, extra);
            if (!Files.ContainsKey(alternatePath)) {
                return alternatePath;
            }
            extra++;
        }
    }

    internal void ProcessErrorEvent(ErrorEventArgs args) {
        foreach (RedWatcher watcher in _watchers.LiveList) {
            watcher.ProcessErrorEvent(args);
        }
    }

    internal void ProcessStorageEvent(FileSystemEventArgs args) {
        foreach (RedWatcher watcher in _watchers.LiveList) {
            watcher.ProcessStorageEvent(args);
        }
    }

    internal void ProcessStorageEvent(RenamedEventArgs args) {
        foreach (RedWatcher watcher in _watchers.LiveList) {
            watcher.ProcessStorageEvent(args);
        }
    }

    private Task<RedFolder> EnsureFolderTree(AbsoluteFolderPath folderPath) {
        return Task.Run(() => {
                            RedFolder? found = null;
                            //build list of paths from leaf in for evey level that is missing
                            List<AbsoluteFolderPath> missing = [folderPath];
                            while (folderPath.HasParent) {
                                folderPath = folderPath.Parent;
                                if (Folders.TryGetValue(folderPath, out found)) {
                                    break;
                                }
                                missing.Add(folderPath);
                            }
                            missing.Reverse();
                            //ensure we have a root to build from
                            if (found is null) {
                                folderPath = missing[0];
                                missing.RemoveAt(0);
                                //NB: this should be a new root
                                found = Folders.GetOrAdd(folderPath, path => new RedFolder(this, path));
                            }
                            //unwrap the list
                            while (missing.Count > 0) {
                                found = new RedFolder(this, missing[0]);
                                Folders[found.Path] = found;
                                missing.RemoveAt(0);
                            }
                            //return
                            return found;
                        });
    }

}