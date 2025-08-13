using System.Collections.Concurrent;
using System.Diagnostics;

namespace SmartPaths.Storage;

[DebuggerDisplay("[Folder] {Path}")]
public sealed class DiskFolder : SmartFolder<DiskFolder, DiskFile>
{

    internal DiskFolder(AbsoluteFolderPath path)
        : base(path) {
        Parent = IsRoot ? null : _cache.GetOrAdd(Path.Parent, p => new DiskFolder(p));
    }

    public override Task Delete() {
        return Task.Run(() => {
                            if (Directory.Exists(Path)) {
                                Directory.Delete(Path, true);
                            }
                        });
    }

    public override Task<bool> Exists() {
        return Task.FromResult(Directory.Exists(Path));
    }

    public override Task<IReadOnlyList<DiskFile>> GetFiles() {
        AssertExists();
        IReadOnlyList<DiskFile> result = Directory.GetFiles(Path).Select(filePath => new DiskFile(filePath)).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public override Task<IReadOnlyList<DiskFolder>> GetFolders() {
        AssertExists();
        IReadOnlyList<DiskFolder> result = Directory.GetDirectories(Path).Select(child => new DiskFolder(child)).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    //todo: this should be the sole implementation of collision strategy for Disk
    internal override Task<DiskFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy) {
        AssertExists();
        if (File.Exists(filePath)) {
            switch (collisionStrategy) {
                case CollisionStrategy.GenerateUniqueName:
                    filePath = MakeUnique(filePath);
                    break;
                case CollisionStrategy.ReplaceExisting:
                    File.Delete(filePath);
                    break;
                case CollisionStrategy.FailIfExists:
                    throw StorageExceptions.FileExists(filePath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
            }
        }
        using (File.Create(filePath)) { }
        return Task.FromResult(new DiskFile(filePath));
    }

    internal override Task<DiskFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        if (folderPath.Parent != Path) {
            throw new Exception($"Expected paths don't match.\nThis Folder: {Path}\nNew Folder: {folderPath}");
        }

        AssertExists();
        Directory.CreateDirectory(folderPath);
        DiskFolder? folder = _cache.GetOrAdd(folderPath, path => new DiskFolder(path));
        return Task.FromResult(folder);
    }

    internal override void Expunge(SmartFile<DiskFolder, DiskFile> file, bool notify) {
        if (File.Exists(file.Path)) {
            File.Delete(file.Path);
        }
    }

    internal override Task<DiskFile?> GetFile(AbsoluteFilePath filePath) {
        AssertExists();
        DiskFile? result = null;
        if (File.Exists(filePath)) {
            result = new DiskFile(filePath);
        }
        return Task.FromResult(result);
    }

    internal override Task<DiskFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        AssertExists();
        DiskFolder? result = null;
        if (Directory.Exists(folderPath)) {
            result = new DiskFolder(folderPath);
        }
        return Task.FromResult(result);
    }

    internal AbsoluteFilePath MakeUnique(AbsoluteFilePath filePath) {
        int extra = 2;
        while (true) {
            string alternateName = $"{filePath.FileNameWithoutExtension} ({extra}).{filePath.FileExtension}";
            AbsoluteFilePath alternatePath = Path.GetSiblingFilePath(alternateName);
            if (!File.Exists(alternatePath)) {
                return alternatePath;
            }

            extra++;
        }
    }

    private void AssertExists() {
        if (!Directory.Exists(Path)) {
            throw StorageExceptions.FolderMissing(Path);
        }
    }

    private static readonly ConcurrentDictionary<AbsoluteFolderPath, DiskFolder> _cache = [];

}