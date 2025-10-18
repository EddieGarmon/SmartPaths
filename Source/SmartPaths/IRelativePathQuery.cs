namespace SmartPaths;

public interface IRelativePathQuery : IPathQuery
{

    RelativeFilePath ToFilePath();

    RelativeFolderPath ToFolderPath();

}