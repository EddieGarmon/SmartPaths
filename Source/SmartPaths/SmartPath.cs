using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SmartPaths;

/// <summary>Parses a path from a string. By convention, ALL Folder paths end with the
///     <see cref="Path.DirectorySeparatorChar" /></summary>
public static class SmartPath
{

    public static IPath Combine(IFolderPath start, IRelativePath relative) {
        return start switch {
            AbsoluteFolderPath absoluteStart => relative switch {
                RelativeFolderPath folderPath => absoluteStart.ResolveRelative(folderPath),
                RelativeFilePath filePath => absoluteStart.ResolveRelative(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(relative))
            },
            RelativeFolderPath relativeStart => relative switch {
                RelativeFolderPath folderPath => relativeStart.ResolveRelative(folderPath),
                RelativeFilePath filePath => relativeStart.ResolveRelative(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(relative))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(start))
        };
    }

    public static IQuery Combine(IFolderPath start, RelativeQuery relative) {
        return start switch {
            AbsoluteFolderPath absoluteStart => absoluteStart.ResolveRelative(relative),
            RelativeFolderPath relativeStart => relativeStart.ResolveRelative(relative),
            _ => throw new ArgumentOutOfRangeException(nameof(start))
        };
    }

    public static IPath Parse(string path) {
        return path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? ParseFolder(path) : ParseFile(path);
    }

    public static AbsolutePath ParseAbsolute(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType.HasFlag(PathType.Absolute)) {
            return path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? new AbsoluteFolderPath(path) : new AbsoluteFilePath(path);
        }
        throw new Exception($"Path [{path}] is {pathType} not Absolute.");
    }

    public static IFilePath ParseFile(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType.HasFlag(PathType.Relative)) {
            return new RelativeFilePath(path);
        }
        if (pathType.HasFlag(PathType.Absolute)) {
            return new AbsoluteFilePath(path);
        }
        throw new Exception($"Invalid path for file: {path}");
    }

    public static IFolderPath ParseFolder(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType.HasFlag(PathType.Relative)) {
            return new RelativeFolderPath(path);
        }
        if (pathType.HasFlag(PathType.Absolute)) {
            return new AbsoluteFolderPath(path);
        }
        throw new Exception($"Invalid path for folder: {path}");
    }

    public static IQuery ParseQuery(string query) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(query);
        if (pathType.HasFlag(PathType.Relative)) {
            return new RelativeQuery(query);
        }
        if (pathType.HasFlag(PathType.Absolute)) {
            return new AbsoluteQuery(query);
        }
        throw new Exception($"Invalid path query: {query}");
    }

    public static RelativePath ParseRelative(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType.HasFlag(PathType.Relative)) {
            return path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? new RelativeFolderPath(path) : new RelativeFilePath(path);
        }
        throw new Exception($"Path [{path}] is {pathType} not Relative.");
    }

    public static bool TryParse(string path, [NotNullWhen(true)] out IPath? smartPath) {
        smartPath = null;
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }
        try {
            smartPath = Parse(path);
            return true;
        } catch (Exception) {
            smartPath = null;
            return false;
        }
    }

    public static bool TryParseFile(string path, [NotNullWhen(true)] out IFilePath? filePath) {
        filePath = null;
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType.HasFlag(PathType.Relative)) {
            filePath = new RelativeFilePath(path);
            return true;
        }
        if (pathType.HasFlag(PathType.Absolute)) {
            filePath = new AbsoluteFilePath(path);
            return true;
        }
        return false;
    }

    public static bool TryParseFolder(string path, [NotNullWhen(true)] out IFolderPath? folderPath) {
        folderPath = null;
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType.HasFlag(PathType.Relative)) {
            folderPath = new RelativeFolderPath(path);
            return true;
        }
        if (pathType.HasFlag(PathType.Absolute)) {
            folderPath = new AbsoluteFolderPath(path);
            return true;
        }
        return false;
    }

    public static bool TryParseQuery(string query, [NotNullWhen(true)] out IQuery? pathQuery) {
        pathQuery = null;
        if (string.IsNullOrWhiteSpace(query)) {
            return false;
        }
        try {
            pathQuery = ParseQuery(query);
            return true;
        } catch (Exception) {
            pathQuery = null;
            return false;
        }
    }

}