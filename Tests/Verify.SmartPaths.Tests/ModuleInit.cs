using System.Runtime.CompilerServices;

namespace VerifySmartPaths;

public static class ModuleInit
{

    [ModuleInitializer]
    public static void Init() {
        UseProjectRelativeDirectory("Snapshots");
        VerifyImageSharp.Initialize();
        VerifySmartPaths.Initialize();
    }

}
