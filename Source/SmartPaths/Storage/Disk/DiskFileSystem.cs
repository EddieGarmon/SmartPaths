using System.Reflection;

namespace SmartPaths.Storage.Disk;

public class DiskFileSystem : IFileSystem
{

    private DiskFolder? _appLocal;
    private DiskFolder? _appRoaming;
    private DiskFolder? _temp;

    public AbsoluteFolderPath AppLocalStoragePath { get; } = MakeAppStoragePath(Environment.SpecialFolder.LocalApplicationData);

    public AbsoluteFolderPath AppRoamingStoragePath { get; } = MakeAppStoragePath(Environment.SpecialFolder.ApplicationData);

    public AbsoluteFolderPath CurrentDirectory {
        get => Environment.CurrentDirectory;
        set => Environment.CurrentDirectory = value;
    }

    public AbsoluteFolderPath TempStoragePath { get; } = Path.GetTempPath();

    public Task<IFile> CreateFile(AbsoluteFilePath absoluteFile, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists) {
        return Task.Run<IFile>(() => {
                                   if (!File.Exists(absoluteFile)) {
                                       Directory.CreateDirectory(absoluteFile.Folder);
                                       using FileStream? _ = File.Create(absoluteFile);
                                       return new DiskFile(absoluteFile);
                                   }
                                   //todo: support collisions
                                   throw new NotImplementedException("DiskFileSystem.CreateFile - with collision");
                               });
    }

    public Task<IFolder> CreateFolder(AbsoluteFolderPath absoluteFolder) {
        return Task.Run<IFolder>(() => {
                                     Directory.CreateDirectory(absoluteFolder);
                                     return new DiskFolder(absoluteFolder);
                                 });
    }

    public Task DeleteFile(AbsoluteFilePath absoluteFile) {
        return Task.Run(() => {
                            if (File.Exists(absoluteFile)) {
                                File.Delete(absoluteFile);
                            }
                        });
    }

    public Task DeleteFolder(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => {
                            if (Directory.Exists(absoluteFolder)) {
                                Directory.Delete(absoluteFolder, true);
                            }
                        });
    }

    public Task<bool> FileExists(AbsoluteFilePath absoluteFile) {
        return Task.Run(() => File.Exists(absoluteFile));
    }

    public Task<bool> FolderExists(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => Directory.Exists(absoluteFolder));
    }

    public Task<IFolder> GetAppLocalStorage() {
        return Task.FromResult<IFolder>(_appLocal ??= new DiskFolder(AppLocalStoragePath));
    }

    public Task<IFolder> GetAppRoamingStorage() {
        return Task.FromResult<IFolder>(_appRoaming ??= new DiskFolder(AppRoamingStoragePath));
    }

    public Task<IFile?> GetFile(AbsoluteFilePath absoluteFile) {
        return Task.Run(() => {
                            IFile? result = null;
                            if (File.Exists(absoluteFile)) {
                                result = new DiskFile(absoluteFile);
                            }
                            return result;
                        });
    }

    public Task<IFolder?> GetFolder(AbsoluteFolderPath absoluteFolder) {
        return Task.Run(() => {
                            IFolder? result = null;
                            if (Directory.Exists(absoluteFolder)) {
                                result = new DiskFolder(absoluteFolder);
                            }
                            return result;
                        });
    }

    public Task<IFolder> GetTempStorage() {
        return Task.FromResult<IFolder>(_temp ??= new DiskFolder(TempStoragePath));
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