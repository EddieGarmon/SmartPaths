namespace SmartPaths;

public interface IFolderPath : IPath
{

    string FolderName { get; }

}

public interface IFolderPath<out TFolderPath, out TFilePath> : IFolderPath
{

    TFilePath GetChildFilePath(string name, string extension);

    TFilePath GetChildFilePath(string fileNameWithExtension);

    TFolderPath GetChildFolderPath(string folderName);

}