﻿using System.Diagnostics.CodeAnalysis;

namespace SmartPaths;

public sealed class RelativeFilePath : RelativePath, IFilePath
{

    private string? _extension;
    private string? _userGivenName;

    public RelativeFilePath(string path)
        : base(false, path ?? throw new ArgumentNullException(nameof(path))) { }

    internal RelativeFilePath(LinkedList<string> parts, int partsLength, string? newItemName = null)
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

    public RelativeFolderPath? Folder => Parent;

    public static explicit operator AbsoluteFilePath(RelativeFilePath path) {
        AbsoluteFolderPath currentDir = Environment.CurrentDirectory;
        return currentDir / path;
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static implicit operator RelativeFilePath?(string? path) {
        return path is null ? null : new RelativeFilePath(path);
    }

}