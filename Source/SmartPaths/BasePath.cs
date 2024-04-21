using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartPaths;

/// <summary>Directories will always end with a 'PathSeparator' and files will not.</summary>
[DebuggerDisplay("{ToString()}")]
public abstract class BasePath : IPath, IEquatable<BasePath>
{

    private string? _toString;

    protected BasePath(PathType pathType, bool isFolder, string path) {
        PathType = pathType;
        IsFolderPath = isFolder;
        Parts = new LinkedList<string>();

        // should we trim whitespace?
        path = path.Trim();

        // pre validate input
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            throw new Exception("Invalid characters found: " + path);
        }

        if (IsFilePath) {
            if (path.LastIndexOfAny(['\\', '/']) == path.Length - 1) {
                throw new Exception("No filename specified: " + path);
            }
        }

        //Match and strip the root
        (PathType matchType, Match match) = PathPatterns.DeterminePathType(path);
        if (!matchType.HasFlag(pathType)) {
            throw PathExceptions.TypeMismatch(pathType, matchType);
        }
        //set the more specific path type
        PathType = matchType;
        switch (matchType) {
            case PathType.Relative:
                Parts.AddFirst(string.Empty);
                break;
            case PathType.RootRelative:
                Parts.AddFirst($"{Path.DirectorySeparatorChar}");
                path = path.Substring(1);
                break;
            case PathType.DriveLetter:
                Parts.AddFirst($@"{match.Groups[1].Value}:\");
                path = match.Groups[2].Value;
                break;
            case PathType.RamDrive:
                Parts.AddFirst(@"ram:\");
                path = match.Groups[2].Value;
                break;
            case PathType.NetworkShare:
                Parts.AddFirst($@"{match.Groups[1].Value}\");
                path = match.Groups[2].Value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(path));
        }

        //parse remaining parts now that the root has been stripped
        ReadOnlySpan<char> span = path.AsSpan();
        int nextSegmentStart = 0;
        for (int i = 0; i < path.Length; i++) {
            char @char = span[i];
            if (@char != '\\' && @char != '/') {
                continue;
            }

            // add a segment if length is > 0;
            int length = i - nextSegmentStart;
            if (length > 0) {
                Parts.AddLast(span.Slice(nextSegmentStart, length).ToString());
            }

            nextSegmentStart = i + 1;
        }

        if (nextSegmentStart < path.Length) {
            Parts.AddLast(span.Slice(nextSegmentStart).ToString());
        }

        CleanUpRoute();
    }

    protected BasePath(PathType pathType, bool isFolder, IEnumerable<string> parts, int partsLength, string? newItemName = null) {
        PathType = pathType;
        IsFolderPath = isFolder;
        Parts = new LinkedList<string>(parts.Take(partsLength));

        if (newItemName is not null) {
            Parts.AddLast(newItemName);
        }

        CleanUpRoute();
    }

    public abstract bool HasParent { get; }

    /// <summary>Gets a value indicating whether this instance is an absolute path.</summary>
    /// <value><c>true</c> if this instance is absolute path; otherwise, <c>false</c>.</value>
    public bool IsAbsolutePath => PathType.HasFlag(PathType.Absolute);

    public bool IsFilePath => !IsFolderPath;

    public bool IsFolderPath { get; }

    public bool IsRelativePath => PathType.HasFlag(PathType.Relative);

    public PathType PathType { get; }

    public string RootValue => Parts.First!.Value;

    protected bool IsRootedPath {
        get {
            switch (PathType) {
                case PathType.Relative:
                    return false;

                case PathType.RootRelative:
                case PathType.Absolute:
                case PathType.DriveLetter:
                case PathType.RamDrive:
                case PathType.NetworkShare:
                    return true;

                case PathType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    protected string ItemName => Parts.Last!.Value;

    /// <summary>
    ///     A list containing all the individual parts of the path. <br /> The root is
    ///     always stored in the first segment. If that segment is <see cref="string.Empty" />,
    ///     it is a relative path.
    /// </summary>
    protected internal LinkedList<string> Parts { get; }

    protected internal IEnumerable<string> PartsAfterRoot => Parts.Skip(1);

    public bool Equals(BasePath? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        return PathType == other.PathType && IsFolderPath == other.IsFolderPath && Parts.SequenceEqual(other.Parts);
    }

    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (ReferenceEquals(this, obj)) {
            return true;
        }
        return obj.GetType() == GetType() && Equals((BasePath)obj);
    }

    /// <summary>Returns a hash code for this instance.</summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data
    ///     structures like a hash table.
    /// </returns>
    public override int GetHashCode() {
        int hashCode = HashCode.Combine((int)PathType, IsFolderPath, ToString());
        return hashCode;
    }

    /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() {
        if (_toString is not null) {
            return _toString;
        }
        if (Parts.Count == 1) {
            //NB: This is a root folder
            _toString = Parts.First!.Value;
        } else {
            StringBuilder builder = new();
            builder.Append(Parts.First!.Value); // append the root
            //NB: do not add a separator between root and first segment
            builder.Append(Parts.First!.Next!.Value); // append the first segment
            foreach (string value in Parts.Skip(2)) {
                builder.Append(Path.DirectorySeparatorChar);
                builder.Append(value);
            }

            if (IsFolderPath) {
                builder.Append(Path.DirectorySeparatorChar);
            }

            _toString = builder.ToString();
        }

        return _toString;
    }

    protected abstract IFolderPath? GetParent();

    private void CleanUpRoute() {
        if (Parts.Count == 1) {
            if (IsFolderPath && IsRootedPath) {
                //a single root folder is allowed.
                return;
            }
            throw new Exception("Empty path is not valid.");
        }

        NameHelper.EnsureOnlyValidCharacters(Parts);

        // NB: process all inner special directories of the route segments
        // ""	-> remove empty segments
        // "."	-> reference to current folder, remove unless it is the first segment of a relative path
        // ".."	-> reference parent folder, remove this and parent folder, as long as the parent is not first segment or special too
        LinkedListNode<string>? node = Parts.First!.Next;
        while (node is not null) {
            LinkedListNode<string> previous = node.Previous!;
            LinkedListNode<string>? next = node.Next;
            switch (node.Value) {
                case "":
                    Parts.Remove(node);
                    node = next;
                    break;

                case ".":
                    if (previous == Parts.First) {
                        node = next;
                        break;
                    }

                    Parts.Remove(node);
                    node = next;
                    break;

                case "..":
                    if (previous == Parts.First) {
                        node = next;
                        break;
                    }

                    if (previous.Value == ".") {
                        Parts.Remove(previous);
                        node = next;
                        break;
                    }

                    if (previous.Value == "..") {
                        node = next;
                        break;
                    }

                    Parts.Remove(previous);
                    Parts.Remove(node);
                    node = next;
                    break;

                default:
                    node = next;
                    break;
            }
        }

        // re-validate cleaned up input
        // .\ and ..\ required for non-rooted relative paths: part[1]
        // .\ and ..\ illegal for all rooted paths: path[1]
        if (IsRootedPath && Parts.Count > 1 && PathHelper.IsRelativeSpecialPart(Parts.First!.Next!.Value)) {
            throw new Exception("Not a valid rooted path.");
        }
        if (PathType == PathType.Relative && !PathHelper.IsRelativeSpecialPart(Parts.First!.Next!.Value)) {
            //normalize to current directory relative
            Parts.AddAfter(Parts.First, ".");
        }
        // ensure filename on file path
        if (IsFilePath &&
            ((IsAbsolutePath && Parts.Count == 1) ||
             (PathType == PathType.RootRelative && Parts.Count == 1) ||
             (PathType == PathType.Relative && Parts.Count == 2))) {
            throw new Exception("No file name specified.");
        }
    }

    IFolderPath? IPath.GetParent() {
        return GetParent();
    }

    public static bool operator ==(BasePath? left, BasePath? right) {
        return Equals(left, right);
    }

    /// <summary>
    ///     Performs an implicit conversion from <see cref="BasePath" /> to
    ///     <see cref="string" />.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator string(BasePath path) {
        return path.ToString();
    }

    public static bool operator !=(BasePath? left, BasePath? right) {
        return !Equals(left, right);
    }

}