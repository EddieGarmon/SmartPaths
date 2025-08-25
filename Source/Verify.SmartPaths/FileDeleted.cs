namespace SmartPaths;

internal class FileDeleted
{

    public FileDeleted(AbsoluteFilePath path) {
        Path = path;
    }

    public AbsoluteFilePath Path { get; }

}