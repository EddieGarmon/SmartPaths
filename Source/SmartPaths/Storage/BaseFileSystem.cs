namespace SmartPaths.Storage;

public abstract class BaseFileSystem<TFolder, TFile, TWatcher> : IFileSystem
    where TFolder : IFolder
    where TFile : IFile
    where TWatcher : IFileSystemWatcher
{

    public abstract AbsoluteFolderPath AppLocalStoragePath { get; }

    public abstract AbsoluteFolderPath AppRoamingStoragePath { get; }

    public abstract AbsoluteFolderPath TempStoragePath { get; }

    public abstract AbsoluteFolderPath WorkingDirectory { get; set; }

    public abstract Task<TFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    public Task<TFile> CreateFile(RelativeFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(WorkingDirectory / filePath, collisionStrategy);
    }

    public abstract Task<TFolder> CreateFolder(AbsoluteFolderPath folderPath);

    public Task<TFolder> CreateFolder(RelativeFolderPath folderPath) {
        return CreateFolder(WorkingDirectory / folderPath);
    }

    public abstract Task DeleteFile(AbsoluteFilePath filePath);

    public Task DeleteFile(RelativeFilePath filePath) {
        return DeleteFile(WorkingDirectory / filePath);
    }

    public abstract Task DeleteFolder(AbsoluteFolderPath folderPath);

    public Task DeleteFolder(RelativeFolderPath folderPath) {
        return DeleteFolder(WorkingDirectory / folderPath);
    }

    public abstract Task<bool> FileExists(AbsoluteFilePath filePath);

    public Task<bool> FileExists(RelativeFilePath filePath) {
        return FileExists(WorkingDirectory / filePath);
    }

    public abstract Task<bool> FolderExists(AbsoluteFolderPath folderPath);

    public Task<bool> FolderExists(RelativeFolderPath relativeFolder) {
        return FolderExists(WorkingDirectory / relativeFolder);
    }

    public abstract Task<TFolder> GetAppLocalStorage();

    public abstract Task<TFolder> GetAppRoamingStorage();

    public abstract Task<TFile?> GetFile(AbsoluteFilePath filePath);

    public Task<TFile?> GetFile(RelativeFilePath relativeFile) {
        return GetFile(WorkingDirectory / relativeFile);
    }

    public abstract Task<TFolder?> GetFolder(AbsoluteFolderPath folderPath);

    public Task<TFolder?> GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(WorkingDirectory / relativeFolder);
    }

    public abstract Task<TFolder> GetTempStorage();

    public abstract Task<TWatcher> GetWatcher(AbsoluteFolderPath folderPath,
                                              string filter = "*",
                                              bool includeSubFolders = false,
                                              NotifyFilters notifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite);

    protected AbsoluteFilePath UpdateName(AbsoluteFilePath source, int uniqueness) {
        return source.GetSiblingFilePath($"{source.FileNameWithoutExtension} ({uniqueness}).{source.FileExtension}");
    }

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, collisionStrategy).ContinueWith(task => (IFile)task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(RelativeFilePath filePath, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, collisionStrategy).ContinueWith(task => (IFile)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.CreateFolder(AbsoluteFolderPath folderPath) {
        return CreateFolder(folderPath).ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.CreateFolder(RelativeFolderPath folderPath) {
        return CreateFolder(folderPath).ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetAppLocalStorage() {
        return GetAppLocalStorage().ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetAppRoamingStorage() {
        return GetAppRoamingStorage().ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFile?> IFileSystem.GetFile(AbsoluteFilePath filePath) {
        return GetFile(filePath).ContinueWith(task => (IFile?)task.Result, ContinuationOptions);
    }

    Task<IFile?> IFileSystem.GetFile(RelativeFilePath relativeFile) {
        return GetFile(relativeFile).ContinueWith(task => (IFile?)task.Result, ContinuationOptions);
    }

    Task<IFolder?> IFileSystem.GetFolder(AbsoluteFolderPath folderPath) {
        return GetFolder(folderPath).ContinueWith(task => (IFolder?)task.Result, ContinuationOptions);
    }

    Task<IFolder?> IFileSystem.GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(relativeFolder).ContinueWith(task => (IFolder?)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetTempStorage() {
        return GetTempStorage().ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFileSystemWatcher> IFileSystem.GetWatcher(AbsoluteFolderPath folderPath, string filter, bool includeSubFolders, NotifyFilters notifyFilter) {
        return GetWatcher(folderPath, filter, includeSubFolders, notifyFilter).ContinueWith(task => (IFileSystemWatcher)task.Result, ContinuationOptions);
    }

    private const TaskContinuationOptions ContinuationOptions = TaskContinuationOptions.ExecuteSynchronously;

}