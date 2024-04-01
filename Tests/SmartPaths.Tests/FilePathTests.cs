using Shouldly;

namespace SmartPaths;

public class FilePathTests
{

    [Theory]
    [InlineData(@"")]
    [InlineData(@".\")]
    [InlineData(@".\filename.ext")]
    [InlineData(@"c:")]
    [InlineData(@"c:\")]
    [InlineData(@"c:\filename\")]
    [InlineData(@"c:\filename.ext\")]
    public void AbsoluteFileInvalid(string source) {
        Should.Throw<Exception>(() => new AbsoluteFilePath(source));
    }

    [Theory]
    [InlineData(@"c:\filename", @"c:\filename", @"")]
    [InlineData(@"c:\filename.ext", @"c:\filename.ext", @"ext")]
    [InlineData(@"c:\filename.ext.gz", @"c:\filename.ext.gz", @"gz")]
    [InlineData(@"\\server\share\filename", @"\\server\share\filename", @"")]
    [InlineData(@"\\server\share\filename.ext", @"\\server\share\filename.ext", @"ext")]
    public void AbsoluteFileValid(string source, string clean, string extension) {
        AbsoluteFilePath file = new(source);
        file.ToString().ShouldBe(clean);
        file.FileExtension.ShouldBe(extension);
    }

    [Theory]
    [InlineData(@"")]
    [InlineData(@".\")]
    [InlineData(@"c:\filename")]
    public void RelativeFileInvalid(string source) {
        Should.Throw<Exception>(() => new RelativeFilePath(source));
    }

    [Theory]
    [InlineData(@".\filename", @".\filename", @"")]
    [InlineData(@".\filename.ext", @".\filename.ext", @"ext")]
    [InlineData(@".\filename.ext.gz", @".\filename.ext.gz", @"gz")]
    [InlineData(@"..\filename", @"..\filename", @"")]
    [InlineData(@"..\filename.ext", @"..\filename.ext", @"ext")]
    [InlineData(@"..\filename.ext.gz", @"..\filename.ext.gz", @"gz")]
    public void RelativeFileValid(string source, string clean, string extension) {
        RelativeFilePath file = new(source);
        file.FileExtension.ShouldBe(extension);
    }

}