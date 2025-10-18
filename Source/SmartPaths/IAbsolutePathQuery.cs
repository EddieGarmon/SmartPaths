namespace SmartPaths;

public interface IAbsolutePathQuery : IPathQuery
{

    string RootValue { get; }

    AbsoluteFilePath ToFilePath();

    AbsoluteFolderPath ToFolderPath();

}