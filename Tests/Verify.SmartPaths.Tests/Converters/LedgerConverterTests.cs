using SmartPaths.Storage;

namespace SmartPaths.Converters;

public class LedgerConverterTests
{

    [Fact]
    public async Task ActivityLedger() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();

        //manipulate file system
        RamFile ramFile = await fileSystem.CreateFile((AbsoluteFilePath)"/activity/file1.txt", "File 1 Content");

        //verify ledger
        await Verify(ledger).UseDirectory(@"..\Snapshots");
    }

    [Fact]
    public async Task EmptyLedger() {
        RamFileSystem fileSystem = new();
        Ledger ledger = await fileSystem.StartLedger();
        await Verify(ledger).UseDirectory(@"..\Snapshots");
    }

}