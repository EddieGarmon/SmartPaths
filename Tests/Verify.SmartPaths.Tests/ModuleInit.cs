using System.Runtime.CompilerServices;

namespace VerifyTests.SmartPathsTests;

public static class ModuleInit
{

    [ModuleInitializer]
    public static void Init() {
        UseProjectRelativeDirectory("Snapshots");
        VerifyImageSharp.Initialize();
        VerifySmartPaths.Initialize();
    }

}