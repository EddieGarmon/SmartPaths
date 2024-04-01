namespace SmartPaths.Storage;

//todo: really make all of these async
public static class IFileSystemExtensions
{

    public static async Task<bool> CopyFile(this IFileSystem fileSystem,
                                            AbsoluteFilePath sourcePath,
                                            AbsoluteFilePath targetPath,
                                            bool overwriteIfExists = false) {
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
        await using Stream to = await file.OpenToWrite();
        await using Stream from = await file.OpenToRead();
        await from.CopyToAsync(to);

        return true;
    }

    public static Task<bool> CopyFile(this IFileSystem fileSystem,
                                      AbsoluteFilePath sourcePath,
                                      RelativeFilePath targetPath,
                                      bool overwriteIfExists = false) {
        AbsoluteFilePath absoluteTargetPath = sourcePath.Folder + targetPath;
        return CopyFile(fileSystem, sourcePath, absoluteTargetPath, overwriteIfExists);
    }

    public static Task<IFile> CreateFile(this IFileSystem fileSystem,
                                         AbsoluteFilePath path,
                                         byte[] contents,
                                         CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return fileSystem.CreateFolder(path.Folder)
                         .ContinueWith(task => task.Result.CreateFile(path, contents, collisionStrategy))
                         .Unwrap();
    }

    public static Task<IFile> CreateFile(this IFileSystem fileSystem,
                                         AbsoluteFilePath path,
                                         string utf8Contents,
                                         CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return fileSystem.CreateFolder(path.Folder)
                         .ContinueWith(task => task.Result.CreateFile(path, utf8Contents, collisionStrategy))
                         .Unwrap();
    }

    public static Task DeleteFile(this IFileSystem fileSystem, AbsoluteFilePath filePath) {
        return fileSystem.GetFile(filePath)
                         .ContinueWith(task => {
                                           IFile file = task.Result;
                                           if (file is not null) {
                                               return file.Delete();
                                           }
                                           return Task.CompletedTask;
                                       })
                         .Unwrap();
    }

    public static Task<IFile> GetOrCreateFile(this IFileSystem fileSystem, AbsoluteFilePath path) {
        return fileSystem.CreateFolder(path.Folder).ContinueWith(task => task.Result.GetOrCreateFile(path.FileName)).Unwrap();
    }

    public static Task<bool> MoveFile(this IFileSystem fileSystem,
                                      AbsoluteFilePath sourcePath,
                                      AbsoluteFilePath targetPath,
                                      bool overwriteIfExists = false) {
        throw new NotImplementedException();
    }

    public static Task<bool> MoveFile(this IFileSystem fileSystem,
                                      AbsoluteFilePath sourcePath,
                                      RelativeFilePath targetPath,
                                      bool overwriteIfExists = false) {
        AbsoluteFilePath absoluteTargetPath = sourcePath.Folder + targetPath;
        return fileSystem.MoveFile(sourcePath, absoluteTargetPath, overwriteIfExists);
    }

}