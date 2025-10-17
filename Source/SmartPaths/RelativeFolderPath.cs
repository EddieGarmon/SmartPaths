using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class RelativeFolderPath : RelativePath, IRelativeFolderPath
{

    public RelativeFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFolderPath(PathType pathType, IEnumerable<string> parts, int partsLength, string? newItemName = null)
        : base(pathType, true, parts, partsLength, newItemName) { }

    public string FolderName => ItemName;

    public RelativeFilePath GetChildFilePath(string name, string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetChildFilePath(string fileNameWithExtension) {
        ArgumentException.ThrowIfNullOrEmpty(fileNameWithExtension);
        if (fileNameWithExtension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid filename: " + fileNameWithExtension);
        }

        return new RelativeFilePath(PathType, Parts, Parts.Count, fileNameWithExtension);
    }

    public RelativeFolderPath GetChildFolderPath(string folderName) {
        ArgumentException.ThrowIfNullOrEmpty(folderName);
        if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid folder name: " + folderName);
        }

        return new RelativeFolderPath(PathType, Parts, Parts.Count, folderName);
    }

    public RelativeFolderPath ResolveRelative(RelativeFolderPath relativeFolderPath) {
        LinkedList<string> parts = PathHelper.MakeRelative(this, relativeFolderPath);
        return new RelativeFolderPath(PathType, parts, parts.Count);
    }

    public RelativeFilePath ResolveRelative(RelativeFilePath relativeFilePath) {
        LinkedList<string> parts = PathHelper.MakeRelative(this, relativeFilePath);
        return new RelativeFilePath(PathType, parts, parts.Count);
    }

    public static bool TryParse(string value, [NotNullWhen(true)] out RelativeFolderPath? path) {
        try {
            path = new RelativeFolderPath(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    public static RelativeFilePath operator +(RelativeFolderPath start, string relativeFile) {
        return start.ResolveRelative(new RelativeFilePath(relativeFile));
    }

    public static RelativeFilePath operator +(RelativeFolderPath start, RelativeFilePath relativeFile) {
        return start.ResolveRelative(relativeFile);
    }

    public static RelativeFilePath operator /(RelativeFolderPath start, RelativeFilePath relativeFile) {
        return start.ResolveRelative(relativeFile);
    }

    public static RelativeFolderPath operator /(RelativeFolderPath start, string relativeFolder) {
        return start.ResolveRelative(new RelativeFolderPath(relativeFolder));
    }

    public static RelativeFolderPath operator /(RelativeFolderPath start, RelativeFolderPath relativeFolder) {
        return start.ResolveRelative(relativeFolder);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator RelativeFolderPath?(string? path) {
        return path is null ? null : new RelativeFolderPath(path);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator string?(RelativeFolderPath? path) {
        return path?.ToString();
    }

#if !NETSTANDARD2_0
    static IPath IFolderPath.operator /(IFolderPath start, IRelativePath relative) {
        return SmartPath.Combine(start, relative);
    }
#endif

}