using System.IO.IsolatedStorage;

namespace SmartPaths.Storage.Isolated;

#if !NETSTANDARD2_0
public class IsolatedFileSystem : IFileSystem
{

    //https://learn.microsoft.com/en-us/dotnet/standard/io/isolated-storage
    //https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-create-files-and-directories-in-isolated-storage

    public IsolatedFileSystem(IsolatedStorageScope scope = IsolatedStorageScope.User) {
        IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
    }

    public AbsoluteFolderPath AppLocalStoragePath { get; }

    public AbsoluteFolderPath AppRoamingStoragePath { get; }

    public AbsoluteFolderPath TempStoragePath { get; }

    public AbsoluteFolderPath WorkingDirectory { get; set; }

    public Task<IFile> CreateFile(AbsoluteFilePath absoluteFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        throw new NotImplementedException();
    }

    public Task<IFile> CreateFile(RelativeFilePath relativeFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        throw new NotImplementedException();
    }

    public Task<IFolder> CreateFolder(AbsoluteFolderPath absoluteFolder) {
        throw new NotImplementedException();
    }

    public Task<IFolder> CreateFolder(RelativeFolderPath relativeFolder) {
        throw new NotImplementedException();
    }

    public Task DeleteFile(AbsoluteFilePath absoluteFile) {
        throw new NotImplementedException();
    }

    public Task DeleteFile(RelativeFilePath relativeFile) {
        throw new NotImplementedException();
    }

    public Task DeleteFolder(AbsoluteFolderPath absoluteFolder) {
        throw new NotImplementedException();
    }

    public Task DeleteFolder(RelativeFolderPath relativeFolder) {
        throw new NotImplementedException();
    }

    public Task<bool> FileExists(AbsoluteFilePath absoluteFile) {
        throw new NotImplementedException();
    }

    public Task<bool> FileExists(RelativeFilePath relativeFile) {
        throw new NotImplementedException();
    }

    public Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder) {
        throw new NotImplementedException();
    }

    public Task<bool> FolderExists(RelativeFolderPath relativeFolder) {
        throw new NotImplementedException();
    }

    public Task<IFolder> GetAppLocalStorage() {
        throw new NotImplementedException();
    }

    public Task<IFolder> GetAppRoamingStorage() {
        throw new NotImplementedException();
    }

    public Task<IFile?> GetFile(AbsoluteFilePath absoluteFile) {
        throw new NotImplementedException();
    }

    public Task<IFile?> GetFile(RelativeFilePath relativeFile) {
        throw new NotImplementedException();
    }

    public Task<IFolder?> GetFolder(AbsoluteFolderPath absoluteFolder) {
        throw new NotImplementedException();
    }

    public Task<IFolder?> GetFolder(RelativeFolderPath relativeFolder) {
        throw new NotImplementedException();
    }

    public Task<IFolder> GetTempStorage() {
        throw new NotImplementedException();
    }

}
#endif