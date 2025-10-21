namespace SmartPaths;

public abstract class RelativePath : BasePath, IRelativePath
{

    private RelativeFolderPath? _parent;

    internal RelativePath(bool isFolder, PathCore core)
        : base(isFolder, core) { }

    protected RelativePath(bool isFolder, string path)
        : base(PathType.Relative, isFolder, path) { }

    public RelativeFolderPath? Parent {
        get {
            if (HasParent) {
                return _parent ??= new RelativeFolderPath(Core.GetParent());
            }
            return null;
        }
    }

    public RelativeFilePath GetSiblingFilePath(string name, string extension) {
        return GetSiblingFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetSiblingFilePath(string fileNameWithExtension) {
        NameHelper.EnsureOnlyValidCharacters(fileNameWithExtension);
        // cant from root, can elsewhere
        if (Core.Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(Core.RootValue);
        }
        return new RelativeFilePath(Core.GetParent().GetChild(fileNameWithExtension));
    }

    public RelativeFolderPath GetSiblingFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        // cant from root, can elsewhere
        if (Core.Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(Core.RootValue);
        }
        return new RelativeFolderPath(Core.GetParent().GetChild(folderName));
    }

    protected override IFolderPath? GetParent() {
        return HasParent ? Parent : null;
    }

}