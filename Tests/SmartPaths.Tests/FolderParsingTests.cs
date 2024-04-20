using Shouldly;

namespace SmartPaths;

public class FolderParsingTests
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
    public void InvalidAbsoluteFolder(string input) {
        Should.Throw<Exception>(() => new AbsoluteFolderPath(input));
    }

    [Theory]
    [InlineData(@"")]
    [InlineData(@"c:\")]
    [InlineData(@"\\unc\path")]
    [InlineData(@"http://path/")]
    //[InlineData(@"\..")] todo: should not be able to go above the root
    public void InvalidRelativeFolder(string input) {
        Should.Throw<Exception>(() => new RelativeFolderPath(input));
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
    [InlineData(@"ram:\Hello\World\..", @"ram:\Hello\", @"ram:\")]
    public void ValidAbsoluteFolder(string input, string clean, string rootValue) {
        AbsoluteFolderPath source = input;
        source.PathType.HasFlag(PathType.Absolute).ShouldBeTrue();
        source.RootValue.ShouldBe(rootValue);
        source.ToString().ShouldBe(clean);
    }

    [Theory]
    [InlineData(@"C", @".\C\", false)]
    [InlineData(@"Hello", @".\Hello\", false)]
    [InlineData(@"Hello\World", @".\Hello\World\", false)]
    [InlineData(@".", @".\", false)]
    [InlineData(@".\", @".\", false)]
    [InlineData(@".\\", @".\", false)]
    [InlineData(@".\\\", @".\", false)]
    [InlineData(@".\.", @".\", false)]
    [InlineData(@".\..", @"..\", false)]
    [InlineData(@".\Hello\..\..\", @"..\", false)]
    [InlineData(@".\..\..\", @"..\..\", false)]
    [InlineData(@".\Hello\World", @".\Hello\World\", false)]
    [InlineData(@".\Hello\World\", @".\Hello\World\", false)]
    [InlineData(@"..", @"..\", false)]
    [InlineData(@"..\", @"..\", false)]
    [InlineData(@"..\.", @"..\", false)]
    [InlineData(@"..\..", @"..\..\", false)]
    [InlineData(@"..\Hello\World", @"..\Hello\World\", false)]
    [InlineData(@"..\Hello\World\", @"..\Hello\World\", false)]
    //[InlineData(@"\", @"\", true)]
    [InlineData(@"\Hello", @"\Hello\", true)]
    public void ValidRelativeFolder(string input, string clean, bool isRooted) {
        RelativeFolderPath source = input;
        source.PathType.ShouldBe(isRooted ? PathType.RootRelative : PathType.Relative);
        source.ToString().ShouldBe(clean);
    }

}