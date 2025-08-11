using System.Collections.Concurrent;

namespace SmartPaths.Storage;

public sealed class RamFileSystem : BaseFileSystem<RamFolder, RamFile, RamWatcher>
{

    private readonly WeakCollection<RamWatcher> _watchers = [];

    public RamFileSystem() {
        WorkingDirectory = RootPath;
        Root = new RamFolder(this, RootPath);
        Folders[RootPath] = Root;
    }

    public override AbsoluteFolderPath AppLocalStoragePath { get; } = @"ram:\LocalStorage\";

    public override AbsoluteFolderPath AppRoamingStoragePath { get; } = @"ram:\RoamingStorage\";

    public RamFolder Root { get; }

    public override AbsoluteFolderPath TempStoragePath { get; } = @"ram:\Temp\";

    public override AbsoluteFolderPath WorkingDirectory { get; set; }

    internal ConcurrentDictionary<AbsoluteFilePath, RamFile> Files { get; } = [];

    internal ConcurrentDictionary<AbsoluteFolderPath, RamFolder> Folders { get; } = [];

    public override async Task<RamFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        EnsureIsRamPath(filePath);
        RamFolder folder = await CreateFolder(filePath.Folder);
        return await folder.CreateFile(filePath.FileName);
    }

    public override async Task<RamFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        if (!Folders.TryGetValue(folderPath, out RamFolder? folder)) {
            folder = await Root.EnsureFolderTree(folderPath);
        }
        return folder;
    }

    public override Task DeleteFile(AbsoluteFilePath filePath) {
        EnsureIsRamPath(filePath);
        return !Files.TryGetValue(filePath, out RamFile? file) ? Task.CompletedTask : file.Delete();
    }

    public override Task DeleteFolder(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        return !Folders.TryGetValue(folderPath, out RamFolder? folder) ? Task.CompletedTask : folder.Delete();
    }

    public override Task<bool> FileExists(AbsoluteFilePath filePath) {
        EnsureIsRamPath(filePath);
        return Task.FromResult(Files.ContainsKey(filePath));
    }

    public override Task<bool> FolderExists(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        return Task.FromResult(Folders.ContainsKey(folderPath));
    }

    public override Task<RamFolder> GetAppLocalStorage() {
        return Root.CreateFolder(AppLocalStoragePath);
    }

    public override Task<RamFolder> GetAppRoamingStorage() {
        return Root.CreateFolder(AppRoamingStoragePath);
    }

    public override Task<RamFile?> GetFile(AbsoluteFilePath filePath) {
        EnsureIsRamPath(filePath);
        Files.TryGetValue(filePath, out RamFile? file);
        return Task.FromResult<RamFile?>(file);
    }

    public override Task<RamFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        Folders.TryGetValue(folderPath, out RamFolder? folder);
        return Task.FromResult<RamFolder?>(folder);
    }

    public override Task<RamFolder> GetTempStorage() {
        return Root.CreateFolder(TempStoragePath);
    }

    public override Task<RamWatcher> GetWatcher(AbsoluteFolderPath folderPath,
                                                string filter = "*",
                                                bool includeSubFolders = false,
                                                NotifyFilters notifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite) {
        RamWatcher watcher = new(folderPath, filter, includeSubFolders, notifyFilter);
        _watchers.Add(watcher);
        return Task.FromResult(watcher);
    }

    internal AbsoluteFilePath MakeUnique(AbsoluteFilePath path) {
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
        foreach (RamWatcher watcher in _watchers.LiveList) {
            watcher.ProcessErrorEvent(args);
        }
    }

    internal void ProcessStorageEvent(FileSystemEventArgs args) {
        foreach (RamWatcher watcher in _watchers.LiveList) {
            watcher.ProcessStorageEvent(args);
        }
    }

    internal void ProcessStorageEvent(RenamedEventArgs args) {
        foreach (RamWatcher watcher in _watchers.LiveList) {
            watcher.ProcessStorageEvent(args);
        }
    }

    public static AbsoluteFolderPath RootPath { get; } = @"ram:\";

    private static void EnsureIsRamPath(AbsolutePath filePath) {
        if (filePath.RootValue != "ram:\\") {
            throw new Exception($"Only 'ram:\\' rooted paths allowed. ({filePath})");
        }
    }

}