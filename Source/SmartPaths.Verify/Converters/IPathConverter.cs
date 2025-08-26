namespace SmartPaths.Converters;

internal class IPathConverter : WriteOnlyJsonConverter<IPath>
{

    public override void Write(VerifyJsonWriter writer, IPath value) {
        writer.WriteValue(value.ToString());
    }

}