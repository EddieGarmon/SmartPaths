namespace SmartPaths;

[Flags]
public enum PathType
{

    Unknown = 0,

    Relative = 0x0100,

    RootRelative = 0x0101, // \<path> or /<path>

    Absolute = 0x0200,

    DriveLetter = 0x0201, // C:\<path>
    RamDrive = 0x0202,    // ram:\<path>
    NetworkShare = 0x0204 // \\server\share\<path>

    //todo: Should we support any of the following?
    //Uri: file://
    //DOS Device: \\.\<path>  or \\?\<path>
    //DOS Legacy: \\.\COM1

}