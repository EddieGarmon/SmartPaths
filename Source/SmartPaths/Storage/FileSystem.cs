using SmartPaths.Storage.Disk;
using SmartPaths.Storage.Isolated;
using SmartPaths.Storage.Ram;

namespace SmartPaths.Storage;

public static class FileSystem
{

    private static DiskFileSystem? _disk;
    private static RamFileSystem? _ram;

    public static DiskFileSystem Disk => _disk ??= new DiskFileSystem();

    public static RamFileSystem Ram => _ram ??= new RamFileSystem();

    public static IFileSystem Current { get; set; } = Disk;

#if !NETSTANDARD2_0
    public static IsolatedFileSystem Isolated(string key) {
        throw new NotImplementedException("FileSystem.Isolated");
    }
#endif

}