using Shouldly;

namespace SmartPaths;

public class FolderPathTests
{

    [Theory]
    [InlineData(@".\")]
    [InlineData(@"..\")]
    [InlineData(@"c:\..\")]
    [InlineData(@"c:\foo\..\..\")]
    [InlineData(@"c:\foo\..\.\..\")]
    [InlineData(@"c:\foo\|\")]
    [InlineData(@"c:\foo\:\")]
    [InlineData(@"osi:\foo\")]
    public void AbsoluteFolderInvalid(string source) {
        Should.Throw<Exception>(() => new AbsoluteFolderPath(source)).ShouldNotBeNull();
    }

    [Theory]
    [InlineData(@"C:", @"C:\", @"C:\")]
    [InlineData(@"C:\", @"C:\", @"C:\")]
    [InlineData(@"C:\Hello\World", @"C:\Hello\World\", @"C:\")]
    [InlineData(@"C:\Hello\World\", @"C:\Hello\World\", @"C:\")]
    [InlineData(@"C:\Hello\\World\", @"C:\Hello\World\", @"C:\")]
    [InlineData(@"C:\Hello\.\World\", @"C:\Hello\World\", @"C:\")]
    [InlineData(@"C:\Hello\..\World\", @"C:\World\", @"C:\")]
    [InlineData(@"C:\Hello\..\World\..", @"C:\", @"C:\")]
    [InlineData(@"C:\Hello\World\..\..", @"C:\", @"C:\")]
    [InlineData(@"\\server\share\Hello\World\..", @"\\server\share\Hello\", @"\\server\share\")]
    public void AbsoluteFolderValid(string source, string clean, string rootValue) {
        AbsoluteFolderPath dir = new(source);
        dir.ToString().ShouldBe(clean);
        //todo maybe validate root value and path type instead
        dir.RootValue.ShouldBe(rootValue);
    }

    [Theory]
    [InlineData(@"")]
    [InlineData(@"c:\")]
    [InlineData(@"\\unc\path")]
    [InlineData(@"http://path/")]
    public void RelativeFolderInvalid(string source) {
        Should.Throw<Exception>(() => new RelativeFolderPath(source));
    }

    [Theory]
    [InlineData(@".", @".\")]
    [InlineData(@".\", @".\")]
    [InlineData(@".\.", @".\")]
    [InlineData(@".\..", @"..\")]
    [InlineData(@".\Hello\..\..\", @"..\")]
    [InlineData(@".\..\..\", @"..\..\")]
    [InlineData(@".\Hello\World", @".\Hello\World\")]
    [InlineData(@".\Hello\World\", @".\Hello\World\")]
    [InlineData(@"..", @"..\")]
    [InlineData(@"..\", @"..\")]
    [InlineData(@"..\.", @"..\")]
    [InlineData(@"..\Hello\World", @"..\Hello\World\")]
    [InlineData(@"..\Hello\World\", @"..\Hello\World\")]
    [InlineData(@"hello\world\", @".\hello\world\")]
    public void RelativeFolderValid(string source, string clean) {
        RelativeFolderPath dir = new(source);
        dir.ToString().ShouldBe(clean);
    }

}