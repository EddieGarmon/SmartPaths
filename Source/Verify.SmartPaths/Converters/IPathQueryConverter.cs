using SmartPaths;

namespace VerifyTests.Converters;

internal class IPathQueryConverter : WriteOnlyJsonConverter<IQuery>
{

    public override void Write(VerifyJsonWriter writer, IQuery value) {
        writer.WriteValue(value.ToString());
    }

}