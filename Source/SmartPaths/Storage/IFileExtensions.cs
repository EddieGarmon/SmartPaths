using System.Text;

namespace SmartPaths.Storage;

public static class IFileExtensions
{

    public static Task<IFile> Move(this IFile file,
                                   RelativeFilePath relativePath,
                                   CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(relativePath);
        AbsoluteFilePath newPath = file.Path.Folder + relativePath;
        return file.Move(newPath, collisionStrategy);
    }

    public static async Task<string> ReadAllText(this IFile file, Encoding? encoding = null) {
        ArgumentNullException.ThrowIfNull(file);
        encoding ??= Encoding.UTF8;
        await using Stream stream = await file.OpenToRead();
        using StreamReader reader = new(stream, encoding);
        return await reader.ReadToEndAsync();
    }

    public static Task<IFile> Rename(this IFile file,
                                     string newFilenameWithExtension,
                                     CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(newFilenameWithExtension);
        AbsoluteFilePath newPath = file.Path.GetSiblingFilePath(newFilenameWithExtension);
        return file.Move(newPath, collisionStrategy);
    }

    public static async Task WriteAllText(this IFile file, string content, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        byte[] bytes1 = encoding.GetBytes(content);
        await using Stream stream = await file.OpenToWrite();
        await stream.WriteAsync(bytes1);
    }

}