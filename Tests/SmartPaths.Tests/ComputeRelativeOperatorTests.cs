using Shouldly;

namespace SmartPaths;

public class ComputeRelativeOperatorTests
{

    [Theory]
    [InlineData(@"c:\hello", @"d:\world\filename.ext")]
    [InlineData(@"c:\hello", @"ram:\hello\filename.ext")]
    public void InvalidFile(string startDir, string endDir) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFilePath end = endDir;
        Should.Throw<Exception>(() => start >> end);
    }

    [Theory]
    [InlineData(@"c:\hello", @"d:\world")]
    [InlineData(@"c:\hello", @"ram:\world")]
    public void InvalidFolder(string startDir, string endDir) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFolderPath end = endDir;
        Should.Throw<Exception>(() => start >> end);
    }

    [Theory]
    [InlineData(@"c:\hello", @"c:\world\filename.ext", @"..\world\filename.ext")]
    public void ValidFile(string startDir, string endFile, string relativePath) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFilePath end = endFile;
        RelativeFilePath relative = relativePath;
        (start >> end).ShouldBe(relative);
    }

    [Theory]
    [InlineData(@"c:\hello\world", @"c:\hello\moto", @"..\moto\")]
    [InlineData(@"c:\hello\world", @"c:\hello", @"..\")]
    [InlineData(@"c:\hello\world", @"c:\hello\world", @".\")]
    [InlineData(@"c:\hello", @"c:\hello\world", @".\world\")]
    public void ValidFolder(string startDir, string endDir, string relativePath) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFolderPath end = endDir;
        RelativeFolderPath relative = relativePath;
        (start >> end).ShouldBe(relative);
    }

}