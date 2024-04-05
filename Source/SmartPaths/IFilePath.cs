namespace SmartPaths;

public interface IFilePath : IPath
{

    string FileExtension { get; }

    string FileName { get; }

    string FileNameWithoutExtension { get; }

}