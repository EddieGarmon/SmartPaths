using SmartPaths.Storage;

namespace SmartPaths;

public class VerifyLedgerTests
{

    [Fact]
    public async Task CreateAFile() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        RamFile newFile = await fileSystem.CreateFile("/activity/file1.txt", "File 1 Content");

        //verify ledger
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

    [Fact]
    public async Task CreateAFolder() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        RamFolder newFolder = await fileSystem.CreateFolder("/activity/");

        //verify ledger
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

    [Fact]
    public async Task DeleteAFile() {
        RamFileSystem fileSystem = new();
        await fileSystem.CreateFile("/activity/file1.txt", "File 1 Content");
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        await fileSystem.DeleteFile("/activity/file1.txt");

        //verify ledger
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

    [Fact]
    public async Task DeleteAFolder() {
        RamFileSystem fileSystem = new();
        await fileSystem.CreateFile("/activity/file1.txt", "File 1 Content");
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        await fileSystem.DeleteFolder("/activity/");

        //verify ledger
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

    [Fact]
    public async Task EditAFile() {
        RamFileSystem fileSystem = new();
        RamFile file = await fileSystem.CreateFile("/activity/file1.txt", "File 1 Content");
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        RamFile? ramFile = await fileSystem.GetFile("/activity/file1.txt") ?? throw new Exception("File not found");
        await using (Stream openToWrite = await ramFile.OpenToWrite()) {
            await using StreamWriter writer = new(openToWrite);
            await writer.WriteLineAsync(" 123 New File Content 456 ");
        }

        //verify ledger
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

    [Fact]
    public async Task EmptyLedger() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

    [Fact]
    public async Task MoveAFile() {
        RamFileSystem fileSystem = new();
        RamFile ramFile = await fileSystem.CreateFile("/activity/file1.txt", "File 1 Content");
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        RamFile movedFile = await ramFile.Move("/activity/movedFile1.txt");

        //verify ledger
        await Verify(ledger).UseDirectory(@"Snapshots");
    }

}