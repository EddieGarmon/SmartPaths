namespace SmartPaths;

public interface IFolderPath : IPath
{

    string FolderName { get; }

#if !NETSTANDARD2_0
    static abstract IPath operator /(IFolderPath start, IRelativePath relative);
#endif

}

public interface IFolderPath<out TFolderPath, out TFilePath> : IFolderPath
{

    TFilePath GetChildFilePath(string name, string extension);

    TFilePath GetChildFilePath(string fileNameWithExtension);

    TFolderPath GetChildFolderPath(string folderName);

}