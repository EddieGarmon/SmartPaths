using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public class AbsoluteQuery : BaseQuery
{

    public AbsoluteQuery(string query)
        : base(PathType.Absolute, query) { }

    internal AbsoluteQuery(PathCore core)
        : base(core) { }

    public string RootValue => Core.RootValue;

    internal bool IsRoot => Core.Parts.Count == 1;

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

    public static bool TryParse(string value, [NotNullWhen(true)] out AbsoluteQuery? path) {
        try {
            path = new AbsoluteQuery(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator AbsoluteQuery?(string? path) {
        return path is null ? null : new AbsoluteQuery(path);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator string?(AbsoluteQuery? path) {
        return path?.ToString();
    }

    //todo: what operators should we implement?

}