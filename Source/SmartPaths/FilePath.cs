using System.Text.RegularExpressions;

namespace SmartPaths;

public static class FilePath
{

    public static IFilePath Parse(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType == PathType.Relative) {
            return new RelativeFilePath(path);
        }
        return new AbsoluteFilePath(path);
    }

}