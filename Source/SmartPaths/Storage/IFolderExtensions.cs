using System.Text;

namespace SmartPaths.Storage;

public static class IFolderExtensions
{

    public static async Task<IFile> CreateFile(this IFolder folder,
                                               AbsoluteFilePath path,
                                               byte[] contents,
                                               CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        IFile file = await folder.CreateFile(path, collisionStrategy);
        using (Stream writer = await file.OpenToWrite()) {
            await writer.WriteAsync(contents, 0, contents.Length);
        }
        return file;
    }

    public static Task<IFile> CreateFile(this IFolder folder,
                                         AbsoluteFilePath path,
                                         string utf8Contents,
                                         CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(folder, path, Encoding.UTF8.GetBytes(utf8Contents), collisionStrategy);
    }

    public static async Task<IFile> GetOrCreateFile(this IFolder parent, string name) {
        IFile? file = await parent.GetFile(name);
        return file ?? await parent.CreateFile(name);
    }

}