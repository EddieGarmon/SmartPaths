using System.Diagnostics;

namespace SmartPaths;

[DebuggerDisplay("{ToString()}")]
public abstract class BaseQuery : IPathQuery, IEquatable<BaseQuery>
{

    protected BaseQuery(PathType pathType, string query) {
        Core = new PathCore(pathType, query);
    }

    public bool IsAbsoluteQuery => Core.IsAbsolute;

    public bool IsRelativeQuery => Core.IsRelative;

    internal PathCore Core { get; }

    protected string ItemName => Core.ItemName;

    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (ReferenceEquals(this, obj)) {
            return true;
        }
        return obj.GetType() == GetType() && Equals((BaseQuery)obj);
    }

    public bool Equals(BaseQuery? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        return Core.Equals(other.Core);
    }

    public override int GetHashCode() {
        return Core.GetHashCode();
    }

    public override string ToString() {
        return Core.ToString(false);
    }

    protected abstract IFilePath AsFilePath();

    protected abstract IFolderPath AsFolderPath();

    IFilePath IPathQuery.AsFilePath() {
        return AsFilePath();
    }

    IFolderPath IPathQuery.AsFolderPath() {
        return AsFolderPath();
    }

    public static bool operator ==(BaseQuery? left, BaseQuery? right) {
        return Equals(left, right);
    }

    public static bool operator !=(BaseQuery? left, BaseQuery? right) {
        return !Equals(left, right);
    }

}