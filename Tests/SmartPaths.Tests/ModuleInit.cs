using System.Runtime.CompilerServices;

namespace SmartPaths;

public static class ModuleInit
{

    [ModuleInitializer]
    public static void Init() {
        VerifySmartPaths.Initialize();
        //VerifierSettings.UseSplitModeForUniqueDirectory();
    }

}