namespace SmartPaths.Storage;

public interface IFileSystem
{

    AbsoluteFolderPath AppLocalStoragePath { get; }

    AbsoluteFolderPath AppRoamingStoragePath { get; }

    AbsoluteFolderPath CurrentDirectory { get; set; }

    AbsoluteFolderPath TempStoragePath { get; }

    Task<IFile> CreateFile(AbsoluteFilePath absoluteFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath relativeFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(CurrentDirectory / relativeFile, collisionStrategy);
    }

    Task<IFolder> CreateFolder(AbsoluteFolderPath absoluteFolder);

    Task<IFolder> CreateFolder(RelativeFolderPath relativeFolder) {
        return CreateFolder(CurrentDirectory / relativeFolder);
    }

    Task DeleteFile(AbsoluteFilePath absoluteFile);

    Task DeleteFile(RelativeFilePath relativeFile) {
        return DeleteFile(CurrentDirectory / relativeFile);
    }

    Task DeleteFolder(AbsoluteFolderPath absoluteFolder);

    Task DeleteFolder(RelativeFolderPath relativeFolder) {
        return DeleteFolder(CurrentDirectory / relativeFolder);
    }

    Task<bool> FileExists(AbsoluteFilePath absoluteFile);

    Task<bool> FileExists(RelativeFilePath relativeFile) {
        return FileExists(CurrentDirectory / relativeFile);
    }

    Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder);

    Task<bool> FolderExists(RelativeFolderPath relativeFolder) {
        return FolderExists(CurrentDirectory / relativeFolder);
    }

    Task<IFolder> GetAppLocalStorage();

    Task<IFolder> GetAppRoamingStorage();

    Task<IFile?> GetFile(AbsoluteFilePath absoluteFile);

    Task<IFile?> GetFile(RelativeFilePath relativeFile) {
        return GetFile(CurrentDirectory / relativeFile);
    }

    Task<IFolder?> GetFolder(AbsoluteFolderPath absoluteFolder);

    Task<IFolder?> GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(CurrentDirectory / relativeFolder);
    }

    Task<IFolder> GetTempStorage();

}