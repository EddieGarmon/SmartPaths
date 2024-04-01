using System.Text;

namespace SmartPaths.Storage;

public static class IFolderExtensions
{

    public static Task<IFile> CreateFile(this IFolder folder,
                                         AbsoluteFilePath path,
                                         byte[] contents,
                                         CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return folder.CreateFile(path, collisionStrategy)
                     .ContinueWith(task => {
                                       using (Stream writer = task.Result.OpenToWrite().Result) {
                                           writer.Write(contents, 0, contents.Length);
                                       }
                                       return task.Result;
                                   });
    }

    public static Task<IFile> CreateFile(this IFolder folder,
                                         AbsoluteFilePath path,
                                         string utf8Contents,
                                         CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return CreateFile(folder, path, Encoding.UTF8.GetBytes(utf8Contents), collisionStrategy);
    }

    public static async Task<IFile> GetOrCreateFile(this IFolder folder, string name) {
        IFile? file = await folder.GetFile(name);
        return file ?? await folder.CreateFile(name);
    }

}