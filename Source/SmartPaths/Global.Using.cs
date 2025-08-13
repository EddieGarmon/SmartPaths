global using IAbsoluteFolderPath = SmartPaths.IFolderPath<SmartPaths.AbsoluteFolderPath, SmartPaths.AbsoluteFilePath>;
global using IRelativeFolderPath = SmartPaths.IFolderPath<SmartPaths.RelativeFolderPath, SmartPaths.RelativeFilePath>;

#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif