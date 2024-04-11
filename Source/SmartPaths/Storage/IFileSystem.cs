namespace SmartPaths.Storage;

public interface IFileSystem
{

    AbsoluteFolderPath AppLocalStoragePath { get; }

    AbsoluteFolderPath AppRoamingStoragePath { get; }

    AbsoluteFolderPath CurrentDirectory { get; set; }

    AbsoluteFolderPath TempStoragePath { get; }

    Task<IFile> CreateFile(AbsoluteFilePath absoluteFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath relativeFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFolder> CreateFolder(AbsoluteFolderPath absoluteFolder);

    Task<IFolder> CreateFolder(RelativeFolderPath relativeFolder);

    Task DeleteFile(AbsoluteFilePath absoluteFile);

    Task DeleteFile(RelativeFilePath relativeFile);

    Task DeleteFolder(AbsoluteFolderPath absoluteFolder);

    Task DeleteFolder(RelativeFolderPath relativeFolder);

    Task<bool> FileExists(AbsoluteFilePath absoluteFile);

    Task<bool> FileExists(RelativeFilePath relativeFile);

    Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder);

    Task<bool> FolderExists(RelativeFolderPath relativeFolder);

    Task<IFolder> GetAppLocalStorage();

    Task<IFolder> GetAppRoamingStorage();

    Task<IFile?> GetFile(AbsoluteFilePath absoluteFile);

    Task<IFile?> GetFile(RelativeFilePath relativeFile);

    Task<IFolder?> GetFolder(AbsoluteFolderPath absoluteFolder);

    Task<IFolder?> GetFolder(RelativeFolderPath relativeFolder);

    Task<IFolder> GetTempStorage();

}