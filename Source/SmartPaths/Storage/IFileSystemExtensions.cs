namespace SmartPaths.Storage;

//todo: really make all of these async
public static class IFileSystemExtensions
{

    public static async Task<bool> CopyFile(this IFileSystem fileSystem, AbsoluteFilePath sourcePath, AbsoluteFilePath targetPath, bool overwriteIfExists = false) {
        IFile? source = await fileSystem.GetFile(sourcePath);
        if (source is null) {
            return false;
        }
        IFolder targetFolder = await fileSystem.CreateFolder(targetPath.Folder);
        IFile? file = await targetFolder.GetFile(targetPath.FileName);
        if (file is not null && !overwriteIfExists) {
            return false;
        }
        file ??= await targetFolder.CreateFile(targetPath.FileName);
        using Stream to = await file.OpenToWrite();
        using Stream from = await file.OpenToRead();
        await from.CopyToAsync(to);

        return true;
    }

    public static Task<bool> CopyFile(this IFileSystem fileSystem, AbsoluteFilePath sourcePath, RelativeFilePath targetPath, bool overwriteIfExists = false) {
        AbsoluteFilePath absoluteTargetPath = sourcePath.Folder + targetPath;
        return CopyFile(fileSystem, sourcePath, absoluteTargetPath, overwriteIfExists);
    }

    public static async Task<IFile> CreateFile(this IFileSystem fileSystem,
                                               AbsoluteFilePath path,
                                               byte[] contents,
                                               CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        IFolder folder = await fileSystem.CreateFolder(path.Folder);
        return await folder.CreateFile(path, contents, collisionStrategy);
    }

    public static async Task<IFile> CreateFile(this IFileSystem fileSystem,
                                               AbsoluteFilePath path,
                                               string utf8Contents,
                                               CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        IFolder folder = await fileSystem.CreateFolder(path.Folder);
        return await folder.CreateFile(path, utf8Contents, collisionStrategy);
    }

    public static async Task<IEnumerable<IFile>> GetFiles(this IFileSystem filesystem, AbsoluteFolderPath folderPath, string wildcardPattern = "") {
        IFolder? folder = await filesystem.GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        IReadOnlyList<IFile> files = await folder.GetFiles();
        return wildcardPattern == string.Empty ? files : files.Filter(wildcardPattern);
    }

    public static async Task<IEnumerable<IFolder>> GetFolders(this IFileSystem filesystem, AbsoluteFolderPath folderPath, string wildcardPattern = "") {
        IFolder? folder = await filesystem.GetFolder(folderPath);
        if (folder is null) {
            return [];
        }
        IReadOnlyList<IFolder> folders = await folder.GetFolders();
        return wildcardPattern == string.Empty ? folders : folders.Filter(wildcardPattern);
    }

    public static async Task<IFile> GetOrCreateFile(this IFileSystem fileSystem, AbsoluteFilePath path) {
        IFolder folder = await fileSystem.CreateFolder(path.Folder);
        return await folder.GetOrCreateFile(path.FileName);
    }

    public static async Task<bool> MoveFile(this IFileSystem fileSystem,
                                            AbsoluteFilePath sourcePath,
                                            AbsoluteFilePath targetPath,
                                            CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        IFile? source = await fileSystem.GetFile(sourcePath);
        if (source is null) {
            return false;
        }
        await source.Move(targetPath);
        return true;
    }

    public static Task<bool> MoveFile(this IFileSystem fileSystem,
                                      AbsoluteFilePath sourcePath,
                                      RelativeFilePath targetPath,
                                      CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        AbsoluteFilePath absoluteTargetPath = sourcePath.Folder + targetPath;
        return fileSystem.MoveFile(sourcePath, absoluteTargetPath, collisionStrategy);
    }

}