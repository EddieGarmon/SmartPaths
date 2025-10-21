namespace SmartPaths;

public interface IQuery
{

    bool IsAbsoluteQuery { get; }

    bool IsRelativeQuery { get; }

    IFilePath AsFilePath();

    IFolderPath AsFolderPath();

    string ToString();

}