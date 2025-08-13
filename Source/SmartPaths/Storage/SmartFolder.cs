namespace SmartPaths.Storage;

public abstract class SmartFolder<TFolder, TFile> : IFolder
    where TFolder : SmartFolder<TFolder, TFile>
    where TFile : SmartFile<TFolder, TFile>
{

    protected SmartFolder(AbsoluteFolderPath path) {
        Path = path;
    }

    public bool IsRoot => Path.IsRoot;

    public string Name => Path.FolderName;

    public TFolder? Parent { get; protected init; }

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

    /// <summary>Deletes the current folder and its contents asynchronously.</summary>
    /// <remarks>This method removes the folder and all its child files and subfolders. The exact behavior
    ///     of this method depends on the specific implementation in derived classes.</remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
    public abstract Task Delete();

    /// <summary>Deletes a file with the specified name from the folder.</summary>
    /// <param name="fileName">The name of the file to delete. This should be the name of an existing file
    ///     within the folder.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous operation.</returns>
    /// <remarks>If the file does not exist, the method completes without performing any action.</remarks>
    public async Task DeleteFile(string fileName) {
        TFile? file = await GetFile(fileName);
        if (file is not null) {
            await file.Delete();
        }
    }

    /// <summary>Deletes a subfolder with the specified name from the current folder asynchronously.</summary>
    /// <param name="folderName">The name of the subfolder to delete. This should be the name of an
    ///     existing subfolder within the current folder.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous operation.</returns>
    /// <remarks>If the subfolder does not exist, the method completes without performing any action.</remarks>
    public async Task DeleteFolder(string folderName) {
        TFolder? folder = await GetFolder(folderName);
        if (folder is not null) {
            await folder.Delete();
        }
    }

    public abstract Task<bool> Exists();

    public Task<TFile?> GetFile(string fileName) {
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        return GetFile(filePath);
    }

    public abstract Task<IReadOnlyList<TFile>> GetFiles();

    public Task<IReadOnlyList<TFile>> GetFiles(string searchPattern) {
        throw new NotImplementedException();
    }

    public Task<TFolder?> GetFolder(string folderName) {
        AbsoluteFolderPath folderPath = Path.GetChildFolderPath(folderName);
        return GetFolder(folderPath);
    }

    public abstract Task<IReadOnlyList<TFolder>> GetFolders();

    public Task<IReadOnlyList<TFolder>> GetFolders(string searchPattern) {
        throw new NotImplementedException();
    }

    internal abstract Task<TFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy);

    internal abstract Task<TFolder> CreateFolder(AbsoluteFolderPath folderPath);

    internal abstract void Expunge(SmartFile<TFolder, TFile> file, bool notify);

    internal abstract Task<TFile?> GetFile(AbsoluteFilePath filePath);

    internal abstract Task<TFolder?> GetFolder(AbsoluteFolderPath folderPath);

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
        return GetFiles().ContinueWith(IReadOnlyList<IFile> (task) => task.Result);
    }

    Task<IReadOnlyList<IFile>> IFolder.GetFiles(string searchPattern) {
        return GetFiles(searchPattern).ContinueWith(IReadOnlyList<IFile> (task) => task.Result);
    }

    Task<IFolder?> IFolder.GetFolder(string folderName) {
        return GetFolder(folderName).ContinueWith(IFolder? (task) => task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFolder.GetFolders() {
        return GetFolders().ContinueWith(IReadOnlyList<IFolder> (task) => task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFolder.GetFolders(string searchPattern) {
        return GetFolders(searchPattern).ContinueWith(IReadOnlyList<IFolder> (task) => task.Result);
    }

}