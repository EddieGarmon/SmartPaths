namespace SmartPaths;

public abstract class AbsolutePath : BasePath, IAbsolutePath
{

    private AbsoluteFolderPath? _parent;

    protected AbsolutePath(bool isFolder, string path)
        : base(PathType.Absolute, isFolder, path) { }

    protected AbsolutePath(bool isFolder, IEnumerable<string> parts, int partsLength, string? newItemName = null)
        : base(PathType.Absolute, isFolder, parts, partsLength, newItemName) { }

    public override bool HasParent => Parts.Count > 1;

    public override AbsoluteFolderPath Parent =>
        HasParent ?
            _parent ??= new AbsoluteFolderPath(Parts, Parts.Count - 1) :
            throw new Exception($"The root {RootValue} does not have a parent.");

    public AbsoluteFilePath GetSiblingFilePath(string name, string extension) {
        return GetSiblingFilePath($"{name}.{extension}");
    }

    public AbsoluteFilePath GetSiblingFilePath(string fileNameWithExtension) {
        NameHelper.EnsureOnlyValidCharacters(fileNameWithExtension);
        // cant from root, can elsewhere
        if (Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }

        return new AbsoluteFilePath(Parts, Parts.Count - 1, fileNameWithExtension);
    }

    public AbsoluteFolderPath GetSiblingFolderPath(string folderName) {
        NameHelper.EnsureOnlyValidCharacters(folderName);
        // cant from root, can elsewhere
        if (Parts.Count == 1) {
            throw PathExceptions.UndefinedSiblingFor(RootValue);
        }

        return new AbsoluteFolderPath(Parts, Parts.Count - 1, folderName);
    }

}