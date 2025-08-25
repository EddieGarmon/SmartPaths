namespace SmartPaths;

[Flags]
public enum PathType
{

    Unknown = 0,

    Relative = 0x0100,

    Absolute = 0x0200,

    RootRelative = 0x0201, // \<path> or /<path>
    DriveLetter = 0x0202,  // C:\<path>
    NetworkShare = 0x0204  // \\server\share\<path>

    //todo: Should we support any of the following?
    //Uri: file://
    //DOS Device: \\.\<path>  or \\?\<path>
    //DOS Legacy: \\.\COM1

}