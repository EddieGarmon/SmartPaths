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
        return pathType switch {
            PathType.Absolute => path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? new AbsoluteFolderPath(path) : new AbsoluteFilePath(path),
            _ => throw new Exception($"Path [{path}] is {pathType} not Absolute.")
        };
    }

    public static IFilePath ParseFile(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        return pathType switch {
            PathType.Relative => new RelativeFilePath(path),
            PathType.Absolute => new AbsoluteFilePath(path),
            _ => throw new Exception($"Invalid path for file: {path}")
        };
    }

    public static IFolderPath ParseFolder(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        return pathType switch {
            PathType.Relative => new RelativeFolderPath(path),
            PathType.Absolute => new AbsoluteFolderPath(path),
            _ => throw new Exception($"Invalid path for folder: {path}")
        };
    }

    public static RelativePath ParseRelative(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        return pathType switch {
            PathType.Relative => path.LastIndexOfAny(['\\', '/']) == path.Length - 1 ? new RelativeFolderPath(path) : new RelativeFilePath(path),
            _ => throw new Exception($"Path [{path}] is {pathType} not Relative.")
        };
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

    public static bool TryParseFile(string path, [NotNullWhen(true)] out IPath? filePath) {
        filePath = null;
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        switch (pathType) {
            case PathType.Relative:
                filePath = new RelativeFilePath(path);
                return true;
            case PathType.Absolute:
                filePath = new AbsoluteFilePath(path);
                return true;
            default:
                return false;
        }
    }

    public static bool TryParseFolder(string path, [NotNullWhen(true)] out IPath? folderPath) {
        folderPath = null;
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        switch (pathType) {
            case PathType.Relative:
                folderPath = new RelativeFolderPath(path);
                return true;
            case PathType.Absolute:
                folderPath = new AbsoluteFolderPath(path);
                return true;
            default:
                return false;
        }
    }

}