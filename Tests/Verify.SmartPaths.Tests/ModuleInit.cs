using System.Runtime.CompilerServices;

namespace VerifySmartPaths;

public static class ModuleInit
{

    [ModuleInitializer]
    public static void Init() {
        VerifierSettings.InitializePlugins();
    }

}