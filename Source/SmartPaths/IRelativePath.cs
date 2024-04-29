namespace SmartPaths;

public interface IRelativePath : IPath
{

    RelativeFolderPath? Parent { get; }

    RelativeFilePath GetSiblingFilePath(string name,
                                        string extension);

    RelativeFilePath GetSiblingFilePath(string fileNameWithExtension);

    RelativeFolderPath GetSiblingFolderPath(string folderName);

}