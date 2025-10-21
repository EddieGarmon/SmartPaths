namespace SmartPaths;

public interface IFilePath : IPath
{

    string FileExtension { get; }

    string FileName { get; }

    string FileNameWithoutExtension { get; }

#if !NETSTANDARD2_0
    static abstract IPath operator /(IFilePath start, IRelativePath relative);

    static abstract IQuery operator /(IFilePath start, RelativeQuery relative);
#endif

}