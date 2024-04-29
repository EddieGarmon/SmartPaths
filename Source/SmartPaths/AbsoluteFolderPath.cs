using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class AbsoluteFolderPath : AbsolutePath, IAbsoluteFolderPath
{

    public AbsoluteFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal AbsoluteFolderPath(PathType pathType,
                                IEnumerable<string> parts,
                                int partsLength,
                                string? newItemName = null)
        : base(pathType, true, parts, partsLength, newItemName) { }

    public string FolderName => ItemName;

    internal bool IsRoot => Parts.Count == 1;

    public AbsoluteFilePath GetChildFilePath(string name,
                                             string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public AbsoluteFilePath GetChildFilePath(string fileName) {
        NameHelper.EnsureOnlyValidCharacters(fileName);
        return new AbsoluteFilePath(PathType, Parts, Parts.Count, fileName);
    }

    public AbsoluteFolderPath GetChildFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        return new AbsoluteFolderPath(PathType, Parts, Parts.Count, folderName);
    }

    public RelativeFilePath MakeRelative(AbsoluteFilePath target) {
        LinkedList<string> relative = PathHelper.MakeRelative(this, target.Folder);
        return new RelativeFilePath(PathType.Relative, relative, relative.Count, target.FileName);
    }

    public RelativeFolderPath MakeRelative(AbsoluteFolderPath target) {
        LinkedList<string> relative = PathHelper.MakeRelative(this, target);
        return new RelativeFolderPath(PathType.Relative, relative, relative.Count);
    }

    public AbsoluteFolderPath ResolveRelative(RelativeFolderPath relative) {
        LinkedList<string> parts = PathHelper.MakeAbsolute(this, relative);
        return new AbsoluteFolderPath(PathType, parts, parts.Count);
    }

    public AbsoluteFilePath ResolveRelative(RelativeFilePath relative) {
        LinkedList<string> parts = PathHelper.MakeAbsolute(this, relative);
        return new AbsoluteFilePath(PathType, parts, parts.Count);
    }

    public static bool TryParse(string value,
                                [NotNullWhen(true)] out AbsoluteFolderPath? path) {
        try {
            path = new AbsoluteFolderPath(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    public static AbsoluteFolderPath operator +(AbsoluteFolderPath root,
                                                RelativeFolderPath relative) {
        return root.ResolveRelative(relative);
    }

    public static AbsoluteFilePath operator +(AbsoluteFolderPath root,
                                              RelativeFilePath relative) {
        return root.ResolveRelative(relative);
    }

    public static AbsoluteFolderPath operator /(AbsoluteFolderPath root,
                                                RelativeFolderPath relative) {
        return root.ResolveRelative(relative);
    }

    public static AbsoluteFilePath operator /(AbsoluteFolderPath root,
                                              RelativeFilePath relative) {
        return root.ResolveRelative(relative);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator AbsoluteFolderPath?(string? path) {
        return path is null ? null : new AbsoluteFolderPath(path);
    }

    public static RelativeFolderPath operator >> (AbsoluteFolderPath fromDir,
                                                  AbsoluteFolderPath toDir) {
        return fromDir.MakeRelative(toDir);
    }

    public static RelativeFilePath operator >> (AbsoluteFolderPath fromDir,
                                                AbsoluteFilePath toFile) {
        return fromDir.MakeRelative(toFile);
    }

}