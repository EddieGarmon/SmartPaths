using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class AbsoluteFilePath : AbsolutePath, IFilePath
{

    private string? _extension;
    private string? _userGivenName;

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

    public AbsoluteFolderPath Folder => Parent!;

    public static explicit operator RelativeFilePath(AbsoluteFilePath path) {
        AbsoluteFolderPath currentDir = Environment.CurrentDirectory;
        return path - currentDir;
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator AbsoluteFilePath?(string? path) {
        return path is null ? null : new AbsoluteFilePath(path);
    }

}