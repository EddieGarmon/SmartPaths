using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class RelativeFolderPath : RelativePath, IRelativeFolderPath
{

    public RelativeFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFolderPath(PathCore core)
        : base(true, core) { }

    public string FolderName => ItemName;

    public RelativeFilePath GetChildFilePath(string name, string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetChildFilePath(string fileNameWithExtension) {
        ArgumentException.ThrowIfNullOrEmpty(fileNameWithExtension);
        if (fileNameWithExtension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid filename: " + fileNameWithExtension);
        }
        PathCore core = new(PathType, Core.Parts, Core.Parts.Count, fileNameWithExtension);
        return new RelativeFilePath(core);
    }

    public RelativeFolderPath GetChildFolderPath(string folderName) {
        ArgumentException.ThrowIfNullOrEmpty(folderName);
        if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid folder name: " + folderName);
        }
        PathCore newCore = new(PathType, Core.Parts, Core.Parts.Count, folderName);
        return new RelativeFolderPath(newCore);
    }

    public RelativeFolderPath ResolveRelative(RelativeFolderPath relative) {
        PathCore core = Core.AdjustRelative(relative.Core);
        return new RelativeFolderPath(core);
    }

    public RelativeFilePath ResolveRelative(RelativeFilePath relative) {
        PathCore core = Core.AdjustRelative(relative.Core);
        return new RelativeFilePath(core);
    }

    public RelativeQueryPath ResolveRelative(RelativeQueryPath relative) {
        PathCore core = Core.AdjustRelative(relative.Core);
        return new RelativeQueryPath(core);
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

    public static RelativeFolderPath operator /(RelativeFolderPath start, RelativeFolderPath relativeFolder) {
        return start.ResolveRelative(relativeFolder);
    }

    public static RelativeFolderPath operator /(RelativeFolderPath start, string relativeFolder) {
        return start.ResolveRelative(new RelativeFolderPath(relativeFolder));
    }

    public static RelativeQueryPath operator /(RelativeFolderPath start, RelativeQueryPath relativeQuery) {
        return start.ResolveRelative(relativeQuery);
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

    static IPathQuery IFolderPath.operator /(IFolderPath start, RelativeQueryPath relative) {
        return SmartPath.Combine(start, relative);
    }
#endif

}