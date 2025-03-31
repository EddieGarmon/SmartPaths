namespace SmartPaths;

internal static class PathHelper
{

    public static bool IsRelativeSpecialPart(string part) {
        return part is "." or "..";
    }

    public static LinkedList<string> MakeAbsolute(AbsolutePath fromHere, RelativePath adjustment) {
        LinkedList<string> result;

        //handle RootRelative
        if (adjustment.PathType == PathType.RootRelative) {
            result = [];
            result.AddFirst(fromHere.RootValue);
            foreach (string part in adjustment.PartsAfterRoot) {
                result.AddLast(part);
            }
            return result;
        }

        //handle Relative
        result = new LinkedList<string>(fromHere.Parts);
        foreach (string part in adjustment.PartsAfterRoot) {
            switch (part) {
                case ".":
                    break;
                case "..":
                    if (result.Count > 1) {
                        result.RemoveLast();
                    } else {
                        throw new Exception($"Can not resolve '{adjustment}' from '{fromHere}'.");
                    }
                    break;
                default:
                    result.AddLast(part);
                    break;
            }
        }

        return result;
    }

    public static LinkedList<string> MakeRelative(AbsolutePath fromHere, AbsolutePath toHere) {
        if (fromHere.RootValue != toHere.RootValue) {
            throw new Exception("No shared root between: " + fromHere + " -> " + toHere);
        }

        LinkedListNode<string>? fromNode = fromHere.Parts.First;
        LinkedListNode<string>? toNode = toHere.Parts.First;

        while (fromNode is not null && toNode is not null && fromNode.Value == toNode.Value) {
            if (fromHere.IsFilePath && fromNode.Next is null) {
                // NB: Exclude filename from path consideration
                fromNode = fromNode.Next;
                break;
            }
            fromNode = fromNode.Next;
            toNode = toNode.Next;
        }

        LinkedList<string> relative = new([string.Empty]);
        while (fromNode is not null) {
            relative.AddLast("..");
            fromNode = fromNode.Next;
        }

        if (relative.Count == 1) {
            //NB: establish relative root as current directory
            relative.AddLast(".");
        }

        while (toNode is not null) {
            relative.AddLast(toNode.Value);
            toNode = toNode.Next;
        }

        return relative;
    }

}