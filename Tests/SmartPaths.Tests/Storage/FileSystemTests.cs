using Shouldly;
using SmartPaths.Storage.Disk;

namespace SmartPaths.Storage;

public class FileSystemTests
{

    [Fact]
    public async Task Disk_SuperTest() {
        await SuperTest(new DiskFileSystem());
    }

    private static async Task SuperTest(IFileSystem fileSystem) {
        AbsoluteFolderPath tempDir = fileSystem.TempStoragePath.GetChildFolderPath("FileSystemTesting");
        await fileSystem.DeleteFolder(tempDir);
        IFolder temp = await fileSystem.CreateFolder(tempDir);
        temp.ShouldNotBeNull();
        (await temp.GetFolders()).Count.ShouldBe(0);
        (await temp.GetFiles()).Count.ShouldBe(0);
        await temp.CreateFolder("child");
        (await temp.GetFolders()).Count.ShouldBe(1);
        IFile file = await temp.CreateFile("file1.txt");
        (await temp.GetFiles()).Count.ShouldBe(1);
        DateTime timestamp = DateTime.Now;
        await using (StreamWriter writer = new(await file.OpenToWrite())) {
            await writer.WriteAsync("Hello World");
        }
        (await file.GetLastWriteTime()).ShouldBeGreaterThanOrEqualTo(timestamp);
        using (StreamReader reader = new(await file.OpenToRead())) {
            (await reader.ReadToEndAsync()).ShouldBe("Hello World");
        }
        timestamp = DateTime.Now;
        await using (StreamWriter writer = new(await file.OpenToAppend())) {
            await writer.WriteAsync(" - PASS");
        }
        (await file.GetLastWriteTime()).ShouldBeGreaterThanOrEqualTo(timestamp);
        using (StreamReader reader = new(await file.OpenToRead())) {
            (await reader.ReadToEndAsync()).ShouldBe("Hello World - PASS");
        }
        await file.Delete();
        (await temp.GetFiles()).Count.ShouldBe(0);
        await temp.Delete();
        (await fileSystem.GetFolder(tempDir)).ShouldBeNull();
    }

}