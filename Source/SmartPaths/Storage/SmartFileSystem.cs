using System.Text;

namespace SmartPaths.Storage;

public abstract class SmartFileSystem<TFolder, TFile, TWatcher> : IFileSystem
    where TFolder : SmartFolder<TFolder, TFile>
    where TFile : SmartFile<TFolder, TFile>
    where TWatcher : class, IFileSystemWatcher
{

    public abstract AbsoluteFolderPath AppLocalStoragePath { get; }

    public abstract AbsoluteFolderPath AppRoamingStoragePath { get; }

    public abstract AbsoluteFolderPath TempStoragePath { get; }

    public abstract AbsoluteFolderPath WorkingDirectory { get; set; }

    public abstract Task<TFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    public async Task<TFile> CreateFile(AbsoluteFilePath filePath, byte[] data, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        TFile file = await CreateFile(filePath, collisionStrategy);
        file.Data = data;
        return file;
    }

    public Task<TFile> CreateFile(AbsoluteFilePath filePath, string content, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(filePath, content, Encoding.Default, collisionStrategy);
    }

    public Task<TFile> CreateFile(AbsoluteFilePath filePath, string content, Encoding encoding, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        byte[] bytes = encoding.GetBytes(content);
        return CreateFile(filePath, bytes, collisionStrategy);
    }

    public Task<TFile> CreateFile(RelativeFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(WorkingDirectory / filePath, collisionStrategy);
    }

    public async Task<TFile> CreateFile(RelativeFilePath filePath, byte[] data, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        TFile file = await CreateFile(filePath, collisionStrategy);
        file.Data = data;
        return file;
    }

    public Task<TFile> CreateFile(RelativeFilePath filePath, string content, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(filePath, content, Encoding.Default, collisionStrategy);
    }

    public Task<TFile> CreateFile(RelativeFilePath filePath, string content, Encoding encoding, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        byte[] bytes = encoding.GetBytes(content);
        return CreateFile(filePath, bytes, collisionStrategy);
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

    public async Task<IReadOnlyList<TFile>> GetFiles(AbsoluteFolderPath folderPath) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFiles();
    }

    public async Task<IReadOnlyList<TFile>> GetFiles(AbsoluteFolderPath folderPath, string searchPattern) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFiles(searchPattern);
    }

    public async Task<IReadOnlyList<TFile>> GetFiles(RelativeFolderPath folderPath) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFiles();
    }

    public async Task<IReadOnlyList<TFile>> GetFiles(RelativeFolderPath folderPath, string searchPattern) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFiles(searchPattern);
    }

    public abstract Task<TFolder?> GetFolder(AbsoluteFolderPath folderPath);

    public Task<TFolder?> GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(WorkingDirectory / relativeFolder);
    }

    public async Task<IReadOnlyList<TFolder>> GetFolders(AbsoluteFolderPath folderPath) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFolders();
    }

    public async Task<IReadOnlyList<TFolder>> GetFolders(AbsoluteFolderPath folderPath, string searchPattern) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFolders(searchPattern);
    }

    public async Task<IReadOnlyList<TFolder>> GetFolders(RelativeFolderPath folderPath) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFolders();
    }

    public async Task<IReadOnlyList<TFolder>> GetFolders(RelativeFolderPath folderPath, string searchPattern) {
        TFolder? folder = await GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        return await folder.GetFolders(searchPattern);
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
        return CreateFile(filePath, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath filePath, byte[] data, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, data, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath filePath, string content, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, content, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(AbsoluteFilePath filePath, string content, Encoding encoding, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, content, encoding, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(RelativeFilePath filePath, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(RelativeFilePath filePath, byte[] data, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, data, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(RelativeFilePath filePath, string content, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, content, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFile> IFileSystem.CreateFile(RelativeFilePath filePath, string content, Encoding encoding, CollisionStrategy collisionStrategy) {
        return CreateFile(filePath, content, encoding, collisionStrategy).ContinueWith(IFile (task) => task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.CreateFolder(AbsoluteFolderPath folderPath) {
        return CreateFolder(folderPath).ContinueWith(IFolder (task) => task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.CreateFolder(RelativeFolderPath folderPath) {
        return CreateFolder(folderPath).ContinueWith(IFolder (task) => task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetAppLocalStorage() {
        return GetAppLocalStorage().ContinueWith(IFolder (task) => task.Result, ContinuationOptions);
    }

    Task<IFolder> IFileSystem.GetAppRoamingStorage() {
        return GetAppRoamingStorage().ContinueWith(IFolder (task) => task.Result, ContinuationOptions);
    }

    Task<IFile?> IFileSystem.GetFile(AbsoluteFilePath filePath) {
        return GetFile(filePath).ContinueWith(IFile? (task) => task.Result, ContinuationOptions);
    }

    Task<IFile?> IFileSystem.GetFile(RelativeFilePath relativeFile) {
        return GetFile(relativeFile).ContinueWith(IFile? (task) => task.Result, ContinuationOptions);
    }

    Task<IReadOnlyList<IFile>> IFileSystem.GetFiles(AbsoluteFolderPath folderPath) {
        return GetFiles(folderPath).ContinueWith(IReadOnlyList<IFile> (task) => task.Result);
    }

    Task<IReadOnlyList<IFile>> IFileSystem.GetFiles(AbsoluteFolderPath folderPath, string searchPattern) {
        return GetFiles(folderPath, searchPattern).ContinueWith(IReadOnlyList<IFile> (task) => task.Result);
    }

    Task<IReadOnlyList<IFile>> IFileSystem.GetFiles(RelativeFolderPath folderPath) {
        return GetFiles(folderPath).ContinueWith(IReadOnlyList<IFile> (task) => task.Result);
    }

    Task<IReadOnlyList<IFile>> IFileSystem.GetFiles(RelativeFolderPath folderPath, string searchPattern) {
        return GetFiles(folderPath, searchPattern).ContinueWith(IReadOnlyList<IFile> (task) => task.Result);
    }

    Task<IFolder?> IFileSystem.GetFolder(AbsoluteFolderPath folderPath) {
        return GetFolder(folderPath).ContinueWith(IFolder? (task) => task.Result, ContinuationOptions);
    }

    Task<IFolder?> IFileSystem.GetFolder(RelativeFolderPath relativeFolder) {
        return GetFolder(relativeFolder).ContinueWith(IFolder? (task) => task.Result, ContinuationOptions);
    }

    Task<IReadOnlyList<IFolder>> IFileSystem.GetFolders(AbsoluteFolderPath folderPath) {
        return GetFolders(folderPath).ContinueWith(IReadOnlyList<IFolder> (task) => task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFileSystem.GetFolders(AbsoluteFolderPath folderPath, string searchPattern) {
        return GetFolders(folderPath, searchPattern).ContinueWith(IReadOnlyList<IFolder> (task) => task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFileSystem.GetFolders(RelativeFolderPath folderPath) {
        return GetFolders(folderPath).ContinueWith(IReadOnlyList<IFolder> (task) => task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFileSystem.GetFolders(RelativeFolderPath folderPath, string searchPattern) {
        return GetFolders(folderPath, searchPattern).ContinueWith(IReadOnlyList<IFolder> (task) => task.Result);
    }

    Task<IFolder> IFileSystem.GetTempStorage() {
        return GetTempStorage().ContinueWith(IFolder (task) => task.Result, ContinuationOptions);
    }

    Task<IFileSystemWatcher> IFileSystem.GetWatcher(AbsoluteFolderPath folderPath, string filter, bool includeSubFolders, NotifyFilters notifyFilter) {
        return GetWatcher(folderPath, filter, includeSubFolders, notifyFilter).ContinueWith(task => (IFileSystemWatcher)task.Result, ContinuationOptions);
    }

    private const TaskContinuationOptions ContinuationOptions = TaskContinuationOptions.ExecuteSynchronously;

}