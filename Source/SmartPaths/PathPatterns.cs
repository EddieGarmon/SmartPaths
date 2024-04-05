using System.Text.RegularExpressions;

namespace SmartPaths;

public static class PathPatterns
{

    public static Regex DriveLetterPattern { get; } = new(@"^([a-zA-Z]):(\\?$|\\(.*))$");

    public static Regex NetworkSharePattern { get; } = new(@"^(\\\\\w+\\\w+\\)(.*)$");

    public static Regex RamDrivePattern { get; } = new(@"^[Rr][Aa][Mm]:(\\?$|\\(.*)$)");

    public static Regex RelativePattern { get; } = new(@"^\.$|^\.\.$|^\.\\.*$|^\.\.\\.*$");

    public static (PathType, Match) DeterminePathType(string path) {
        Match match = RelativePattern.Match(path);
        if (match.Success) {
            return (PathType.Relative, match);
        }
        match = DriveLetterPattern.Match(path);
        if (match.Success) {
            return (PathType.DriveLetter, match);
        }
        match = RamDrivePattern.Match(path);
        if (match.Success) {
            return (PathType.RamDrive, match);
        }
        match = NetworkSharePattern.Match(path);
        if (match.Success) {
            return (PathType.NetworkShare, match);
        }
        return (PathType.Unknown, Match.Empty);
    }

}