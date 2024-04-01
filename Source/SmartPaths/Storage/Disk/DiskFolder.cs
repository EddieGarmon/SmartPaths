namespace SmartPaths.Storage.Disk;

public class DiskFolder : IFolder
{

    public DiskFolder(AbsoluteFolderPath path) {
        Path = path;
    }

    public string Name => Path.FolderName;

    public AbsoluteFolderPath Path { get; }

    public Task<IFile> CreateFile(string fileName, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        AssertExists();
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        if (File.Exists(filePath)) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    filePath = DiskFile.MakeUnique(filePath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    File.Delete(filePath);
                    break;
                case CollisionStrategy.FailIfExists:
                    throw StorageExceptions.FileExists(filePath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
            }
        }
        using (File.Create(filePath)) { }
        return Task.FromResult<IFile>(new DiskFile(filePath));
    }

    public Task<IFolder> CreateFolder(string folderName) {
        AssertExists();
        AbsoluteFolderPath child = Path.GetChildFolderPath(folderName);
        Directory.CreateDirectory(child);
        return Task.FromResult<IFolder>(new DiskFolder(child));
    }

    public Task Delete() {
        AssertExists();
        Directory.Delete(Path, true);
        return Task.CompletedTask;
    }

    public Task<bool> Exists() {
        return Task.FromResult(Directory.Exists(Path));
    }

    public Task<IFile?> GetFile(string name) {
        AssertExists();
        IFile? result = null;
        AbsoluteFilePath filePath = Path.GetChildFilePath(name);
        if (File.Exists(filePath)) {
            result = new DiskFile(filePath);
        }
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<IFile>> GetFiles() {
        AssertExists();
        IReadOnlyList<IFile> result = Directory.GetFiles(Path).Select(filePath => new DiskFile(filePath)).ToList<IFile>().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IFolder?> GetFolder(string name) {
        AssertExists();
        IFolder? result = null;
        AbsoluteFolderPath child = Path.GetChildFolderPath(name);
        if (Directory.Exists(child)) {
            result = new DiskFolder(child);
        }
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<IFolder>> GetFolders() {
        AssertExists();
        IReadOnlyList<IFolder> result = Directory.GetDirectories(Path)
                                                 .Select(child => new DiskFolder(child))
                                                 .ToList<IFolder>()
                                                 .AsReadOnly();
        return Task.FromResult(result);
    }

    private void AssertExists() {
        if (!Directory.Exists(Path)) {
            throw StorageExceptions.FolderMissing(Path);
        }
    }

}