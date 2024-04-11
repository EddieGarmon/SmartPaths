namespace SmartPaths;

public abstract class RelativePath : BasePath, IRelativePath
{

    private RelativeFolderPath? _parent;

    protected RelativePath(bool isFolder, string path)
        : base(PathType.Relative, isFolder, path) { }

    protected RelativePath(bool isFolder, IEnumerable<string> parts, int partsLength, string? newItemName = null)
        : base(PathType.Relative, isFolder, parts, partsLength, newItemName) { }

    public override bool HasParent => Parts.Count > 2 && !PathHelper.IsRelativeSpecialPart(Parts.Last!.Previous!.Value);

    public RelativeFolderPath? Parent => HasParent ? _parent ??= new RelativeFolderPath(Parts, Parts.Count - 1) : null;

    public RelativeFilePath GetSiblingFilePath(string name, string extension) {
        return GetSiblingFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetSiblingFilePath(string fileNameWithExtension) {
        NameHelper.EnsureOnlyValidCharacters(fileNameWithExtension);
        // cant from root, can elsewhere
        if (Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }

        return new RelativeFilePath(Parts, Parts.Count - 1, fileNameWithExtension);
    }

    public RelativeFolderPath GetSiblingFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        // cant from root, can elsewhere
        if (Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }

        return new RelativeFolderPath(Parts, Parts.Count - 1, folderName);
    }

    protected override IFolderPath? GetParent() {
        return HasParent ? Parent : null;
    }

}