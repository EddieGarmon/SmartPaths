using Shouldly;

namespace SmartPaths;

public class ComputeRelativeOperatorTests
{

    [Theory]
    [InlineData(@"c:\hello", @"d:\world\filename.ext")]
    [InlineData(@"c:\hello", @"ram:\hello\filename.ext")]
    public void FileInvalid(AbsoluteFolderPath startDir, AbsoluteFilePath endDir) {
        Should.Throw<Exception>(() => startDir >> endDir);
    }

    [Theory]
    [InlineData(@"c:\hello", @"c:\world\filename.ext", @"..\world\filename.ext")]
    public void FileValid(AbsoluteFolderPath startDir, AbsoluteFilePath endFile, RelativeFilePath relativePath) {
        (startDir >> endFile).ShouldBe(relativePath);
    }

    [Theory]
    [InlineData(@"c:\hello", @"d:\world")]
    [InlineData(@"c:\hello", @"ram:\world")]
    public void FolderInvalid(AbsoluteFolderPath startDir, AbsoluteFolderPath endDir) {
        Should.Throw<Exception>(() => startDir >> endDir);
    }

    [Theory]
    [InlineData(@"c:\hello\world", @"c:\hello\moto", @"..\moto\")]
    [InlineData(@"c:\hello\world", @"c:\hello", @"..\")]
    [InlineData(@"c:\hello\world", @"c:\hello\world", @".\")]
    [InlineData(@"c:\hello", @"c:\hello\world", @".\world\")]
    public void FolderValid(AbsoluteFolderPath startDir, AbsoluteFolderPath endDir, RelativeFolderPath relativePath) {
        (startDir >> endDir).ShouldBe(relativePath);
    }

}