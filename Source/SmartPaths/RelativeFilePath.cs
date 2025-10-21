using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class RelativeFilePath : RelativePath, IFilePath
{

    private string? _extension;
    private string? _userGivenName;

    public RelativeFilePath(string path)
        : base(false, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFilePath(PathCore core)
        : base(false, core) { }

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

    public RelativeFolderPath? Folder => Parent;

    public static bool TryParse(string value, [NotNullWhen(true)] out RelativeFilePath? path) {
        try {
            path = new RelativeFilePath(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    public static RelativeFilePath operator +(RelativeFilePath file, string relativeFile) {
        return file.Folder!.ResolveRelative(new RelativeFilePath(relativeFile));
    }

    public static RelativeFilePath operator +(RelativeFilePath file, RelativeFilePath relativeFile) {
        return file.Folder!.ResolveRelative(relativeFile);
    }

    public static RelativeFolderPath operator /(RelativeFilePath file, string relativeFolder) {
        return file.Folder!.ResolveRelative(new RelativeFolderPath(relativeFolder));
    }

    public static RelativeFolderPath operator /(RelativeFilePath file, RelativeFolderPath relativeFolder) {
        return file.Folder!.ResolveRelative(relativeFolder);
    }

    public static RelativeFilePath operator /(RelativeFilePath file, RelativeFilePath relative) {
        return file.Folder!.ResolveRelative(relative);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator RelativeFilePath?(string? path) {
        return path is null ? null : new RelativeFilePath(path);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator string?(RelativeFilePath? path) {
        return path?.ToString();
    }

#if !NETSTANDARD2_0
    static IPath IFilePath.operator /(IFilePath start, IRelativePath relative) {
        return SmartPath.Combine(start.GetParent()!, relative);
    }

    static IPathQuery IFilePath.operator /(IFilePath start, RelativeQueryPath relative) {
        return SmartPath.Combine(start.GetParent()!, relative);
    }
#endif

}