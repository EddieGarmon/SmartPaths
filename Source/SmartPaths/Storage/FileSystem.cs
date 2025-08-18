namespace SmartPaths.Storage;

public static class FileSystem
{

    private static DiskFileSystem? _disk;
    private static RamFileSystem? _ram;
    private static RedFileSystem? _red;

    public static DiskFileSystem Disk => _disk ??= new DiskFileSystem();

    public static RamFileSystem Ram => _ram ??= new RamFileSystem();

    public static RedFileSystem Red => _red ??= new RedFileSystem();

    public static IFileSystem Current { get; set; } = Disk;

    public static void UseDisk() {
        Current = Disk;
    }

    public static void UseRam() {
        Current = Ram;
    }

    public static void UseRed() {
        Current = Red;
    }

}