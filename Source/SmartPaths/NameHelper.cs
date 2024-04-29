namespace SmartPaths;

internal static class NameHelper
{

    public static void EnsureOnlyValidCharacters(string pathSegment) {
        if (pathSegment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new Exception("Invalid path segment: " + pathSegment);
        }
    }

    public static void EnsureOnlyValidCharacters(LinkedList<string> path) {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        int index = 0;
        foreach (string part in path) {
            //NB: skip the 'root' part
            if (index > 0) {
                if (part.IndexOfAny(invalidChars) >= 0) {
                    throw new Exception("Invalid path segment: " + part);
                }
            }

            index++;
        }
    }

    public static void ExtractExtension(string nameWithExtension,
                                        out string userGivenPart,
                                        out string extension) {
        int lastDot = nameWithExtension.LastIndexOf('.');
        if (lastDot > 0) {
            userGivenPart = nameWithExtension.Substring(0, lastDot);
            extension = nameWithExtension.Substring(lastDot + 1);
        } else {
            userGivenPart = nameWithExtension;
            extension = string.Empty;
        }
    }

}