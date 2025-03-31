using System.Diagnostics;

namespace SmartPaths.Storage.Disk;

[DebuggerDisplay("[Folder] {Path.ToString()}")]
public class DiskFolder : IFolder
{

    public DiskFolder(AbsoluteFolderPath path) {
        Path = path;
    }

    public string Name => Path.FolderName;

    public DiskFolder Parent => new(Path.Parent);

    public AbsoluteFolderPath Path { get; }

    IFolder IFolder.Parent => Parent;

    public Task<DiskFile> CreateFile(string fileName, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
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
        return Task.FromResult(new DiskFile(filePath));
    }

    public Task<DiskFolder> CreateFolder(string folderName) {
        AssertExists();
        AbsoluteFolderPath child = Path.GetChildFolderPath(folderName);
        Directory.CreateDirectory(child);
        return Task.FromResult(new DiskFolder(child));
    }

    public Task Delete() {
        return Task.Run(() => Directory.Delete(Path, true));
    }

    public Task DeleteFile(string fileName) {
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        return Task.Run(() => {
                            if (File.Exists(filePath)) {
                                File.Delete(filePath);
                            }
                        });
    }

    public Task DeleteFolder(string folderName) {
        AbsoluteFolderPath folderPath = Path.GetChildFolderPath(folderName);
        return Task.Run(() => {
                            if (Directory.Exists(folderPath)) {
                                Directory.Delete(folderPath, true);
                            }
                        });
    }

    public Task<bool> Exists() {
        return Task.FromResult(Directory.Exists(Path));
    }

    public Task<DiskFile?> GetFile(string fileName) {
        AssertExists();
        DiskFile? result = null;
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        if (File.Exists(filePath)) {
            result = new DiskFile(filePath);
        }
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DiskFile>> GetFiles() {
        AssertExists();
        IReadOnlyList<DiskFile> result = Directory.GetFiles(Path).Select(filePath => new DiskFile(filePath)).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<DiskFolder?> GetFolder(string folderName) {
        AssertExists();
        DiskFolder? result = null;
        AbsoluteFolderPath child = Path.GetChildFolderPath(folderName);
        if (Directory.Exists(child)) {
            result = new DiskFolder(child);
        }
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DiskFolder>> GetFolders() {
        AssertExists();
        IReadOnlyList<DiskFolder> result = Directory.GetDirectories(Path).Select(child => new DiskFolder(child)).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    private void AssertExists() {
        if (!Directory.Exists(Path)) {
            throw StorageExceptions.FolderMissing(Path);
        }
    }

    Task<IFile> IFolder.CreateFile(string fileName, CollisionStrategy collisionStrategy) {
        return CreateFile(fileName, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

    Task<IFolder> IFolder.CreateFolder(string folderName) {
        return CreateFolder(folderName).ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFile?> IFolder.GetFile(string fileName) {
        return GetFile(fileName).ContinueWith(task => (IFile?)task.Result);
    }

    Task<IReadOnlyList<IFile>> IFolder.GetFiles() {
        AssertExists();
        IReadOnlyList<IFile> result = Directory.GetFiles(Path).Select(filePath => new DiskFile(filePath)).ToList<IFile>().AsReadOnly();
        return Task.FromResult(result);
    }

    Task<IFolder?> IFolder.GetFolder(string folderName) {
        return GetFolder(folderName).ContinueWith(task => (IFolder?)task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFolder.GetFolders() {
        AssertExists();
        IReadOnlyList<IFolder> result = Directory.GetDirectories(Path).Select(child => new DiskFolder(child)).ToList<IFolder>().AsReadOnly();
        return Task.FromResult(result);
    }

}