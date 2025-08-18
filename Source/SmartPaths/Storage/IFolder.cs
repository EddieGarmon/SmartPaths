namespace SmartPaths.Storage;

public interface IFolder
{

    string Name { get; }

    IFolder? Parent { get; }

    AbsoluteFolderPath Path { get; }

    Task<IFile> CreateFile(string fileName, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(string folderName);

    Task Delete();

    Task DeleteFile(string fileName);

    Task DeleteFolder(string folderName);

    Task<bool> Exists();

    Task<IFile?> GetFile(string fileName);

    Task<IReadOnlyList<IFile>> GetFiles();

    Task<IReadOnlyList<IFile>> GetFiles(string searchPattern);

    Task<IFolder?> GetFolder(string folderName);

    Task<IReadOnlyList<IFolder>> GetFolders();

    Task<IReadOnlyList<IFolder>> GetFolders(string searchPattern);

    Task<IFileSystemWatcher> GetWatcher(string filter = "*",
                                        bool includeSubFolders = false,
                                        NotifyFilters notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

}