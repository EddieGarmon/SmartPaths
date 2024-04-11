using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class AbsoluteFilePath : AbsolutePath, IFilePath
{

    private string? _extension;
    private string? _userGivenName;

    public AbsoluteFilePath(string path)
        : base(false, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal AbsoluteFilePath(IEnumerable<string> parts, int partsLength, string? newItemName = null)
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

    public static bool TryParse(string value, out AbsoluteFilePath? path) {
        try {
            path = new AbsoluteFilePath(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator AbsoluteFilePath?(string? path) {
        return path is null ? null : new AbsoluteFilePath(path);
    }

    public static RelativeFolderPath operator >> (AbsoluteFilePath fromFile, AbsoluteFolderPath toDir) {
        return fromFile.Parent.MakeRelative(toDir);
    }

    public static RelativeFilePath operator >> (AbsoluteFilePath fromFile, AbsoluteFilePath toFile) {
        return fromFile.Parent.MakeRelative(toFile);
    }

}