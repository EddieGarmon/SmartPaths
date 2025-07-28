using Shouldly;

namespace SmartPaths.Storage;

public class FileSystemTests
{

    [Fact(Skip = "temporary")]
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

        DateTime timestamp = DateTime.Now;
        await using (StreamWriter writer = new(await tempFile.OpenToWrite())) {
            await Task.Delay(100);
            await writer.WriteAsync("Hello World");
        }
        await Task.Delay(200); //let the stream close
        (await tempFile.GetLastWriteTime()).ShouldBeGreaterThanOrEqualTo(timestamp);

        using (StreamReader reader = new(await tempFile.OpenToRead())) {
            (await reader.ReadToEndAsync()).ShouldBe("Hello World");
        }
        timestamp = DateTime.Now;
        await using (StreamWriter writer = new(await tempFile.OpenToAppend())) {
            await Task.Delay(100);
            await writer.WriteAsync(" - PASS");
        }
        await Task.Delay(200); //let the stream close
        (await tempFile.GetLastWriteTime()).ShouldBeGreaterThanOrEqualTo(timestamp);

        using (StreamReader reader = new(await tempFile.OpenToRead())) {
            (await reader.ReadToEndAsync()).ShouldBe("Hello World - PASS");
        }
        await Task.Delay(200); //let the stream close

        tempFile = await tempFile.Rename("moved1.txt");
        using (StreamReader reader = new(await tempFile.OpenToRead())) {
            (await reader.ReadToEndAsync()).ShouldBe("Hello World - PASS");
        }
        await Task.Delay(200); //let the stream close

        tempFile = await tempFolder.CreateFile("file 2.txt");
        (await tempFolder.GetFiles()).Count.ShouldBe(2);
        await tempFile.Delete();
        (await tempFolder.GetFiles()).Count.ShouldBe(1);
        await tempFolder.Delete();
        (await fileSystem.GetFolder(tempPath)).ShouldBeNull();
    }

}