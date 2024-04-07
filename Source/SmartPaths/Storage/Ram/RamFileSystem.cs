namespace SmartPaths.Storage.Ram;

public class RamFileSystem : IFileSystem
{

    private readonly Dictionary<AbsoluteFilePath, RamFile> _allFiles = [];
    private readonly Dictionary<AbsoluteFolderPath, RamFolder> _allFolders = [];
    private readonly RamFolder _root;

    public RamFileSystem() {
        _root = new RamFolder(this, "ram:\\");
        _allFolders.Add(_root.Path, _root);
        CurrentDirectory = _root.Path;
    }

    public AbsoluteFolderPath AppLocalStoragePath { get; } = @"ram:\LocalStorage\";

    public AbsoluteFolderPath AppRoamingStoragePath { get; } = @"ram:\RoamingStorage\";

    public AbsoluteFolderPath CurrentDirectory { get; set; }

    public AbsoluteFolderPath TempStoragePath { get; } = @"ram:\Temp\";

    public async Task<RamFile> CreateFile(AbsoluteFilePath absoluteFile,
                                          CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        EnsureIsRamPath(absoluteFile);
        if (!_allFolders.TryGetValue(absoluteFile.Folder, out RamFolder? folder)) {
            folder = await _root.EnsureFolderTree(absoluteFile.Folder);
        }
        return await folder.CreateFile(absoluteFile);
    }

    public async Task<RamFolder> CreateFolder(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        if (!_allFolders.TryGetValue(absoluteFolder, out RamFolder? folder)) {
            folder = await _root.EnsureFolderTree(absoluteFolder);
        }
        return folder;
    }

    public Task DeleteFile(AbsoluteFilePath absoluteFile) {
        EnsureIsRamPath(absoluteFile);
        return !_allFiles.TryGetValue(absoluteFile, out RamFile? file) ? Task.CompletedTask : file.Delete();
    }

    public Task DeleteFolder(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        return !_allFolders.TryGetValue(absoluteFolder, out RamFolder? folder) ? Task.CompletedTask : folder.Delete();
    }

    public Task<bool> FileExists(AbsoluteFilePath absoluteFile) {
        EnsureIsRamPath(absoluteFile);
        return Task.FromResult(_allFiles.ContainsKey(absoluteFile));
    }

    public Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        return Task.FromResult(_allFolders.ContainsKey(absoluteFolder));
    }

    public Task<RamFolder> GetAppLocalStorage() {
        return _root.CreateFolder(AppLocalStoragePath);
    }

    public Task<RamFolder> GetAppRoamingStorage() {
        return _root.CreateFolder(AppRoamingStoragePath);
    }

    public Task<RamFile?> GetFile(AbsoluteFilePath absoluteFile) {
        EnsureIsRamPath(absoluteFile);
        _allFiles.TryGetValue(absoluteFile, out RamFile? file);
        return Task.FromResult(file);
    }

    public Task<RamFolder?> GetFolder(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        _allFolders.TryGetValue(absoluteFolder, out RamFolder? folder);
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

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath absoluteFile, CollisionStrategy collisionStrategy) {
        return CreateFile(absoluteFile, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

    Task<IFolder> IFileSystem.CreateFolder(AbsoluteFolderPath absoluteFolder) {
        return CreateFolder(absoluteFolder).ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFolder> IFileSystem.GetAppLocalStorage() {
        return GetAppLocalStorage().ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFolder> IFileSystem.GetAppRoamingStorage() {
        return GetAppRoamingStorage().ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFile?> IFileSystem.GetFile(AbsoluteFilePath absoluteFile) {
        return GetFile(absoluteFile).ContinueWith(task => (IFile?)task.Result);
    }

    Task<IFolder?> IFileSystem.GetFolder(AbsoluteFolderPath absoluteFolder) {
        return GetFolder(absoluteFolder).ContinueWith(task => (IFolder?)task.Result);
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