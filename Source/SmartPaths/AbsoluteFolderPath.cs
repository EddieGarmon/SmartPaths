using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class AbsoluteFolderPath : AbsolutePath, IAbsoluteFolderPath
{

    public AbsoluteFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal AbsoluteFolderPath(PathCore core)
        : base(true, core) { }

    public string FolderName => ItemName;

    internal bool IsRoot => Core.Parts.Count == 1;

    public RelativeFilePath ComputeRelative(AbsoluteFilePath target) {
        PathCore core = Core.ComputeRelative(false, target.Core);
        return new RelativeFilePath(core);
    }

    public RelativeFolderPath ComputeRelative(AbsoluteFolderPath target) {
        PathCore core = Core.ComputeRelative(false, target.Core);
        return new RelativeFolderPath(core);
    }

    public RelativeQuery ComputeRelative(AbsoluteQuery target) {
        PathCore core = Core.ComputeRelative(false, target.Core);
        return new RelativeQuery(core);
    }

    public AbsoluteFilePath GetChildFilePath(string name, string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public AbsoluteFilePath GetChildFilePath(string fileName) {
        NameHelper.EnsureOnlyValidCharacters(fileName);
        return new AbsoluteFilePath(Core.GetChild(fileName));
    }

    public AbsoluteFolderPath GetChildFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        return new AbsoluteFolderPath(Core.GetChild(folderName));
    }

    public AbsoluteFilePath ResolveRelative(RelativeFilePath relative) {
        PathCore core = Core.AdjustAbsolute(relative.Core);
        return new AbsoluteFilePath(core);
    }

    public AbsoluteFolderPath ResolveRelative(RelativeFolderPath relative) {
        PathCore core = Core.AdjustAbsolute(relative.Core);
        return new AbsoluteFolderPath(core);
    }

    public AbsoluteQuery ResolveRelative(RelativeQuery relative) {
        PathCore core = Core.AdjustAbsolute(relative.Core);
        return new AbsoluteQuery(core);
    }

    public static bool TryParse(string value, [NotNullWhen(true)] out AbsoluteFolderPath? path) {
        try {
            path = new AbsoluteFolderPath(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    public static AbsoluteFilePath operator +(AbsoluteFolderPath root, RelativeFilePath relative) {
        return root.ResolveRelative(relative);
    }

    public static AbsoluteFilePath operator +(AbsoluteFolderPath root, string relativeFile) {
        return root.ResolveRelative(new RelativeFilePath(relativeFile));
    }

    public static AbsoluteFilePath operator /(AbsoluteFolderPath root, RelativeFilePath relative) {
        return root.ResolveRelative(relative);
    }

    public static AbsoluteFolderPath operator /(AbsoluteFolderPath root, RelativeFolderPath relative) {
        return root.ResolveRelative(relative);
    }

    public static AbsoluteFolderPath operator /(AbsoluteFolderPath root, string relativeFolder) {
        return root.ResolveRelative(new RelativeFolderPath(relativeFolder));
    }

    public static AbsoluteQuery operator /(AbsoluteFolderPath root, RelativeQuery relative) {
        return root.ResolveRelative(relative);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator AbsoluteFolderPath?(string? path) {
        return path is null ? null : new AbsoluteFolderPath(path);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator string?(AbsoluteFolderPath? path) {
        return path?.ToString();
    }

    public static RelativeFolderPath operator >> (AbsoluteFolderPath fromDir, AbsoluteFolderPath toDir) {
        return fromDir.ComputeRelative(toDir);
    }

    public static RelativeFilePath operator >> (AbsoluteFolderPath fromDir, AbsoluteFilePath toFile) {
        return fromDir.ComputeRelative(toFile);
    }

#if !NETSTANDARD2_0
    static IPath IFolderPath.operator /(IFolderPath start, IRelativePath relative) {
        return SmartPath.Combine(start, relative);
    }

    static IQuery IFolderPath.operator /(IFolderPath start, RelativeQuery relative) {
        return SmartPath.Combine(start, relative);
    }
#endif

}