namespace SmartPaths.Storage;

public interface IFileSystem
{

    AbsoluteFolderPath AppLocalStoragePath { get; }

    AbsoluteFolderPath AppRoamingStoragePath { get; }

    AbsoluteFolderPath TempStoragePath { get; }

    AbsoluteFolderPath WorkingDirectory { get; set; }

    Task<IFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(AbsoluteFolderPath folderPath);

    Task<IFolder> CreateFolder(RelativeFolderPath folderPath);

    Task DeleteFile(AbsoluteFilePath filePath);

    Task DeleteFile(RelativeFilePath filePath);

    Task DeleteFolder(AbsoluteFolderPath folderPath);

    Task DeleteFolder(RelativeFolderPath folderPath);

    Task<bool> FileExists(AbsoluteFilePath filePath);

    Task<bool> FileExists(RelativeFilePath filePath);

    Task<bool> FolderExists(AbsoluteFolderPath folderPath);

    Task<bool> FolderExists(RelativeFolderPath folderPath);

    Task<IFolder> GetAppLocalStorage();

    Task<IFolder> GetAppRoamingStorage();

    Task<IFile?> GetFile(AbsoluteFilePath filePath);

    Task<IFile?> GetFile(RelativeFilePath filePath);

    Task<IFolder?> GetFolder(AbsoluteFolderPath folderPath);

    Task<IFolder?> GetFolder(RelativeFolderPath folderPath);

    Task<IFolder> GetTempStorage();

}