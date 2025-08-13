using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[File] {Path}")]
public sealed class DiskFile : SmartFile<DiskFolder, DiskFile>
{

    public DiskFile(AbsoluteFilePath path)
        : base(path) {
        Folder = new DiskFolder(path.Parent);
    }

    public override Task<bool> Exists() {
        return Task.Run(() => File.Exists(Path));
    }

    public override Task<DateTimeOffset> GetLastWriteTime() {
        AssertExists();
        return Task.Run(() => new DateTimeOffset(File.GetLastWriteTime(Path)));
    }

    public override Task<DiskFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        ArgumentNullException.ThrowIfNull(newPath);
        AssertExists();

        if (File.Exists(newPath)) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    newPath = Folder.MakeUnique(newPath);
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

    public new Task<Stream> OpenToAppend() {
        AssertExists();
        return Task.Run<Stream>(() => File.Open(Path, FileMode.Append));
    }

    public new Task<Stream> OpenToRead() {
        AssertExists();
        return Task.Run<Stream>(() => File.Open(Path, FileMode.Open, FileAccess.Read));
    }

    public new Task<Stream> OpenToWrite() {
        AssertExists();
        return Task.Run<Stream>(() => File.Open(Path, FileMode.Open, FileAccess.ReadWrite));
    }

    public override Task Touch() {
        AssertExists();
        return Task.Run(() => File.SetLastWriteTime(Path, DateTime.Now));
    }

    private void AssertExists() {
        if (!File.Exists(Path)) {
            throw StorageExceptions.FileMissing(Path);
        }
    }

}