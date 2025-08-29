using SmartPaths.Storage;

namespace VerifyTests.Converters;

public class IFileSystemConverter : WriteOnlyJsonConverter<IFileSystem>
{

    public override void Write(VerifyJsonWriter writer, IFileSystem value) {
        //write out a list of all files?
        throw new NotImplementedException();
    }

}