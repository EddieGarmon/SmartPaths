using System.Text;

namespace SmartPaths.Storage;

public static class IFileExtensions
{

    public static Task<IFile> Move(this IFile file, RelativeFilePath relativePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(relativePath);
        AbsoluteFilePath newPath = file.Path.Folder + relativePath;
        return file.Move(newPath, collisionStrategy);
    }

    public static async Task<byte[]> ReadAllBytes(this IFile file) {
        ArgumentNullException.ThrowIfNull(file);
        using Stream stream = await file.OpenToRead();
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public static async Task<string> ReadAllText(this IFile file, Encoding? encoding = null) {
        ArgumentNullException.ThrowIfNull(file);
        encoding ??= Encoding.UTF8;
        using Stream stream = await file.OpenToRead();
        using StreamReader reader = new(stream, encoding);
        return await reader.ReadToEndAsync();
    }

    public static Task<IFile> Rename(this IFile file, string newFilenameWithExtension, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(newFilenameWithExtension);
        AbsoluteFilePath newPath = file.Path.GetSiblingFilePath(newFilenameWithExtension);
        return file.Move(newPath, collisionStrategy);
    }

    public static async Task WriteAllBytes(this IFile file, byte[] content) {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(content);
        using Stream stream = await file.OpenToWrite();
        await stream.WriteAsync(content, 0, content.Length);
    }

    public static async Task WriteAllText(this IFile file, string content, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        byte[] buffer = encoding.GetBytes(content);
        using Stream stream = await file.OpenToWrite();
        await stream.WriteAsync(buffer, 0, buffer.Length);
    }

}