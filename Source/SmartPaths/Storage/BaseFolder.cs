namespace SmartPaths.Storage;

public abstract class BaseFolder<TFolder, TFile> : IFolder
    where TFolder : IFolder
    where TFile : IFile
{

    protected BaseFolder(AbsoluteFolderPath path) {
        Path = path;
    }

    public bool IsRoot => Path.IsRoot;

    public string Name => Path.FolderName;

    public abstract TFolder? Parent { get; }

    public AbsoluteFolderPath Path { get; }

    IFolder? IFolder.Parent => Parent;

    public Task<TFile> CreateFile(string fileName, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        return CreateFile(filePath, collisionStrategy);
    }

    public Task<TFolder> CreateFolder(string folderName) {
        AbsoluteFolderPath folderPath = Path.GetChildFolderPath(folderName);
        return CreateFolder(folderPath);
    }

    public abstract Task Delete();

    public abstract Task DeleteFile(string fileName);

    public abstract Task DeleteFolder(string folderName);

    public abstract Task<bool> Exists();

    public abstract Task<TFile?> GetFile(string fileName);

    public abstract Task<IReadOnlyList<TFile>> GetFiles();

    public abstract Task<TFolder?> GetFolder(string folderName);

    public abstract Task<IReadOnlyList<TFolder>> GetFolders();

    internal abstract Task<TFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy);

    internal abstract Task<TFolder> CreateFolder(AbsoluteFolderPath folderPath);

    Task<IFile> IFolder.CreateFile(string fileName, CollisionStrategy collisionStrategy) {
        return CreateFile(fileName, collisionStrategy).ContinueWith(IFile (task) => task.Result);
    }

    Task<IFolder> IFolder.CreateFolder(string folderName) {
        return CreateFolder(folderName).ContinueWith(IFolder (task) => task.Result);
    }

    Task<IFile?> IFolder.GetFile(string fileName) {
        return GetFile(fileName).ContinueWith(IFile? (task) => task.Result);
    }

    Task<IReadOnlyList<IFile>> IFolder.GetFiles() {
        return GetFiles().ContinueWith(task => (IReadOnlyList<IFile>)task.Result);
    }

    Task<IFolder?> IFolder.GetFolder(string folderName) {
        return GetFolder(folderName).ContinueWith(IFolder? (task) => task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFolder.GetFolders() {
        return GetFolders().ContinueWith(task => (IReadOnlyList<IFolder>)task.Result);
    }

}