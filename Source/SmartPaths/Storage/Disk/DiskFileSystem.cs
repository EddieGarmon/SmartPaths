using System.Reflection;

namespace SmartPaths.Storage.Disk;

public class DiskFileSystem : BaseFileSystem<DiskFolder, DiskFile>
{

    private DiskFolder? _appLocal;
    private DiskFolder? _appRoaming;
    private DiskFolder? _temp;

    public override AbsoluteFolderPath AppLocalStoragePath { get; } = MakeAppStoragePath(Environment.SpecialFolder.LocalApplicationData);

    public override AbsoluteFolderPath AppRoamingStoragePath { get; } = MakeAppStoragePath(Environment.SpecialFolder.ApplicationData);

    public override AbsoluteFolderPath TempStoragePath { get; } = Path.GetTempPath();

    public override AbsoluteFolderPath WorkingDirectory {
        get => Environment.CurrentDirectory;
        set => Environment.CurrentDirectory = value;
    }

    public override Task<DiskFile> CreateFile(AbsoluteFilePath absoluteFile,
                                              CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return Task.Run(() => {
                            if (!File.Exists(absoluteFile)) {
                                Directory.CreateDirectory(absoluteFile.Folder);
                                using FileStream? _ = File.Create(absoluteFile);
                                return new DiskFile(absoluteFile);
                            }
                            //todo: support collisions
                            throw new NotImplementedException("DiskFileSystem.CreateFile - with collision");
                        });
    }

    public override Task<DiskFolder> CreateFolder(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => {
                            Directory.CreateDirectory(absoluteFolder);
                            return new DiskFolder(absoluteFolder);
                        });
    }

    public override Task DeleteFile(AbsoluteFilePath absoluteFile) {
        return Task.Run(() => {
                            if (File.Exists(absoluteFile)) {
                                File.Delete(absoluteFile);
                            }
                        });
    }

    public override Task DeleteFolder(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => {
                            if (Directory.Exists(absoluteFolder)) {
                                Directory.Delete(absoluteFolder, true);
                            }
                        });
    }

    public override Task<bool> FileExists(AbsoluteFilePath absoluteFile) {
        return Task.Run(() => File.Exists(absoluteFile));
    }

    public override Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => Directory.Exists(absoluteFolder));
    }

    public override Task<DiskFolder> GetAppLocalStorage() {
        return Task.FromResult(_appLocal ??= new DiskFolder(AppLocalStoragePath));
    }

    public override Task<DiskFolder> GetAppRoamingStorage() {
        return Task.FromResult(_appRoaming ??= new DiskFolder(AppRoamingStoragePath));
    }

    public override Task<DiskFile?> GetFile(AbsoluteFilePath absoluteFile) {
        return Task.Run(() => File.Exists(absoluteFile) ? new DiskFile(absoluteFile) : null);
    }

    public override Task<DiskFolder?> GetFolder(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => Directory.Exists(absoluteFolder) ? new DiskFolder(absoluteFolder) : null);
    }

    public override Task<DiskFolder> GetTempStorage() {
        return Task.FromResult(_temp ??= new DiskFolder(TempStoragePath));
    }

    private static AbsoluteFolderPath MakeAppStoragePath(Environment.SpecialFolder specialFolder) {
        AbsoluteFolderPath folder = Environment.GetFolderPath(specialFolder);
        string? companyName = null;
        string? productName = null;
        string? productVersion = null;
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is not null) {
            AssemblyCompanyAttribute? companyAttribute = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (companyAttribute is not null) {
                companyName = companyAttribute.Company;
            }
            AssemblyProductAttribute? productAttribute = entryAssembly.GetCustomAttribute<AssemblyProductAttribute>();
            if (productAttribute is not null) {
                productName = productAttribute.Product;
            }
            AssemblyInformationalVersionAttribute? versionAttribute =
                entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (versionAttribute is not null) {
                productVersion = versionAttribute.InformationalVersion;
            }
        } else {
            throw new Exception("How did we get here? what is the deployment?");
        }

        folder = folder.GetChildFolderPath(companyName ?? productName ?? "GuessWho");
        folder = folder.GetChildFolderPath(productName ?? "GuessWho");
        folder = folder.GetChildFolderPath(productVersion ?? "1.0.0.0");
        return folder;
    }

}