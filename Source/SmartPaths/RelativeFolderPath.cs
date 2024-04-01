namespace SmartPaths;

/// <summary>Class RelativeFolderPath</summary>
public sealed class RelativeFolderPath : RelativePath, IRelativeFolderPath
{

    /// <summary>Initializes a new instance of the <see cref="RelativeFolderPath" /> class.</summary>
    /// <param name="path">The path.</param>
    public RelativeFolderPath(string path)
        : base(true, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFolderPath(LinkedList<string> parts, int partsLength, string? newItemName = null)
        : base(true, parts, partsLength, newItemName) { }

    public string FolderName => ItemName;

    /// <inheritdoc />
    public RelativeFilePath GetChildFilePath(string name, string extension) {
        return GetChildFilePath($"{name}.{extension}");
    }

    public RelativeFilePath GetChildFilePath(string fileNameWithExtension) {
        if (string.IsNullOrEmpty(fileNameWithExtension)) {
            throw new ArgumentException("Invalid empty file name.");
        }
        if (fileNameWithExtension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid filename: " + fileNameWithExtension);
        }

        return new RelativeFilePath(Parts, Parts.Count, fileNameWithExtension);
    }

    public RelativeFolderPath GetChildFolderPath(string folderName) {
        if (string.IsNullOrEmpty(folderName)) {
            throw new ArgumentException("Invalid empty folder name.");
        }
        if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid folder name: " + folderName);
        }

        return new RelativeFolderPath(Parts, Parts.Count, folderName);
    }

    /// <summary>Performs an implicit conversion from <see cref="System.String" /> to <see cref="RelativeFolderPath" />.</summary>
    /// <param name="path">The path.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator RelativeFolderPath?(string? path) {
        return path is null ? null : new RelativeFolderPath(path);
    }

}