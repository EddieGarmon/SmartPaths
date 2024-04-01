namespace SmartPaths;

/// <summary>Class AbsoluteFilePath</summary>
public sealed class AbsoluteFilePath : AbsolutePath, IFilePath
{

    private string? _extension;
    private string? _userGivenName;

    /// <summary>Initializes a new instance of the <see cref="AbsoluteFilePath" /> class.</summary>
    /// <param name="path">The path.</param>
    public AbsoluteFilePath(string path)
        : base(false, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal AbsoluteFilePath(LinkedList<string> parts, int partsLength, string? newItemName = null)
        : base(false, parts, partsLength, newItemName) { }

    public string FileExtension {
        get {
            if (_extension is null) {
                NameHelper.ExtractExtension(ItemName, out _userGivenName, out _extension);
            }

            return _extension;
        }
    }

    public string FileName => ItemName;

    public string FileNameWithoutExtension {
        get {
            if (_userGivenName is null) {
                NameHelper.ExtractExtension(ItemName, out _userGivenName, out _extension);
            }

            return _userGivenName;
        }
    }

    /// <summary>Gets the folder.</summary>
    /// <value>The folder.</value>
    public AbsoluteFolderPath Folder => Parent!;

    /// <summary>Performs an implicit conversion from <see cref="string" /> to <see cref="AbsoluteFilePath" />.</summary>
    /// <param name="path">The path.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator AbsoluteFilePath?(string? path) {
        return path is null ? null : new AbsoluteFilePath(path);
    }

}