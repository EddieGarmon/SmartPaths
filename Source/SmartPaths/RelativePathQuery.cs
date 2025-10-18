namespace SmartPaths;

public class RelativePathQuery : BaseQuery, IRelativePathQuery
{

    public RelativePathQuery(string query)
        : base(PathType.Relative, query) { }

    public RelativeFilePath ToFilePath() {
        return new RelativeFilePath(Core.PathType, Core.Parts, Core.Parts.Count);
    }

    public RelativeFolderPath ToFolderPath() {
        return new RelativeFolderPath(Core.PathType, Core.Parts, Core.Parts.Count);
    }

    protected override IFilePath AsFilePath() {
        return ToFilePath();
    }

    protected override IFolderPath AsFolderPath() {
        return ToFolderPath();
    }

}