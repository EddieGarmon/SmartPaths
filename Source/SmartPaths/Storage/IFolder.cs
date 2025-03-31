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

    Task<IFolder?> GetFolder(string folderName);

    Task<IReadOnlyList<IFolder>> GetFolders();

}