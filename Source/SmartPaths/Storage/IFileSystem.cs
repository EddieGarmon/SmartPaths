namespace SmartPaths.Storage;

public interface IFileSystem
{

    Task<IFolder> AppLocalStorage { get; }

    AbsoluteFolderPath AppLocalStoragePath { get; }

    Task<IFolder> AppRoamingStorage { get; }

    AbsoluteFolderPath AppRoamingStoragePath { get; }

    Task<IFolder> TempStorage { get; }

    AbsoluteFolderPath TempStoragePath { get; }

    Task<IFile> CreateFile(AbsoluteFilePath path, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(AbsoluteFolderPath path);

    Task DeleteFile(AbsoluteFilePath path);

    Task DeleteFolder(AbsoluteFolderPath path);

    Task<IFile?> GetFile(AbsoluteFilePath path);

    Task<IFolder?> GetFolder(AbsoluteFolderPath path);

}