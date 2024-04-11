using System.Collections.Concurrent;

namespace SmartPaths.Storage.Ram;

public class RamFolder : IFolder
{

    private readonly Dictionary<AbsoluteFilePath, RamFile> _files;
    private readonly RamFileSystem _fileSystem;
    private readonly ConcurrentDictionary<AbsoluteFolderPath, RamFolder> _folders;

    internal RamFolder(RamFileSystem fileSystem, AbsoluteFolderPath path) {
        _fileSystem = fileSystem;
        _folders = [];
        _files = [];
        Path = path;
        Parent = path.IsRoot ? this : _fileSystem.GetFolder(Path.Parent).Result!;
    }

    public bool IsRoot => Path.IsRoot;

    public string Name => Path.FolderName;

    public RamFolder Parent { get; }

    public AbsoluteFolderPath Path { get; }

    IFolder IFolder.Parent => Parent;

    public Task<RamFile> CreateFile(string fileName, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        if (!_files.ContainsKey(filePath)) {
            //get the folder, create the file
            //_fileSystem.CreateFolder()
            RamFile newFile = new(_fileSystem, filePath, Array.Empty<byte>(), DateTimeOffset.Now);
            Register(newFile);
            return Task.FromResult(newFile);
        }
        switch (collisionStrategy) {
            case CollisionStrategy.GenerateUniqueName:
                //while loop adding counter?
                throw new NotImplementedException();

            case CollisionStrategy.ReplaceExisting:
                //zero out content?
                throw new NotImplementedException();

            case CollisionStrategy.FailIfExists:
                return Task.FromException<RamFile>(new Exception($"A file already exists at: {filePath}"));

            default:
                throw new ArgumentOutOfRangeException(nameof(collisionStrategy));
        }
    }

    public Task<RamFolder> CreateFolder(string folderName) {
        AbsoluteFolderPath fullPath = Path.GetChildFolderPath(folderName);
        lock (_fileSystem) {
            RamFolder folder = _folders.GetOrAdd(fullPath,
                                                 path => {
                                                     RamFolder newFolder = new(_fileSystem, fullPath);
                                                     _fileSystem.Register(newFolder);
                                                     return newFolder;
                                                 });
            return Task.FromResult(folder);
        }
    }

    public Task Delete() {
        return Task.Run(() => DeleteFolderAndChildren(this));
    }

    public Task DeleteFile(string fileName) {
        AbsoluteFilePath filePath = Path.GetChildFilePath(fileName);
        if (_files.TryGetValue(filePath, out RamFile? file)) {
            _files.Remove(filePath);
            _fileSystem.Expunge(file);
        }
        return Task.CompletedTask;
    }

    public Task DeleteFolder(string folderName) {
        AbsoluteFolderPath folderPath = Path.GetChildFolderPath(folderName);
        return _folders.TryGetValue(folderPath, out RamFolder? folder) ? DeleteFolderAndChildren(folder) : Task.CompletedTask;
    }

    public Task<bool> Exists() {
        return Task.FromResult(true);
    }

    public Task<RamFile?> GetFile(string fileName) {
        _files.TryGetValue(Path.GetChildFilePath(fileName), out RamFile? file);
        return Task.FromResult<RamFile?>(file);
    }

    public Task<IReadOnlyList<RamFile>> GetFiles() {
        return Task.FromResult<IReadOnlyList<RamFile>>(_files.Values.ToList());
    }

    public Task<RamFolder?> GetFolder(string folderName) {
        _folders.TryGetValue(Path.GetChildFolderPath(folderName), out RamFolder? folder);
        return Task.FromResult<RamFolder?>(folder);
    }

    public Task<IReadOnlyList<RamFolder>> GetFolders() {
        return Task.FromResult<IReadOnlyList<RamFolder>>(_folders.Values.ToList());
    }

    internal async Task<RamFolder> EnsureFolderTree(AbsoluteFolderPath folderPath) {
        //match path parts, then create missing.
        LinkedList<string> relative = PathHelper.MakeRelative(Path, folderPath);
        if (relative.First!.Next!.Value != ".") {
            throw new Exception($"The specified path ({folderPath}) is not a child path of ({Path}).");
        }
        RamFolder folder = this;
        foreach (string name in relative.Skip(2)) {
            folder = await folder.CreateFolder(name);
        }
        return folder;
    }

    internal void ExpungeFile(RamFile file) {
        _fileSystem.Expunge(file);
        _files.Remove(file.Path);
    }

    internal void ExpungeFolder(RamFolder folder) {
        _fileSystem.Expunge(folder);
        Parent!._folders.TryRemove(folder.Path, out _);
    }

    internal void Register(RamFile newFile) {
        _fileSystem.Register(newFile);
        _files.Add(newFile.Path, newFile);
    }

    Task<IFile> IFolder.CreateFile(string fileName, CollisionStrategy collisionStrategy) {
        return CreateFile(fileName, collisionStrategy).ContinueWith(task => (IFile)task.Result);
    }

    Task<IFolder> IFolder.CreateFolder(string folderName) {
        return CreateFolder(folderName).ContinueWith(task => (IFolder)task.Result);
    }

    Task<IFile?> IFolder.GetFile(string fileName) {
        return GetFile(fileName).ContinueWith(task => (IFile?)task.Result);
    }

    Task<IReadOnlyList<IFile>> IFolder.GetFiles() {
        return Task.FromResult<IReadOnlyList<IFile>>(_files.Values.ToList());
    }

    Task<IFolder?> IFolder.GetFolder(string folderName) {
        return GetFolder(folderName).ContinueWith(task => (IFolder?)task.Result);
    }

    Task<IReadOnlyList<IFolder>> IFolder.GetFolders() {
        return Task.FromResult<IReadOnlyList<IFolder>>(_folders.Values.ToList());
    }

    private static async Task DeleteFolderAndChildren(RamFolder folder) {
        if (folder.Parent is null) {
            throw new Exception("Can not delete the root node");
        }
        //NB: use ToList() to copy the data as the collections will be changing under us
        //delete files on the way down
        foreach (RamFile file in folder._files.Values.ToList()) {
            folder.ExpungeFile(file);
        }
        //recurse
        foreach (RamFolder child in folder._folders.Values.ToList()) {
            await DeleteFolderAndChildren(child);
        }
        //delete folders on the way back up
        folder.Parent.ExpungeFolder(folder);
    }

}