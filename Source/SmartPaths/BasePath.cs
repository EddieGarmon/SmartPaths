using System.Diagnostics;

namespace SmartPaths;

/// <summary>Directories will always end with a 'PathSeparator' and files will not.</summary>
[DebuggerDisplay("{ToString()}")]
public abstract class BasePath : IPath, IEquatable<BasePath>
{

    protected BasePath(PathType pathType, bool isFolder, string path) {
        if (!isFolder) {
            if (path.LastIndexOfAny(['\\', '/']) == path.Length - 1) {
                throw new Exception("No filename specified: " + path);
            }
        }
        IsFolderPath = isFolder;
        Core = new PathCore(pathType, path);
        EnsureFileNameExists();
    }

    protected BasePath(PathType pathType, bool isFolder, IEnumerable<string> parts, int partsLength, string? newItemName = null) {
        IsFolderPath = isFolder;
        Core = new PathCore(pathType, parts, partsLength, newItemName);
        EnsureFileNameExists();
    }

    public abstract bool HasParent { get; }

    public bool IsAbsolutePath => Core.IsAbsolute;

    public bool IsFilePath => !IsFolderPath;

    public bool IsFolderPath { get; }

    public bool IsRelativePath => Core.IsRelative;

    public PathType PathType => Core.PathType;

    internal PathCore Core { get; }

    protected string ItemName => Core.ItemName;

    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (ReferenceEquals(this, obj)) {
            return true;
        }
        return obj.GetType() == GetType() && Equals((BasePath)obj);
    }

    public bool Equals(BasePath? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        return IsFolderPath == other.IsFolderPath && Core.Equals(other.Core);
    }

    public override int GetHashCode() {
        return Core.GetHashCode();
    }

    public override string ToString() {
        return Core.ToString(IsFolderPath);
    }

    protected abstract IFolderPath? GetParent();

    private void EnsureFileNameExists() {
        // ensure filename on file path
        if (IsFilePath &&
            ((Core.IsAbsolute && Core.Parts.Count == 1) ||
             (PathType == PathType.RootRelative && Core.Parts.Count == 1) ||
             (PathType == PathType.Relative && Core.Parts.Count == 2))) {
            throw new Exception("No file name specified.");
        }
    }

    IFolderPath? IPath.GetParent() {
        return GetParent();
    }

    public static bool operator ==(BasePath? left, BasePath? right) {
        return Equals(left, right);
    }

    public static bool operator !=(BasePath? left, BasePath? right) {
        return !Equals(left, right);
    }

}