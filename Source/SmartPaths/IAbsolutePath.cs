namespace SmartPaths;

public interface IAbsolutePath : IPath
{

    new AbsoluteFolderPath? Parent { get; }

    string RootValue { get; }

    AbsoluteFilePath GetSiblingFilePath(string name, string extension);

    AbsoluteFilePath GetSiblingFilePath(string fileNameWithExtension);

    AbsoluteFolderPath GetSiblingFolderPath(string folderName);

}