using System.Runtime.CompilerServices;

namespace VerifyTests.SmartPathsTests;

public static class ModuleInit
{

    [ModuleInitializer]
    public static void Init() {
        UseProjectRelativeDirectory("Snapshots");
        VerifierSettings.FixNewlinesOnRead();
        VerifyImageSharp.Initialize();
        VerifySmartPaths.Initialize();
    }

}
