using System.Collections.Concurrent;

namespace SmartPaths.Storage;

public sealed class RamFileSystem : SmartFileSystem<RamFolder, RamFile, SmartWatcher>
{

    private readonly WeakCollection<SmartWatcher> _watchers = [];

    public RamFileSystem() {
        WorkingDirectory = RootPath;
        Root = new RamFolder(this, RootPath);
        Folders[RootPath] = Root;
    }

    public override AbsoluteFolderPath AppLocalStoragePath { get; } = @"\LocalStorage\";

    public override AbsoluteFolderPath AppRoamingStoragePath { get; } = @"\RoamingStorage\";

    public RamFolder Root { get; }

    public override AbsoluteFolderPath TempStoragePath { get; } = @"\Temp\";

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
        return Task.FromResult(file);
    }

    public override Task<RamFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        Folders.TryGetValue(folderPath, out RamFolder? folder);
        return Task.FromResult(folder);
    }

    public override Task<RamFolder> GetTempStorage() {
        return Root.CreateFolder(TempStoragePath);
    }

    public override Task<SmartWatcher> GetWatcher(AbsoluteFolderPath folderPath, string filter = "*", bool includeSubFolders = false) {
        SmartWatcher watcher = new(folderPath, filter, includeSubFolders);
        _watchers.Add(watcher);
        return Task.FromResult(watcher);
    }

    public Task<Ledger> StartLedger() {
        return StartNewLedger(RootPath);
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

    internal void ProcessStorageEvent(FolderEventRecord record) {
        foreach (SmartWatcher watcher in _watchers.LiveList) {
            watcher.ProcessStorageEvent(record);
        }
    }

    internal void ProcessStorageEvent(FileEventRecord record) {
        foreach (SmartWatcher watcher in _watchers.LiveList) {
            watcher.ProcessStorageEvent(record);
        }
    }

    public static AbsoluteFolderPath RootPath { get; } = @"\";

    private static void EnsureIsRamPath(AbsolutePath filePath) {
        if (filePath.PathType != PathType.RootRelative) {
            throw new Exception($"Only root relative paths allowed. ({filePath})");
        }
    }

}