using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class RelativeFolderPath : RelativePath, IRelativeFolderPath
{

    public RelativeFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFolderPath(IEnumerable<string> parts, int partsLength, string? newItemName = null)
        : base(true, parts, partsLength, newItemName) { }

    public string FolderName => ItemName;

    public RelativeFilePath GetChildFilePath(string name, string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetChildFilePath(string fileNameWithExtension) {
        ArgumentException.ThrowIfNullOrEmpty(fileNameWithExtension);
        if (fileNameWithExtension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid filename: " + fileNameWithExtension);
        }

        return new RelativeFilePath(Parts, Parts.Count, fileNameWithExtension);
    }

    public RelativeFolderPath GetChildFolderPath(string folderName) {
        ArgumentException.ThrowIfNullOrEmpty(folderName);
        if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid folder name: " + folderName);
        }

        return new RelativeFolderPath(Parts, Parts.Count, folderName);
    }

    public static bool TryParse(string value, [NotNullWhen(true)] out RelativeFolderPath? path) {
        try {
            path = new RelativeFolderPath(value);
            return true;
        } catch {
            path = null;
            return false;
        }
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator RelativeFolderPath?(string? path) {
        return path is null ? null : new RelativeFolderPath(path);
    }

}