namespace SmartPaths.Storage.Ram;

public class RamFileSystem : IFileSystem
{

    private readonly Dictionary<AbsoluteFilePath, RamFile> _allFiles = [];
    private readonly Dictionary<AbsoluteFolderPath, RamFolder> _allFolders = [];
    private readonly RamFolder _root;

    public RamFileSystem() {
        _root = new RamFolder(this, "ram:\\");
        _allFolders.Add(_root.Path, _root);
    }

    public AbsoluteFolderPath AppLocalStoragePath { get; } = @"ram:\LocalStorage\";

    public AbsoluteFolderPath AppRoamingStoragePath { get; } = @"ram:\RoamingStorage\";

    public AbsoluteFolderPath TempStoragePath { get; } = @"ram:\Temp\";

    public async Task<RamFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        EnsureIsRamPath(filePath);
        if (!_allFolders.TryGetValue(filePath.Folder, out RamFolder? folder)) {
            folder = await _root.EnsureFolderTree(filePath.Folder);
        }
        return await folder.CreateFile(filePath);
    }

    public async Task<RamFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        if (!_allFolders.TryGetValue(folderPath, out RamFolder? folder)) {
            folder = await _root.EnsureFolderTree(folderPath);
        }
        return folder;
    }

    public Task DeleteFile(AbsoluteFilePath path) {
        EnsureIsRamPath(path);
        return !_allFiles.TryGetValue(path, out RamFile? file) ? Task.CompletedTask : file.Delete();
    }

    public Task DeleteFolder(AbsoluteFolderPath path) {
        EnsureIsRamPath(path);
        return !_allFolders.TryGetValue(path, out RamFolder? folder) ? Task.CompletedTask : folder.Delete();
    }

    public Task<bool> FileExists(AbsoluteFilePath path) {
        EnsureIsRamPath(path);
        return Task.FromResult(_allFiles.ContainsKey(path));
    }

    public Task<bool> FolderExists(AbsoluteFolderPath path) {
        EnsureIsRamPath(path);
        return Task.FromResult(_allFolders.ContainsKey(path));
    }

    public Task<RamFolder> GetAppLocalStorage() {
        return _root.CreateFolder(AppLocalStoragePath);
    }

    public Task<RamFolder> GetAppRoamingStorage() {
        return _root.CreateFolder(AppRoamingStoragePath);
    }

    public Task<RamFile?> GetFile(AbsoluteFilePath filePath) {
        EnsureIsRamPath(filePath);
        _allFiles.TryGetValue(filePath, out RamFile? file);
        return Task.FromResult(file);
    }

    public Task<RamFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        EnsureIsRamPath(folderPath);
        _allFolders.TryGetValue(folderPath, out RamFolder? folder);
        return Task.FromResult(folder);
    }

    public Task<RamFolder> GetTempStorage() {
        return _root.CreateFolder(TempStoragePath);
    }

    internal void Expunge(RamFile file) {
        _allFiles.Remove(file.Path);
    }

    internal void Expunge(RamFolder folder) {
        _allFolders.Remove(folder.Path);
    }

    internal AbsoluteFilePath MakeUnique(AbsoluteFilePath path) {
        int extra = 2;
        while (true) {
            string alternateName = $"{path.FileNameWithoutExtension} ({extra}).{path.FileExtension}";
            AbsoluteFilePath alternatePath = path.GetSiblingFilePath(alternateName);
            if (!_allFiles.ContainsKey(alternatePath)) {
                return alternatePath;
            }
            extra++;
        }
    }

    internal void Register(RamFile file) {
        _allFiles[file.Path] = file;
    }

    internal void Register(RamFolder folder) {
        _allFolders[folder.Path] = folder;
    }

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

    Task<IFolder> IFileSystem.CreateFolder(AbsoluteFolderPath folderPath) {
        return CreateFolder(folderPath).ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFolder> IFileSystem.GetAppLocalStorage() {
        return GetAppLocalStorage().ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFolder> IFileSystem.GetAppRoamingStorage() {
        return GetAppRoamingStorage().ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFile?> IFileSystem.GetFile(AbsoluteFilePath filePath) {
        return GetFile(filePath).ContinueWith(task => (IFile?)task.Result);
    }

    Task<IFolder?> IFileSystem.GetFolder(AbsoluteFolderPath folderPath) {
        return GetFolder(folderPath).ContinueWith(task => (IFolder?)task.Result);
    }

    Task<IFolder> IFileSystem.GetTempStorage() {
        return GetTempStorage().ContinueWith(task => (IFolder)task.Result);
    }

    private static void EnsureIsRamPath(AbsolutePath filePath) {
        if (filePath.RootValue != "ram:\\") {
            throw new Exception($"Only 'ram:\\' rooted paths allowed. ({filePath})");
        }
    }

}