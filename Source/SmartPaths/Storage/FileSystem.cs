using SmartPaths.Storage.Disk;
using SmartPaths.Storage.Ram;

namespace SmartPaths.Storage;

public static class FileSystem
{

    private static DiskFileSystem? _disk;
    private static RamFileSystem? _ram;

    public static DiskFileSystem Disk => _disk ??= new DiskFileSystem();

    public static RamFileSystem Ram => _ram ??= new RamFileSystem();

}