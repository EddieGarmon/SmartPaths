using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartPaths;

[DebuggerDisplay("{ToString()}")]
internal class PathCore : IEquatable<PathCore>
{

    public PathCore(string path)
        : this(PathType.Unknown, path) { }

    public PathCore(PathType pathType, string path) {
        path = path.Trim();
        ArgumentException.ThrowIfNullOrEmpty(path);

        // pre validate input
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            throw new Exception("Invalid characters found: " + path);
        }

        //Match and strip the root
        (PathType matchType, Match match) = PathPatterns.DeterminePathType(path);
        if (!matchType.HasFlag(pathType)) {
            throw PathExceptions.TypeMismatch(pathType, matchType);
        }
        //set the more specific path type
        PathType = matchType;
        Parts = [];
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
            case PathType.NetworkShare:
                Parts.AddFirst($@"{match.Groups[1].Value}\");
                path = match.Groups[2].Value;
                break;
            case PathType.Absolute:
                throw new ArgumentOutOfRangeException(nameof(pathType));
            case PathType.Unknown:
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

    internal PathCore(PathType pathType, IEnumerable<string> parts, int partsLength, string? newItemName = null) {
        PathType = pathType;
        Parts = new LinkedList<string>(parts.Take(partsLength));

        if (newItemName is not null) {
            Parts.AddLast(newItemName);
        }

        CleanUpRoute();
    }

    /// <summary>Gets a value indicating whether this instance is an absolute path.</summary>
    /// <value><c>true</c> if this instance is absolute path; otherwise, <c>false</c>.</value>
    public bool IsAbsolute => PathType.HasFlag(PathType.Absolute);

    public bool IsRelative => PathType.HasFlag(PathType.Relative);

    public bool IsRooted {
        get {
            switch (PathType) {
                case PathType.Relative:
                    return false;

                case PathType.Absolute:
                case PathType.RootRelative:
                case PathType.DriveLetter:
                case PathType.NetworkShare:
                    return true;

                case PathType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public string ItemName => Parts.Last!.Value;

    /// <summary>A list containing all the individual parts of the path. <br /> The root is always stored
    ///     in the first segment. If that segment is <see cref="string.Empty" />, it is a relative path.</summary>
    public LinkedList<string> Parts { get; }

    public IEnumerable<string> PartsAfterRoot => Parts.Skip(1);

    public PathType PathType { get; }

    public string RootValue => Parts.First!.Value;

    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (ReferenceEquals(this, obj)) {
            return true;
        }
        return obj.GetType() == GetType() && Equals((PathCore)obj);
    }

    public bool Equals(PathCore? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        return PathType == other.PathType && Parts.SequenceEqual(other.Parts);
    }

    public override int GetHashCode() {
        int hashCode = HashCode.Combine((int)PathType, ToString());
        return hashCode;
    }

    public override string ToString() {
        return ToString(false);
    }

    public string ToString(bool isFolderPath) {
        if (Parts.Count == 1) {
            //NB: This is a root folder
            return Parts.First!.Value;
        }

        StringBuilder builder = new();
        builder.Append(Parts.First!.Value); // append the root
        //NB: do not add a separator between root and first segment
        builder.Append(Parts.First!.Next!.Value); // append the first segment
        foreach (string value in Parts.Skip(2)) {
            builder.Append(Path.DirectorySeparatorChar);
            builder.Append(value);
        }

        if (isFolderPath) {
            builder.Append(Path.DirectorySeparatorChar);
        }

        return builder.ToString();
    }

    private void CleanUpRoute() {
        if (Parts.Count == 1) {
            if (IsRooted) {
                //a single root is allowed.
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
                    break;

                case ".":
                    if (previous == Parts.First) {
                        break;
                    }

                    Parts.Remove(node);
                    break;

                case "..":
                    if (previous == Parts.First) {
                        break;
                    }

                    if (previous.Value == ".") {
                        Parts.Remove(previous);
                        break;
                    }

                    if (previous.Value == "..") {
                        break;
                    }

                    Parts.Remove(previous);
                    Parts.Remove(node);
                    break;
            }
            node = next;
        }

        // re-validate cleaned up input
        // .\ and ..\ illegal for all rooted paths @ Parts[1]
        if (IsRooted && Parts.Count > 1 && PathHelper.IsRelativeSpecialPart(Parts.First!.Next!.Value)) {
            throw new Exception("Not a valid rooted path.");
        }
        // .\ and ..\ required for non-rooted relative paths @ Parts[1]
        if (IsRelative && !PathHelper.IsRelativeSpecialPart(Parts.First!.Next!.Value)) {
            //normalize to current directory relative
            Parts.AddAfter(Parts.First, ".");
        }
    }

}