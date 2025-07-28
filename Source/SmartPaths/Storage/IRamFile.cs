namespace SmartPaths.Storage;

internal interface IRamFile : IFile
{

    byte[]? Data { get; set; }

}