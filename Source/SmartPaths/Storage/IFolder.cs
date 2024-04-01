namespace SmartPaths.Storage;

public interface IFolder
{

    string Name { get; }

    AbsoluteFolderPath Path { get; }

    Task<IFile> CreateFile(string fileName, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(string folderName);

    Task Delete();

    Task<bool> Exists();

    Task<IFile?> GetFile(string name);

    Task<IReadOnlyList<IFile>> GetFiles();

    Task<IFolder?> GetFolder(string name);

    Task<IReadOnlyList<IFolder>> GetFolders();

}