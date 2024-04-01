namespace SmartPaths.Storage;

internal class FileOrFolder
{

    public FileOrFolder(IFile file) {
        File = file;
        IsFolder = false;
    }

    public FileOrFolder(IFolder folder) {
        Folder = folder;
        IsFolder = true;
    }

    public IFile? File { get; }

    public IFolder? Folder { get; }

    public bool IsFile => !IsFolder;

    public bool IsFolder { get; }

}