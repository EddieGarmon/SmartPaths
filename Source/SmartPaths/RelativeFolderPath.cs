using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class RelativeFolderPath : RelativePath, IRelativeFolderPath
{

    public RelativeFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFolderPath(PathCore core)
        : base(true, core) { }

    public string FolderName => ItemName;

    public RelativeFolderPath AdjustRelative(RelativeFolderPath relativeFolderPath) {
        PathCore core = Core.AdjustRelative(relativeFolderPath.Core);
        return new RelativeFolderPath(core);
    }

    public RelativeFilePath AdjustRelative(RelativeFilePath relativeFilePath) {
        PathCore core = Core.AdjustRelative(relativeFilePath.Core);
        return new RelativeFilePath(core);
    }

    public RelativeQueryPath AdjustRelative(RelativeQueryPath relativePathQuery) {
        PathCore core = Core.AdjustRelative(relativePathQuery.Core);
        return new RelativeQueryPath(core);
    }

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
        return start.AdjustRelative(new RelativeFilePath(relativeFile));
    }

    public static RelativeFilePath operator +(RelativeFolderPath start, RelativeFilePath relativeFile) {
        return start.AdjustRelative(relativeFile);
    }

    public static RelativeFilePath operator /(RelativeFolderPath start, RelativeFilePath relativeFile) {
        return start.AdjustRelative(relativeFile);
    }

    public static RelativeFolderPath operator /(RelativeFolderPath start, string relativeFolder) {
        return start.AdjustRelative(new RelativeFolderPath(relativeFolder));
    }

    public static RelativeFolderPath operator /(RelativeFolderPath start, RelativeFolderPath relativeFolder) {
        return start.AdjustRelative(relativeFolder);
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