namespace SmartPaths;

public interface IPathQuery
{

    bool IsAbsoluteQuery { get; }

    bool IsRelativeQuery { get; }

    IFilePath AsFilePath();

    IFolderPath AsFolderPath();

}