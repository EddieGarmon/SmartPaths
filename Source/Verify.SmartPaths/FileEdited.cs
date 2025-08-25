using SmartPaths.Storage;

namespace SmartPaths;

internal class FileEdited
{

    public FileEdited(IFileSystem fileSystem, AbsoluteFilePath path) {
        FileSystem = fileSystem;
        Path = path;
    }

    public AbsoluteFilePath Path { get; }

    private IFileSystem FileSystem { get; }

    public async Task<Stream> GetContents() {
        IFile? file = await FileSystem.GetFile(Path);
        if (file is null) {
            return Stream.Null;
        }
        return await file.OpenToRead();
    }

}