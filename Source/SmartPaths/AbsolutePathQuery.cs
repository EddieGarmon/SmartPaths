namespace SmartPaths;

public class AbsolutePathQuery : BaseQuery, IAbsolutePathQuery
{

    public AbsolutePathQuery(string query)
        : base(PathType.Absolute, query) { }

    public string RootValue => Core.RootValue;

    public AbsoluteFilePath ToFilePath() {
        return new AbsoluteFilePath(Core.PathType, Core.Parts, Core.Parts.Count);
    }

    public AbsoluteFolderPath ToFolderPath() {
        return new AbsoluteFolderPath(Core.PathType, Core.Parts, Core.Parts.Count);
    }

    protected override IFilePath AsFilePath() {
        return ToFilePath();
    }

    protected override IFolderPath AsFolderPath() {
        return ToFolderPath();
    }

}