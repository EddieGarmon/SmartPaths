namespace SmartPaths;

public class AbsoluteQueryPath : BaseQuery
{

    public AbsoluteQueryPath(string query)
        : base(PathType.Absolute, query) { }

    internal AbsoluteQueryPath(PathCore core)
        : base(core) { }

    public string RootValue => Core.RootValue;

    public AbsoluteFilePath ToFilePath() {
        return new AbsoluteFilePath(Core);
    }

    public AbsoluteFolderPath ToFolderPath() {
        return new AbsoluteFolderPath(Core);
    }

    protected override IFilePath AsFilePath() {
        return ToFilePath();
    }

    protected override IFolderPath AsFolderPath() {
        return ToFolderPath();
    }

}