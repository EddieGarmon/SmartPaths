namespace SmartPaths.Storage;

public interface IFileSystem
{

    AbsoluteFolderPath AppLocalStoragePath { get; }

    AbsoluteFolderPath AppRoamingStoragePath { get; }

    AbsoluteFolderPath TempStoragePath { get; }

    Task<IFile> CreateFile(AbsoluteFilePath path, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(AbsoluteFolderPath path);

    Task DeleteFile(AbsoluteFilePath path);

    Task DeleteFolder(AbsoluteFolderPath path);

    Task<bool> FileExists(AbsoluteFilePath path);

    Task<bool> FolderExists(AbsoluteFolderPath path);

    Task<IFolder> GetAppLocalStorage();

    Task<IFolder> GetAppRoamingStorage();

    Task<IFile?> GetFile(AbsoluteFilePath path);

    Task<IFolder?> GetFolder(AbsoluteFolderPath folderPath);

    Task<IFolder> GetTempStorage();

}