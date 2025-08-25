using SmartPaths.Storage;

namespace SmartPaths.Converters;

internal class LedgerConverter : WriteOnlyJsonConverter<Ledger>
{

    public override void Write(VerifyJsonWriter writer, Ledger ledger) {
        writer.WriteStartArray();

        foreach (ILedgerRecord record in ledger.AllEvents) {
            writer.WriteStartObject();
            writer.WriteMember(record, record.Action, nameof(record.Action));
            if (record.InitialPath is not null) {
                writer.WriteMember(record, record.InitialPath, nameof(record.InitialPath));
            }
            if (record.ResultPath is not null) {
                writer.WriteMember(record, record.ResultPath, nameof(record.ResultPath));
            }
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

}