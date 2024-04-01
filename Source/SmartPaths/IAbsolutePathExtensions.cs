namespace SmartPaths;

public static class IAbsolutePathExtensions
{

    public static char GetDriveLetter(this IAbsolutePath path) {
        if (path.PathType == PathType.DriveLetter) {
            return char.ToUpperInvariant(path.RootValue[0]);
        }
        throw new Exception("Path is not drive letter rooted: " + path);
    }

}