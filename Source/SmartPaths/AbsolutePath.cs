namespace SmartPaths;

public abstract class AbsolutePath : BasePath, IAbsolutePath
{

    private AbsoluteFolderPath? _parent;

    internal AbsolutePath(bool isFolder, PathCore core)
        : base(isFolder, core) { }

    protected AbsolutePath(bool isFolder, string path)
        : base(PathType.Absolute, isFolder, path) { }

    public AbsoluteFolderPath Parent {
        get {
            if (HasParent) {
                return _parent ??= new AbsoluteFolderPath(Core.GetParent());
            }
            throw new Exception($"The root {RootValue} does not have a parent.");
        }
    }

    public string RootValue => Core.RootValue;

    public AbsoluteFilePath GetSiblingFilePath(string name, string extension) {
        return GetSiblingFilePath($"{name}.{extension}");
    }

    public AbsoluteFilePath GetSiblingFilePath(string fileNameWithExtension) {
        NameHelper.EnsureOnlyValidCharacters(fileNameWithExtension);
        // cant from root, can elsewhere
        if (Core.Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }
        return new AbsoluteFilePath(Core.GetParent().GetChild(fileNameWithExtension));
    }

    public AbsoluteFolderPath GetSiblingFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        // cant from root, can elsewhere
        if (Core.Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }
        return new AbsoluteFolderPath(Core.GetParent().GetChild(folderName));
    }

    //todo: what is this for?
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