namespace SmartPaths;

public abstract class RelativePath : BasePath, IRelativePath
{

    private RelativeFolderPath? _parent;

    protected RelativePath(bool isFolder, string path)
        : base(PathType.Relative, isFolder, path) { }

    protected RelativePath(PathType pathType, bool isFolder, IEnumerable<string> parts, int partsLength, string? newItemName = null)
        : base(pathType, isFolder, parts, partsLength, newItemName) { }

    public override bool HasParent => Core.Parts.Count > 2 && !PathHelper.IsRelativeSpecialPart(Core.Parts.Last!.Previous!.Value);

    public RelativeFolderPath? Parent => HasParent ? _parent ??= new RelativeFolderPath(PathType, Core.Parts, Core.Parts.Count - 1) : null;

    public RelativeFilePath GetSiblingFilePath(string name, string extension) {
        return GetSiblingFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetSiblingFilePath(string fileNameWithExtension) {
        NameHelper.EnsureOnlyValidCharacters(fileNameWithExtension);
        // cant from root, can elsewhere
        if (Core.Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(Core.RootValue);
        }

        return new RelativeFilePath(PathType, Core.Parts, Core.Parts.Count - 1, fileNameWithExtension);
    }

    public RelativeFolderPath GetSiblingFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        // cant from root, can elsewhere
        if (Core.Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(Core.RootValue);
        }

        return new RelativeFolderPath(PathType, Core.Parts, Core.Parts.Count - 1, folderName);
    }

    protected override IFolderPath? GetParent() {
        return HasParent ? Parent : null;
    }

}