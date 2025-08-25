using System.Text.RegularExpressions;

namespace SmartPaths;

internal static partial class PathPatterns
{

    //https://learn.microsoft.com/en-us/dotnet/standard/io/file-path-formats
    //https://unix.stackexchange.com/questions/125522/path-syntax-rules

    // language=regex
    public const string DriveLetterPattern = @"^([a-zA-Z]):(\\?$|\\(.*))$";

    // language=regex
    public const string NetworkSharePattern = @"(\\\\[a-zA-Z0-9\.\-_]{1,}\\[a-zA-Z0-9\-_]{1,})(?:\\?$|\\(.*))$";

    // language=regex
    public const string RootRelativePattern = @"^[\\/](.*)$";

    // language=regex
    public const string SpecialRelativePattern = @"^\.$|^\.\.$|^\.\\.*$|^\.\.\\.*$";

    // language=regex
    public const string GeneralRelativePattern = @"^\S.*$";

    public static (PathType, Match) DeterminePathType(string path) {
        //First attempt to match an absolute path
        Match match = DriveLetterRegex().Match(path);
        if (match.Success) {
            return (PathType.DriveLetter, match);
        }
        match = NetworkShareRegex().Match(path);
        if (match.Success) {
            return (PathType.NetworkShare, match);
        }
        match = RootRelativeRegex().Match(path);
        if (match.Success) {
            return (PathType.RootRelative, match);
        }
        //otherwise attempt to match a relative path
        match = SpecialRelativeRegex().Match(path);
        if (match.Success) {
            return (PathType.Relative, match);
        }
        match = GeneralRelativeRegex().Match(path);
        if (match.Success) {
            return (PathType.Relative, match);
        }
        return (PathType.Unknown, Match.Empty);
    }

#if NET7_0_OR_GREATER
    //compiler generated
    [GeneratedRegex(DriveLetterPattern)]
    public static partial Regex DriveLetterRegex();

    [GeneratedRegex(NetworkSharePattern)]
    public static partial Regex NetworkShareRegex();

    [GeneratedRegex(RootRelativePattern)]
    public static partial Regex RootRelativeRegex();

    [GeneratedRegex(SpecialRelativePattern)]
    public static partial Regex SpecialRelativeRegex();

    [GeneratedRegex(GeneralRelativePattern)]
    public static partial Regex GeneralRelativeRegex();
#endif

#if NETSTANDARD2_0 || NET6_0
    //runtime generated
    public static Regex DriveLetterRegex() {
        return new Regex(DriveLetterPattern);
    }

    public static Regex NetworkShareRegex() {
        return new Regex(NetworkSharePattern);
    }

    public static Regex RootRelativeRegex() {
        return new Regex(RootRelativePattern);
    }

    public static Regex SpecialRelativeRegex() {
        return new Regex(SpecialRelativePattern);
    }

    public static Regex GeneralRelativeRegex() {
        return new Regex(GeneralRelativePattern);
    }
#endif

}