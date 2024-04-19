using Shouldly;

namespace SmartPaths;

public class AbsolutePathOperatorTests
{

    [Theory]
    [InlineData(@"c:\", @"..\relative")]
    [InlineData(@"c:\Hello\World", @".\..\..\..\Moto")]
    public void FileInvalid(string absoluteDir, string relativeFile) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFilePath relative = relativeFile;
        Should.Throw<Exception>(() => absolute + relative);
        Should.Throw<Exception>(() => absolute / relative);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto")]
    public void FileValid(string absoluteDir, string relativeFile, string combinedFile) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFilePath relative = relativeFile;
        (absolute + relative).ToString().ShouldBe(combinedFile);
        (absolute / relative).ToString().ShouldBe(combinedFile);
    }

    [Theory]
    [InlineData(@"c:\", @"..\relative")]
    [InlineData(@"c:\Hello\World", @".\..\..\..\Moto")]
    public void FolderInvalid(string absoluteDir, string relativeDir) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFolderPath relative = relativeDir;
        Should.Throw<Exception>(() => absolute + relative);
        Should.Throw<Exception>(() => absolute / relative);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative\")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto\")]
    public void FolderValid(string absoluteDir, string relativeDir, string combinedDir) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFolderPath relative = relativeDir;
        (absolute + relative).ToString().ShouldBe(combinedDir);
        (absolute / relative).ToString().ShouldBe(combinedDir);
    }

}