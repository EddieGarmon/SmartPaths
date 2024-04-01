namespace SmartPaths.Storage;

public interface IFile
{

    string Name { get; }

    AbsoluteFilePath Path { get; }

    Task Delete();

    Task<bool> Exists();

    Task<DateTimeOffset> GetLastWriteTime();

    Task<IFile> Move(AbsoluteFilePath absolutePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<Stream> OpenToAppend();

    Task<Stream> OpenToRead();

    Task<Stream> OpenToWrite();

    Task<IFile> Rename(string newFilenameWithExtension, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task Touch();

}