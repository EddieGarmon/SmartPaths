using System.Text.RegularExpressions;

namespace SmartPaths;

public static class FolderPath
{

    public static IFolderPath Parse(string path) {
        (PathType pathType, Match _) = PathPatterns.DeterminePathType(path);
        if (pathType == PathType.Relative) {
            return new RelativeFolderPath(path);
        }
        return new AbsoluteFolderPath(path);
    }

}