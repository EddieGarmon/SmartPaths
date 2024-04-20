namespace SmartPaths;

internal static class PathExceptions
{

    public static Exception TypeMismatch(PathType expected, PathType actual) {
        return new Exception($"Expected a path of type: {expected}, but found a path of type: {actual}.");
    }

    public static Exception UndefinedSiblingFor(string path) {
        return new Exception("A sibling of '" + path + "' is undefined");
    }

}