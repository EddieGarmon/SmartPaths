using System.Text.RegularExpressions;
using SmartPaths.Storage;

namespace SmartPaths;

public static class WildcardSearch
{

    public static IEnumerable<AbsoluteFolderPath> Filter(this IEnumerable<AbsoluteFolderPath> paths, string wildcardPattern) {
        EnsureValidWildcardPattern(wildcardPattern);
        string regexPattern = BuildRegexPattern(wildcardPattern);
        Regex regex = new(regexPattern);
        return paths.Where(p => regex.IsMatch(p.FolderName));
    }

    public static IEnumerable<AbsoluteFilePath> Filter(this IEnumerable<AbsoluteFilePath> paths, string wildcardPattern) {
        EnsureValidWildcardPattern(wildcardPattern);
        string regexPattern = BuildRegexPattern(wildcardPattern);
        Regex regex = new(regexPattern);
        return paths.Where(p => regex.IsMatch(p.FileName));
    }

    public static IEnumerable<RelativeFolderPath> Filter(this IEnumerable<RelativeFolderPath> paths, string wildcardPattern) {
        EnsureValidWildcardPattern(wildcardPattern);
        string regexPattern = BuildRegexPattern(wildcardPattern);
        Regex regex = new(regexPattern);
        return paths.Where(p => regex.IsMatch(p.FolderName));
    }

    public static IEnumerable<RelativeFilePath> Filter(this IEnumerable<RelativeFilePath> paths, string wildcardPattern) {
        EnsureValidWildcardPattern(wildcardPattern);
        string regexPattern = BuildRegexPattern(wildcardPattern);
        Regex regex = new(regexPattern);
        return paths.Where(p => regex.IsMatch(p.FileName));
    }

    public static IEnumerable<IFolder> Filter(this IEnumerable<IFolder> folders, string wildcardPattern) {
        EnsureValidWildcardPattern(wildcardPattern);
        string regexPattern = BuildRegexPattern(wildcardPattern);
        Regex regex = new(regexPattern);
        return folders.Where(folder => regex.IsMatch(folder.Name));
    }

    public static IEnumerable<IFile> Filter(this IEnumerable<IFile> files, string wildcardPattern) {
        EnsureValidWildcardPattern(wildcardPattern);
        string regexPattern = BuildRegexPattern(wildcardPattern);
        Regex regex = new(regexPattern);
        return files.Where(file => regex.IsMatch(file.Name));
    }

    private static string BuildRegexPattern(string wildcardPattern) {
        return "^" + Regex.Escape(wildcardPattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
    }

    private static void EnsureValidWildcardPattern(string wildcardPattern) {
        if (string.IsNullOrWhiteSpace(wildcardPattern) || wildcardPattern.IndexOfAny(Path.GetInvalidPathChars()) > 0) {
            throw new Exception("Invalid wildcard pattern: " + wildcardPattern);
        }
    }

}