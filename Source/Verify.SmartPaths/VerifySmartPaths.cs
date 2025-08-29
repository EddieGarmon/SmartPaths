using Argon;
using EmptyFiles;
using SmartPaths;
using SmartPaths.Storage;
using VerifyTests.Converters;

namespace VerifyTests;

public static class VerifySmartPaths
{

    public static bool Initialized { get; private set; }

    public static void Initialize() {
        if (Initialized) {
            throw new Exception("Already Initialized");
        }

        Initialized = true;

        InnerVerifier.ThrowIfVerifyHasBeenRun();
        VerifierSettings.AddExtraSettings(serializer => {
                                              List<JsonConverter> converters = serializer.Converters;
                                              converters.Add(new IPathConverter());
                                              converters.Add(new LedgerConverter());
                                          });
        VerifierSettings.RegisterFileConverter<Ledger>(Convert);
    }

    private static ConversionResult Convert(Ledger ledger, IReadOnlyDictionary<string, object> context) {
        HashSet<AbsoluteFilePath> edits = [];
        HashSet<AbsoluteFilePath> deletes = [];

        //flatten the ledger into a single changeset
        foreach (ILedgerRecord record in ledger.AllEvents) {
            switch (record) {
                case FileEventRecord fileRecord:
                    switch (record.Action) {
                        case LedgerAction.FileCreated:
                            deletes.Remove(fileRecord.ResultPath!);
                            edits.Add(fileRecord.ResultPath!);
                            break;
                        case LedgerAction.FileEdited:
                            edits.Add(fileRecord.ResultPath!);
                            break;
                        case LedgerAction.FileMoved:
                            edits.Remove(fileRecord.InitialPath!);
                            deletes.Add(fileRecord.InitialPath!);
                            edits.Add(fileRecord.ResultPath!);
                            break;
                        case LedgerAction.FileDeleted:
                            deletes.Add(fileRecord.InitialPath!);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case FolderEventRecord folderRecord:
                    switch (record.Action) {
                        case LedgerAction.FolderCreated:
                            break;
                        case LedgerAction.FolderMoved:
                            break;
                        case LedgerAction.FolderDeleted:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(record));
            }
        }

        List<Target> targets = [];
        //create targets for each type of change
        foreach (AbsoluteFilePath delete in deletes) {
            targets.Add(new Target(delete.FileExtension, "** FILE DELETED **", delete.FileNameWithoutExtension));
        }
        foreach (AbsoluteFilePath edit in edits) {
            if (FileExtensions.IsTextExtension(edit.FileExtension)) {
                targets.Add(new Target(edit.FileExtension, ledger.GetAllText(edit).Result, edit.FileNameWithoutExtension));
            } else {
                targets.Add(new Target(edit.FileExtension, ledger.GetFileStream(edit).Result, edit.FileNameWithoutExtension));
            }
        }

        return new ConversionResult(ledger, targets);
    }

}