namespace SmartPaths;

public interface IPath
{

    bool HasParent { get; }

    /// <summary>Gets a value indicating whether this instance is an absolute path.</summary>
    /// <value><c>true</c> if this instance is an absolute path; otherwise, <c>false</c>.</value>
    bool IsAbsolutePath { get; }

    /// <summary>Gets a value indicating whether this instance is a file path.</summary>
    /// <value><c>true</c> if this instance is a file path; otherwise, <c>false</c>.</value>
    bool IsFilePath { get; }

    /// <summary>Gets a value indicating whether this instance is a folder path.</summary>
    /// <value><c>true</c> if this instance is a folder path; otherwise, <c>false</c>.</value>
    bool IsFolderPath { get; }

    /// <summary>Gets a value indicating whether this instance is a relative path.</summary>
    /// <value><c>true</c> if this instance is a relative path; otherwise, <c>false</c>.</value>
    bool IsRelativePath { get; }

    IFolderPath? Parent { get; }

    PathType PathType { get; }

}