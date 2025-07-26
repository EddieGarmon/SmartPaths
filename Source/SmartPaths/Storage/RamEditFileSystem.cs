using System.Collections.Concurrent;

namespace SmartPaths.Storage;

//todo: implement a way to export the delta state (file, folder, and fileSystem)
//todo: implement a way to persist state?

/// <summary>Represents an in-memory file system that supports editing operations.</summary>
/// <remarks>The <see cref="RamEditFileSystem" /> provides an editable, in-memory representation of a
///     file system. It allows creating, deleting, and querying files and folders, while maintaining a
///     cache of the current state. This class is particularly useful for scenarios where a temporary
///     or mock file system is needed, such as testing or prototyping.</remarks>
public sealed class RamEditFileSystem : BaseFileSystem<RamEditFolder, RamEditFile>
{

    public override AbsoluteFolderPath AppLocalStoragePath => Disk.AppLocalStoragePath;

    public override AbsoluteFolderPath AppRoamingStoragePath => Disk.AppRoamingStoragePath;

    public override AbsoluteFolderPath TempStoragePath => Disk.TempStoragePath;

    public override AbsoluteFolderPath WorkingDirectory {
        get => Disk.WorkingDirectory;
        set => Disk.WorkingDirectory = value;
    }

    internal DiskFileSystem Disk { get; } = new();

    internal ConcurrentDictionary<AbsoluteFilePath, RamEditFile> Files { get; } = [];

    internal ConcurrentDictionary<AbsoluteFolderPath, RamEditFolder> Folders { get; } = [];

    public override async Task<RamEditFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        RamEditFolder folder = await CreateFolder(filePath.Folder);
        return await folder.CreateFile(filePath.FileName, collisionStrategy);
    }

    public override async Task<RamEditFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        //Check cache
        if (Folders.TryGetValue(folderPath, out RamEditFolder? folder)) {
            return folder;
        }
        //Create folders
        return await EnsureFolderTree(folderPath);
    }

    public override async Task DeleteFile(AbsoluteFilePath filePath) {
        //Call GetFile() to ensure the disk state is observed
        RamEditFile? file = await GetFile(filePath);
        if (file is not null) {
            //Delete in memory
            await file.Delete();
        }
    }

    public override async Task DeleteFolder(AbsoluteFolderPath folderPath) {
        //Call GetFolder() to ensure the disk state is observed
        RamEditFolder? folder = await GetFolder(folderPath);
        if (folder is not null) {
            //Delete in memory
            await folder.Delete();
        }
    }

    public override async Task<bool> FileExists(AbsoluteFilePath filePath) {
        //Call GetFile() to ensure the disk state is observed
        RamEditFile? file = await GetFile(filePath);
        return file is not null && !file.WasDeleted;
    }

    public override async Task<bool> FolderExists(AbsoluteFolderPath folderPath) {
        //Call GetFolder() to ensure the disk state is observed
        RamEditFolder? folder = await GetFolder(folderPath);
        return folder is not null && !folder.WasDeleted;
    }

    public override Task<RamEditFolder> GetAppLocalStorage() {
        return CreateFolder(AppLocalStoragePath);
    }

    public override Task<RamEditFolder> GetAppRoamingStorage() {
        return CreateFolder(AppRoamingStoragePath);
    }

    public override async Task<RamEditFile?> GetFile(AbsoluteFilePath filePath) {
        //1. check the cache,
        if (Files.TryGetValue(filePath, out RamEditFile? file)) {
            return file;
        }
        //2. check disk
        RamEditFolder? folder = await GetFolder(filePath.Parent);
        if (folder is null) {
            return null;
        }
        //3. return
        return await folder.GetFile(filePath.FileName);
    }

    public override async Task<RamEditFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        //1. check the cache,
        if (Folders.TryGetValue(folderPath, out RamEditFolder? folder)) {
            return folder;
        }
        //2. check disk
        if (await Disk.FolderExists(folderPath)) {
            folder = await EnsureFolderTree(folderPath);
        }
        //3. return
        return folder;
    }

    public override Task<RamEditFolder> GetTempStorage() {
        return CreateFolder(TempStoragePath);
    }

    internal async Task<AbsoluteFilePath> MakeUnique(AbsoluteFilePath path) {
        RamEditFolder? folder = await GetFolder(path.Parent);
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

    private Task<RamEditFolder> EnsureFolderTree(AbsoluteFolderPath folderPath) {
        return Task.Run(() => {
                            RamEditFolder? found = null;
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
                                found = Folders.GetOrAdd(folderPath, path => new RamEditFolder(this, path));
                            }
                            //unwrap the list
                            while (missing.Count > 0) {
                                found = new RamEditFolder(this, missing[0]);
                                missing.RemoveAt(0);
                            }
                            //return
                            return found;
                        });
    }

}