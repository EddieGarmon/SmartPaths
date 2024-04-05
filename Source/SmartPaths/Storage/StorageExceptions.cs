namespace SmartPaths.Storage;

public static class StorageExceptions
{

    public static Exception FileExists(IFilePath path) {
        return new Exception("File already exists: " + path);
    }

    public static Exception FileMissing(IFilePath path) {
        return new Exception("File is missing: " + path);
    }

    public static Exception FolderExists(IFolderPath path) {
        return new Exception("Folder already exists: " + path);
    }

    public static Exception FolderMissing(IFolderPath path) {
        return new Exception("Folder is missing: " + path);
    }

}