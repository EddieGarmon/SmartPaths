using Shouldly;

namespace SmartPaths.Storage;

public class FileSystemTests
{

    [Fact]
    public Task TestAllTheThings_DiskFileSystem() {
        return TestAllTheThings(new DiskFileSystem());
    }

    [Fact]
    public Task TestAllTheThings_RamFileSystem() {
        return TestAllTheThings(new RamFileSystem());
    }

    [Fact]
    public Task TestAllTheThings_RedFileSystem() {
        return TestAllTheThings(new RedFileSystem());
    }

    private static async Task TestAllTheThings(IFileSystem fileSystem) {
        DateTimeOffset startTime = DateTimeOffset.Now;

        AbsoluteFolderPath tempPath = fileSystem.TempStoragePath.GetChildFolderPath("FileSystemTesting");

        await fileSystem.DeleteFolder(tempPath);
        IFolder tempFolder = await fileSystem.CreateFolder(tempPath);
        tempFolder.ShouldNotBeNull();
        (await tempFolder.GetFolders()).Count.ShouldBe(0);
        (await tempFolder.GetFiles()).Count.ShouldBe(0);

        await tempFolder.CreateFolder("child");
        (await tempFolder.GetFolders()).Count.ShouldBe(1);

        IFile tempFile = await tempFolder.CreateFile("file1.txt");
        (await tempFolder.GetFiles()).Count.ShouldBe(1);

        await using (StreamWriter writer = new(await tempFile.OpenToWrite())) {
            await writer.WriteAsync("Hello World");
        }
        (await tempFile.GetLastWriteTime()).ShouldBeGreaterThanOrEqualTo(startTime);

        using (StreamReader reader = new(await tempFile.OpenToRead())) {
            (await reader.ReadToEndAsync(CancellationToken.None)).ShouldBe("Hello World");
        }
        await using (StreamWriter writer = new(await tempFile.OpenToAppend())) {
            await writer.WriteAsync(" - PASS");
        }
        (await tempFile.GetLastWriteTime()).ShouldBeGreaterThanOrEqualTo(startTime);

        using (StreamReader reader = new(await tempFile.OpenToRead())) {
            (await reader.ReadToEndAsync(CancellationToken.None)).ShouldBe("Hello World - PASS");
        }

        tempFile = await tempFile.Rename("moved1.txt");
        using (StreamReader reader = new(await tempFile.OpenToRead())) {
            (await reader.ReadToEndAsync(CancellationToken.None)).ShouldBe("Hello World - PASS");
        }

        tempFile = await tempFolder.CreateFile("file 2.txt");
        (await tempFolder.GetFiles()).Count.ShouldBe(2);
        await tempFile.Delete();
        (await tempFolder.GetFiles()).Count.ShouldBe(1);
        await tempFolder.Delete();
        (await fileSystem.GetFolder(tempPath)).ShouldBeNull();

        ////temp hack to keep this test from Red/Disk hitting each other
        await Task.Delay(200, CancellationToken.None);
    }

}