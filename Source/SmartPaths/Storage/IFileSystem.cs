namespace SmartPaths.Storage;

public interface IFileSystem
{

    IFolder AppLocalStorage { get; }

    IFolder AppRoamingStorage { get; }

    IFolder TempStorage { get; }

    Task<IFile> CreateFile(AbsoluteFilePath path);

    Task<IFolder> CreateFolder(AbsoluteFolderPath path);

    Task<IFile?> GetFile(AbsoluteFilePath path);

    Task<IFolder?> GetFolder(AbsoluteFolderPath path);

}