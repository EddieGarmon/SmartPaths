using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SmartPaths;

/// <summary>Parses a path from a string. By convention, ALL Folder paths end with the
///     <see cref="Path.DirectorySeparatorChar" /></summary>
public static class SmartPath
{

    public static IPath Parse(string path) {
        return path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? ParseFolder(path) : ParseFile(path);
    }

    public static AbsolutePath ParseAbsolute(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType == PathType.Relative) {
            throw new Exception($"Path [{path}] is relative.");
        }
        return path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? new AbsoluteFolderPath(path) : new AbsoluteFilePath(path);
    }

    public static IFilePath ParseFile(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType == PathType.Relative) {
            return new RelativeFilePath(path);
        }
        return new AbsoluteFilePath(path);
    }

    public static IFolderPath ParseFolder(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType == PathType.Relative) {
            return new RelativeFolderPath(path);
        }
        return new AbsoluteFolderPath(path);
    }

    public static RelativePath ParseRelative(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType != PathType.Relative) {
            throw new Exception($"Path [{path}] is absolute.");
        }
        return path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? new RelativeFolderPath(path) : new RelativeFilePath(path);
    }

    public static bool TryParse(string? path, [NotNullWhen(true)] out IPath? smartPath) {
        if (path is null) {
            smartPath = null;
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

}