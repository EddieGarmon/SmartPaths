using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[File] {Path}")]
public sealed class DiskFile : IFile
{

    public DiskFile(AbsoluteFilePath path) {
        Path = path;
    }

    public string Name => Path.FileName;

    public DiskFolder Parent => new(Path.Parent);

    public AbsoluteFilePath Path { get; }

    IFolder IFile.Parent => Parent;

    public Task Delete() {
        return Task.Run(() => File.Delete(Path));
    }

    public Task<bool> Exists() {
        return Task.Run(() => File.Exists(Path));
    }

    public Task<DateTimeOffset> GetLastWriteTime() {
        AssertExists();
        return Task.Run(() => new DateTimeOffset(File.GetLastWriteTime(Path)));
    }

    public Task<DiskFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(newPath);
        AssertExists();

        if (File.Exists(newPath)) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newPath = MakeUnique(newPath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    File.Delete(newPath);
                    break;
                case CollisionStrategy.FailIfExists:
                    throw StorageExceptions.FileExists(newPath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
            }
        }

        File.Move(Path, newPath);
        return Task.FromResult(new DiskFile(newPath));
    }

    public Task<Stream> OpenToAppend() {
        AssertExists();
        return Task.Run<Stream>(() => File.Open(Path, FileMode.Append));
    }

    public Task<Stream> OpenToRead() {
        AssertExists();
        return Task.Run<Stream>(() => File.Open(Path, FileMode.Open, FileAccess.Read));
    }

    public Task<Stream> OpenToWrite() {
        AssertExists();
        return Task.Run<Stream>(() => File.Open(Path, FileMode.Open, FileAccess.ReadWrite));
    }

    public Task Touch() {
        AssertExists();
        return Task.Run(() => File.SetLastWriteTime(Path, DateTime.Now));
    }

    private void AssertExists() {
        if (!File.Exists(Path)) {
            throw StorageExceptions.FileMissing(Path);
        }
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

    internal static AbsoluteFilePath MakeUnique(AbsoluteFilePath path) {
        int extra = 2;
        while (true) {
            string alternateName = string.Format("{0} ({1}).{2}", path.FileNameWithoutExtension, extra, path.FileExtension);
            AbsoluteFilePath alternatePath = path.GetSiblingFilePath(alternateName);
            if (!File.Exists(alternatePath)) {
                return alternatePath;
            }

            extra++;
        }
    }

}