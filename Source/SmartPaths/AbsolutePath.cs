namespace SmartPaths;

public abstract class AbsolutePath : BasePath, IAbsolutePath
{

    private AbsoluteFolderPath? _parent;

    protected AbsolutePath(bool isFolder, string path)
        : base(PathType.Absolute, isFolder, path) { }

    protected AbsolutePath(PathType pathType, bool isFolder, IEnumerable<string> parts, int partsLength, string? newItemName = null)
        : base(pathType, isFolder, parts, partsLength, newItemName) { }

    public override bool HasParent => Parts.Count > 1;

    public AbsoluteFolderPath Parent =>
        HasParent ? _parent ??= new AbsoluteFolderPath(PathType, Parts, Parts.Count - 1) : throw new Exception($"The root {RootValue} does not have a parent.");

    public AbsoluteFilePath GetSiblingFilePath(string name, string extension) {
        return GetSiblingFilePath($"{name}.{extension}");
    }

    public AbsoluteFilePath GetSiblingFilePath(string fileNameWithExtension) {
        NameHelper.EnsureOnlyValidCharacters(fileNameWithExtension);
        // cant from root, can elsewhere
        if (Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }

        return new AbsoluteFilePath(PathType, Parts, Parts.Count - 1, fileNameWithExtension);
    }

    public AbsoluteFolderPath GetSiblingFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        // cant from root, can elsewhere
        if (Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }

        return new AbsoluteFolderPath(PathType, Parts, Parts.Count - 1, folderName);
    }

    internal AbsoluteFolderPath GetRoot() {
        AbsoluteFolderPath root = this switch {
            AbsoluteFolderPath folderPath => folderPath,
            AbsoluteFilePath filePath => filePath.Folder,
            _ => throw new NotSupportedException("Unsupported absolute path type.")
        };
        while (root.HasParent) {
            root = root.Parent;
        }
        return root;
    }

    protected override IFolderPath? GetParent() {
        return HasParent ? Parent : null;
    }

}