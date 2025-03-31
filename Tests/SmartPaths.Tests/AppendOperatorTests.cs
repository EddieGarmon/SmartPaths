using Shouldly;

namespace SmartPaths;

public class AppendOperatorTests
{

    [Theory]
    [InlineData(@"c:\", @"..\relative")]
    [InlineData(@"c:\Hello\World", @".\..\..\..\Moto")]
    public void InvalidFile(string absoluteDir, string relativeFile) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFilePath relative = relativeFile;
        Should.Throw<Exception>(() => absolute + relative);
        Should.Throw<Exception>(() => absolute / relative);
    }

    [Theory]
    [InlineData(@"c:\", @"..\relative")]
    [InlineData(@"c:\Hello\World", @".\..\..\..\Moto")]
    public void InvalidFolder(string absoluteDir, string relativeDir) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFolderPath relative = relativeDir;
        Should.Throw<Exception>(() => absolute + relative);
        Should.Throw<Exception>(() => absolute / relative);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto")]
    [InlineData(@"ram:\Hello\World", @"\Moto", @"ram:\Moto")]
    public void ValidFile(string absoluteDir, string relativeFile, string combinedFile) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFilePath relative = relativeFile;
        AbsoluteFilePath combined = combinedFile;
        (absolute + relative).ShouldBe(combined);
        (absolute / relative).ShouldBe(combined);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative\")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto\")]
    [InlineData(@"ram:\Hello\World", @"\Moto", @"ram:\Moto\")]
    public void ValidFolder(string absoluteDir, string relativeDir, string combinedDir) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFolderPath relative = relativeDir;
        AbsoluteFolderPath combined = combinedDir;
        (absolute + relative).ShouldBe(combined);
        (absolute / relative).ShouldBe(combined);
    }

}