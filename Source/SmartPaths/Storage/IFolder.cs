namespace SmartPaths.Storage;

public interface IFolder
{

    string Name { get; }

    AbsoluteFolderPath Path { get; }

    Task<IFile> CreateFile(string name, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(string name);

    Task Delete();

    Task<bool> Exists();

    Task<IFile?> GetFile(string name);

    Task<IReadOnlyList<IFile>> GetFiles();

    Task<IFolder?> GetFolder(string name);

    Task<IReadOnlyList<IFolder>> GetFolders();

}