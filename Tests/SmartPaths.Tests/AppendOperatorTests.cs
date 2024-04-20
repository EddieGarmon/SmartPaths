using Shouldly;

namespace SmartPaths;

public class AppendOperatorTests
{

    [Theory]
    [InlineData(@"c:\", @"..\relative")]
    [InlineData(@"c:\Hello\World", @".\..\..\..\Moto")]
    public void FileInvalid(AbsoluteFolderPath absoluteDir, RelativeFilePath relativeFile) {
        Should.Throw<Exception>(() => absoluteDir + relativeFile);
        Should.Throw<Exception>(() => absoluteDir / relativeFile);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto")]
    [InlineData(@"ram:\Hello\World", @"\Moto", @"ram:\Moto")]
    public void FileValid(AbsoluteFolderPath absoluteDir, RelativeFilePath relativeFile, AbsoluteFilePath combinedFile) {
        (absoluteDir + relativeFile).ShouldBe(combinedFile);
        (absoluteDir / relativeFile).ShouldBe(combinedFile);
    }

    [Theory]
    [InlineData(@"c:\", @"..\relative")]
    [InlineData(@"c:\Hello\World", @".\..\..\..\Moto")]
    public void FolderInvalid(AbsoluteFolderPath absoluteDir, RelativeFolderPath relativeDir) {
        Should.Throw<Exception>(() => absoluteDir + relativeDir);
        Should.Throw<Exception>(() => absoluteDir / relativeDir);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative\")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto\")]
    [InlineData(@"ram:\Hello\World", @"\Moto", @"ram:\Moto\")]
    public void FolderValid(AbsoluteFolderPath absoluteDir, RelativeFolderPath relativeDir, AbsoluteFolderPath combinedDir) {
        (absoluteDir + relativeDir).ShouldBe(combinedDir);
        (absoluteDir / relativeDir).ShouldBe(combinedDir);
    }

}