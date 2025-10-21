using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public class RelativeQuery : BaseQuery
{

    public RelativeQuery(string query)
        : base(PathType.Relative, query) { }

    internal RelativeQuery(PathCore core)
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

    public static bool TryParse(string value, [NotNullWhen(true)] out RelativeQuery? path) {
        try {
            path = new RelativeQuery(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator RelativeQuery?(string? path) {
        return path is null ? null : new RelativeQuery(path);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator string?(RelativeQuery? path) {
        return path?.ToString();
    }

}