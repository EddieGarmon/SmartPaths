namespace SmartPaths.Storage;

public abstract class BaseFileSystem<TFolder, TFile> : IFileSystem
    where TFolder : IFolder
    where TFile : IFile
{

    public abstract AbsoluteFolderPath AppLocalStoragePath { get; }

    public abstract AbsoluteFolderPath AppRoamingStoragePath { get; }

    public abstract AbsoluteFolderPath TempStoragePath { get; }

    public abstract AbsoluteFolderPath WorkingDirectory { get; set; }

    public abstract Task<TFile> CreateFile(AbsoluteFilePath absoluteFile,
                                           CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    public Task<TFile> CreateFile(RelativeFilePath relativeFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(WorkingDirectory / relativeFile, collisionStrategy);
    }

    public abstract Task<TFolder> CreateFolder(AbsoluteFolderPath absoluteFolder);

    public Task<TFolder> CreateFolder(RelativeFolderPath relativeFolder) {
        return CreateFolder(WorkingDirectory / relativeFolder);
    }

    public abstract Task DeleteFile(AbsoluteFilePath absoluteFile);

    public Task DeleteFile(RelativeFilePath relativeFile) {
        return DeleteFile(WorkingDirectory / relativeFile);
    }

    public abstract Task DeleteFolder(AbsoluteFolderPath absoluteFolder);

    public Task DeleteFolder(RelativeFolderPath relativeFolder) {
        return DeleteFolder(WorkingDirectory / relativeFolder);
    }

    public abstract Task<bool> FileExists(AbsoluteFilePath absoluteFile);

    public Task<bool> FileExists(RelativeFilePath relativeFile) {
        return FileExists(WorkingDirectory / relativeFile);
    }

    public abstract Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder);

    public Task<bool> FolderExists(RelativeFolderPath relativeFolder) {
        return FolderExists(WorkingDirectory / relativeFolder);
    }

    public abstract Task<TFolder> GetAppLocalStorage();

    public abstract Task<TFolder> GetAppRoamingStorage();

    public abstract Task<TFile?> GetFile(AbsoluteFilePath absoluteFile);

    public Task<TFile?> GetFile(RelativeFilePath relativeFile) {
        return GetFile(WorkingDirectory / relativeFile);
    }

    public abstract Task<TFolder?> GetFolder(AbsoluteFolderPath absoluteFolder);

    public Task<TFolder?> GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(WorkingDirectory / relativeFolder);
    }

    public abstract Task<TFolder> GetTempStorage();

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath absoluteFile, CollisionStrategy collisionStrategy) {
        return CreateFile(absoluteFile, collisionStrategy).ContinueWith(task => (IFile)task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(RelativeFilePath relativeFile, CollisionStrategy collisionStrategy) {
        return CreateFile(relativeFile, collisionStrategy).ContinueWith(task => (IFile)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.CreateFolder(AbsoluteFolderPath absoluteFolder) {
        return CreateFolder(absoluteFolder).ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.CreateFolder(RelativeFolderPath relativeFolder) {
        return CreateFolder(relativeFolder).ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetAppLocalStorage() {
        return GetAppLocalStorage().ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetAppRoamingStorage() {
        return GetAppRoamingStorage().ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    Task<IFile?> IFileSystem.GetFile(AbsoluteFilePath absoluteFile) {
        return GetFile(absoluteFile).ContinueWith(task => (IFile?)task.Result, ContinuationOptions);
    }

    Task<IFile?> IFileSystem.GetFile(RelativeFilePath relativeFile) {
        return GetFile(relativeFile).ContinueWith(task => (IFile?)task.Result, ContinuationOptions);
    }

    Task<IFolder?> IFileSystem.GetFolder(AbsoluteFolderPath absoluteFolder) {
        return GetFolder(absoluteFolder).ContinueWith(task => (IFolder?)task.Result, ContinuationOptions);
    }

    Task<IFolder?> IFileSystem.GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(relativeFolder).ContinueWith(task => (IFolder?)task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetTempStorage() {
        return GetTempStorage().ContinueWith(task => (IFolder)task.Result, ContinuationOptions);
    }

    private const TaskContinuationOptions ContinuationOptions =
        TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously;

}