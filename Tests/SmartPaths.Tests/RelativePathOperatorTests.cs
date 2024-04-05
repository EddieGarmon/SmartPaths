using Shouldly;

namespace SmartPaths;

public class RelativePathOperatorTests
{

    [Theory]
    [InlineData(@"c:\hello", @"d:\world\filename.ext")]
    public void FileInvalid(string startDir, string endDir) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFilePath end = endDir;
        RelativeFilePath combined;
        Should.Throw<Exception>(() => combined = end - start);
    }

    [Theory]
    [InlineData(@"c:\hello", @"c:\world\filename.ext", @"..\world\filename.ext")]
    public void FileValid(string startDir, string endFile, string relativePath) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFilePath end = endFile;
        RelativeFilePath relative = end - start;
        relative.ToString().ShouldBe(relativePath);
    }

    [Theory]
    [InlineData(@"c:\hello", @"d:\world")]
    public void FolderInvalid(string startDir, string endDir) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFolderPath end = endDir;
        RelativeFolderPath combined;
        Should.Throw<Exception>(() => combined = end - start);
    }

    [Theory]
    [InlineData(@"c:\hello\world", @"c:\hello\moto", @"..\moto\")]
    [InlineData(@"c:\hello\world", @"c:\hello", @"..\")]
    [InlineData(@"c:\hello\world", @"c:\hello\world", @".\")]
    [InlineData(@"c:\hello", @"c:\hello\world", @".\world\")]
    public void FolderValid(string startDir, string endDir, string relativePath) {
        AbsoluteFolderPath start = startDir;
        AbsoluteFolderPath end = endDir;
        RelativeFolderPath relative = end - start;
        relative.ToString().ShouldBe(relativePath);
    }

}