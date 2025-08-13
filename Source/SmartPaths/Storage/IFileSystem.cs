using System.Text;

namespace SmartPaths.Storage;

public interface IFileSystem
{

    AbsoluteFolderPath AppLocalStoragePath { get; }

    AbsoluteFolderPath AppRoamingStoragePath { get; }

    AbsoluteFolderPath TempStoragePath { get; }

    AbsoluteFolderPath WorkingDirectory { get; set; }

    Task<IFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(AbsoluteFilePath filePath, byte[] data, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(AbsoluteFilePath filePath, string content, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(AbsoluteFilePath filePath, string content, Encoding encoding, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath filePath, byte[] data, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath filePath, string content, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<IFile> CreateFile(RelativeFilePath filePath, string content, Encoding encoding, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

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

    Task<IReadOnlyList<IFile>> GetFiles(AbsoluteFolderPath folderPath);

    Task<IReadOnlyList<IFile>> GetFiles(AbsoluteFolderPath folderPath, string searchPattern);

    Task<IReadOnlyList<IFile>> GetFiles(RelativeFolderPath folderPath);

    Task<IReadOnlyList<IFile>> GetFiles(RelativeFolderPath folderPath, string searchPattern);

    Task<IFolder?> GetFolder(AbsoluteFolderPath folderPath);

    Task<IFolder?> GetFolder(RelativeFolderPath folderPath);

    Task<IReadOnlyList<IFolder>> GetFolders(AbsoluteFolderPath folderPath);

    Task<IReadOnlyList<IFolder>> GetFolders(AbsoluteFolderPath folderPath, string searchPattern);

    Task<IReadOnlyList<IFolder>> GetFolders(RelativeFolderPath folderPath);

    Task<IReadOnlyList<IFolder>> GetFolders(RelativeFolderPath folderPath, string searchPattern);

    Task<IFolder> GetTempStorage();

    Task<IFileSystemWatcher> GetWatcher(AbsoluteFolderPath folderPath,
                                        string filter = "*",
                                        bool includeSubFolders = false,
                                        NotifyFilters notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

}