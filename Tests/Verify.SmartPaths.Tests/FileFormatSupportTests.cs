using System.Reflection;
using SmartPaths.Storage;

namespace VerifyTests.SmartPathsTests;

public class FileFormatSupportTests
{

    [Fact]
    public async Task CanSupportImageFiles() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        await using Stream? resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VerifyTests.SmartPathsTests.Files.icon.png");
        if (resourceStream is null) {
            Assert.Fail("Cant find test resource: icon.png");
        }
        RamFile newFile = await fileSystem.CreateFile("/activity/icon.png");
        await using (Stream openToWrite = await newFile.OpenToWrite()) {
            await resourceStream.CopyToAsync(openToWrite, CancellationToken.None);
        }

        //verify ledger
        //NB: we are using Verify.ImageSharp - Should see picture delta
        await Verify(ledger);
    }

    [Fact]
    public async Task CanSupportPdfFiles() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        await using Stream? resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VerifyTests.SmartPathsTests.Files.example.pdf");
        if (resourceStream is null) {
            Assert.Fail("Cant find test resource: example.pdf");
        }
        RamFile newFile = await fileSystem.CreateFile("/activity/example.pdf");
        await using (Stream openToWrite = await newFile.OpenToWrite()) {
            await resourceStream.CopyToAsync(openToWrite, CancellationToken.None);
        }

        //verify ledger
        //todo: add a PDF plugin and see we pipe to it.
        await Verify(ledger);
    }

    [Fact]
    public async Task CanSupportTextFiles() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        RamFile newFile = await fileSystem.CreateFile("/activity/simple.txt");
        await using (Stream openToWrite = await newFile.OpenToWrite()) {
            await using StreamWriter writer = new(openToWrite);
            await writer.WriteAsync("Text Files Are Supported!");
        }

        //verify ledger
        await Verify(ledger);
    }

}