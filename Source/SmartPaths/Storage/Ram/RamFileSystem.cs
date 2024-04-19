namespace SmartPaths.Storage.Ram;

public class RamFileSystem : BaseFileSystem<RamFolder, RamFile>
{

    private readonly Dictionary<AbsoluteFilePath, RamFile> _allFiles = [];
    private readonly Dictionary<AbsoluteFolderPath, RamFolder> _allFolders = [];
    private readonly RamFolder _root;

    public RamFileSystem() {
        AbsoluteFolderPath rootPath = @"ram:\";
        WorkingDirectory = rootPath;
        _root = new RamFolder(this, rootPath);
        _allFolders.Add(_root.Path, _root);
    }

    public override AbsoluteFolderPath AppLocalStoragePath { get; } = @"ram:\LocalStorage\";

    public override AbsoluteFolderPath AppRoamingStoragePath { get; } = @"ram:\RoamingStorage\";

    public override AbsoluteFolderPath TempStoragePath { get; } = @"ram:\Temp\";

    public override AbsoluteFolderPath WorkingDirectory { get; set; }

    public override async Task<RamFile> CreateFile(AbsoluteFilePath absoluteFile,
                                                   CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        EnsureIsRamPath(absoluteFile);
        if (!_allFolders.TryGetValue(absoluteFile.Folder, out RamFolder? folder)) {
            folder = await _root.EnsureFolderTree(absoluteFile.Folder);
        }
        return await folder.CreateFile(absoluteFile);
    }

    public override async Task<RamFolder> CreateFolder(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        if (!_allFolders.TryGetValue(absoluteFolder, out RamFolder? folder)) {
            folder = await _root.EnsureFolderTree(absoluteFolder);
        }
        return folder;
    }

    public override Task DeleteFile(AbsoluteFilePath absoluteFile) {
        EnsureIsRamPath(absoluteFile);
        return !_allFiles.TryGetValue(absoluteFile, out RamFile? file) ? Task.CompletedTask : file.Delete();
    }

    public override Task DeleteFolder(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        return !_allFolders.TryGetValue(absoluteFolder, out RamFolder? folder) ? Task.CompletedTask : folder.Delete();
    }

    public override Task<bool> FileExists(AbsoluteFilePath absoluteFile) {
        EnsureIsRamPath(absoluteFile);
        return Task.FromResult(_allFiles.ContainsKey(absoluteFile));
    }

    public override Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        return Task.FromResult(_allFolders.ContainsKey(absoluteFolder));
    }

    public override Task<RamFolder> GetAppLocalStorage() {
        return _root.CreateFolder(AppLocalStoragePath);
    }

    public override Task<RamFolder> GetAppRoamingStorage() {
        return _root.CreateFolder(AppRoamingStoragePath);
    }

    public override Task<RamFile?> GetFile(AbsoluteFilePath absoluteFile) {
        EnsureIsRamPath(absoluteFile);
        _allFiles.TryGetValue(absoluteFile, out RamFile? file);
        return Task.FromResult<RamFile?>(file);
    }

    public override Task<RamFolder?> GetFolder(AbsoluteFolderPath absoluteFolder) {
        EnsureIsRamPath(absoluteFolder);
        _allFolders.TryGetValue(absoluteFolder, out RamFolder? folder);
        return Task.FromResult<RamFolder?>(folder);
    }

    public override Task<RamFolder> GetTempStorage() {
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

    private static void EnsureIsRamPath(AbsolutePath filePath) {
        if (filePath.RootValue != "ram:\\") {
            throw new Exception($"Only 'ram:\\' rooted paths allowed. ({filePath})");
        }
    }

}