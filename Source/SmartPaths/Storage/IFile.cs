namespace SmartPaths.Storage;

public interface IFile
{

    string Name { get; }

    IFolder Parent { get; }

    AbsoluteFilePath Path { get; }

    Task Delete();

    Task<bool> Exists();

    Task<DateTimeOffset> GetLastWriteTime();

    Task<IFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    Task<Stream> OpenToAppend();

    Task<Stream> OpenToRead();

    Task<Stream> OpenToWrite();

    Task Touch();

}