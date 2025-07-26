using System.Reflection;

namespace SmartPaths.Storage;

public sealed class DiskFileSystem : BaseFileSystem<DiskFolder, DiskFile>
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

    public override Task<DiskFile> CreateFile(AbsoluteFilePath filePath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return Task.Run(() => {
                            if (!File.Exists(filePath)) {
                                Directory.CreateDirectory(filePath.Folder);
                                using FileStream? _ = File.Create(filePath);
                                return new DiskFile(filePath);
                            }
                            //todo: support collisions
                            throw new NotImplementedException("DiskFileSystem.CreateFile - with collision");
                        });
    }

    public override Task<DiskFolder> CreateFolder(AbsoluteFolderPath folderPath) {
        return Task.Run(() => {
                            Directory.CreateDirectory(folderPath);
                            return new DiskFolder(folderPath);
                        });
    }

    public override Task DeleteFile(AbsoluteFilePath filePath) {
        return Task.Run(() => {
                            if (File.Exists(filePath)) {
                                File.Delete(filePath);
                            }
                        });
    }

    public override Task DeleteFolder(AbsoluteFolderPath folderPath) {
        return Task.Run(() => {
                            if (Directory.Exists(folderPath)) {
                                Directory.Delete(folderPath, true);
                            }
                        });
    }

    public override Task<bool> FileExists(AbsoluteFilePath filePath) {
        return Task.Run(() => File.Exists(filePath));
    }

    public override Task<bool> FolderExists(AbsoluteFolderPath folderPath) {
        return Task.Run(() => Directory.Exists(folderPath));
    }

    public override Task<DiskFolder> GetAppLocalStorage() {
        return Task.FromResult(_appLocal ??= new DiskFolder(AppLocalStoragePath));
    }

    public override Task<DiskFolder> GetAppRoamingStorage() {
        return Task.FromResult(_appRoaming ??= new DiskFolder(AppRoamingStoragePath));
    }

    public override Task<DiskFile?> GetFile(AbsoluteFilePath filePath) {
        return Task.Run(() => File.Exists(filePath) ? new DiskFile(filePath) : null);
    }

    public override Task<DiskFolder?> GetFolder(AbsoluteFolderPath folderPath) {
        return Task.Run(() => Directory.Exists(folderPath) ? new DiskFolder(folderPath) : null);
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
            AssemblyInformationalVersionAttribute? versionAttribute = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
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