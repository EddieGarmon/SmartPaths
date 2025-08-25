namespace SmartPaths.Converters;

internal class FileEditedConverter : WriteOnlyJsonConverter<FileEdited>
{

    public override void Write(VerifyJsonWriter writer, FileEdited value) {
        //todo: how to handle by file type?

        //text extensions
        // txt, csv, md, cs, xml, json

        //image extensions
        //image info and image file.

        //binary files, write in Base64?

        throw new NotImplementedException();
    }

}