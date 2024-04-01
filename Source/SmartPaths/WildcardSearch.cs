using System.Text.RegularExpressions;

namespace SmartPaths;

internal static class WildcardSearch
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

    private static string BuildRegexPattern(string wildcardPattern) {
        return "^" + Regex.Escape(wildcardPattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
    }

    private static void EnsureValidWildcardPattern(string wildcardPattern) {
        if (string.IsNullOrWhiteSpace(wildcardPattern) || wildcardPattern.IndexOfAny(Path.GetInvalidPathChars()) > 0) {
            throw new Exception("Invalid wildcard pattern: " + wildcardPattern);
        }
    }

}