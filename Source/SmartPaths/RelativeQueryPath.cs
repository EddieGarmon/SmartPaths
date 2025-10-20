namespace SmartPaths;

public class RelativeQueryPath : BaseQuery
{

    public RelativeQueryPath(string query)
        : base(PathType.Relative, query) { }

    internal RelativeQueryPath(PathCore core)
        : base(core) { }

    public RelativeFilePath ToFilePath() {
        return new RelativeFilePath(Core);
    }

    public RelativeFolderPath ToFolderPath() {
        return new RelativeFolderPath(Core);
    }

    protected override IFilePath AsFilePath() {
        return ToFilePath();
    }

    protected override IFolderPath AsFolderPath() {
        return ToFolderPath();
    }

}