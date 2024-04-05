using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class AbsoluteFolderPath : AbsolutePath, IAbsoluteFolderPath
{

    public AbsoluteFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal AbsoluteFolderPath(LinkedList<string> parts, int partsLength, string? newItemName = null)
        : base(true, parts, partsLength, newItemName) { }

    public string FolderName => ItemName;

    internal bool IsRoot => Parts.Count == 1;

    public AbsoluteFilePath GetChildFilePath(string name, string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public AbsoluteFilePath GetChildFilePath(string fileName) {
        NameHelper.EnsureOnlyValidCharacters(fileName);
        return new AbsoluteFilePath(Parts, Parts.Count, fileName);
    }

    public AbsoluteFolderPath GetChildFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        return new AbsoluteFolderPath(Parts, Parts.Count, folderName);
    }

    public RelativeFilePath MakeRelative(AbsoluteFilePath target) {
        LinkedList<string> relative = PathHelper.MakeRelative(this, target.Folder);
        return new RelativeFilePath(relative, relative.Count, target.FileName);
    }

    public RelativeFolderPath MakeRelative(AbsoluteFolderPath target) {
        LinkedList<string> relative = PathHelper.MakeRelative(this, target);
        return new RelativeFolderPath(relative, relative.Count);
    }

    public static AbsoluteFolderPath operator +(AbsoluteFolderPath root, RelativeFolderPath relative) {
        LinkedList<string> parts = PathHelper.MakeAbsolute(root, relative);
        return new AbsoluteFolderPath(parts, parts.Count);
    }

    public static AbsoluteFilePath operator +(AbsoluteFolderPath root, RelativeFilePath relative) {
        LinkedList<string> parts = PathHelper.MakeAbsolute(root, relative);
        return new AbsoluteFilePath(parts, parts.Count);
    }

    public static AbsoluteFolderPath operator /(AbsoluteFolderPath root, RelativeFolderPath relative) {
        LinkedList<string> parts = PathHelper.MakeAbsolute(root, relative);
        return new AbsoluteFolderPath(parts, parts.Count);
    }

    public static AbsoluteFilePath operator /(AbsoluteFolderPath root, RelativeFilePath relative) {
        LinkedList<string> parts = PathHelper.MakeAbsolute(root, relative);
        return new AbsoluteFilePath(parts, parts.Count);
    }

    public static explicit operator RelativeFolderPath(AbsoluteFolderPath path) {
        AbsoluteFolderPath currentDir = Environment.CurrentDirectory;
        return path - currentDir;
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator AbsoluteFolderPath?(string? path) {
        return path is null ? null : new AbsoluteFolderPath(path);
    }

    public static RelativeFolderPath operator -(AbsoluteFolderPath toDir, AbsoluteFolderPath fromDir) {
        LinkedList<string> relative = PathHelper.MakeRelative(fromDir, toDir);
        //todo fix duplicate memory use in these situations
        return new RelativeFolderPath(relative, relative.Count);
    }

    public static RelativeFilePath operator -(AbsoluteFilePath toFile, AbsoluteFolderPath fromDir) {
        LinkedList<string> relativePath = PathHelper.MakeRelative(fromDir, toFile.Parent);
        return new RelativeFilePath(relativePath, relativePath.Count, toFile.FileName);
    }

}