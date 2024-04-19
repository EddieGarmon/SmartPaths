namespace SmartPaths;

[Flags]
public enum PathType
{

    Unknown = 0,

    Relative = 0x0100,
    Absolute = 0x0200,

    DriveLetter = 0x0201,  // C:\<path>
    RamDrive = 0x0202,     // ram:\<path>
    NetworkShare = 0x0204, // \\server\share\<path>
    RootRelative = 0x0208  // \<path> or /<path>

    //todo: Should we support any of the following?
    //Uri: file://
    //DOS Device: \\.\<path>  or \\?\<path>
    //DOS Legacy: \\.\COM1

}