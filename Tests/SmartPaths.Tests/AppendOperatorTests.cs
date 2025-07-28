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
    public void ValidAbsoluteFile(string absoluteDir, string relativeFile, string combinedFile) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFilePath relative = relativeFile;
        AbsoluteFilePath combined = combinedFile;
        (absolute + relativeFile).ShouldBe(combined);
        (absolute + relative).ShouldBe(combined);
        //The slash operator to add a typed file path is just for convenience
        (absolute / relative).ShouldBe(combined);
    }

    [Theory]
    [InlineData(@"c:\", @".\relative", @"c:\relative\")]
    [InlineData(@"c:\Hello\World", @"..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @".\..\Moto", @"c:\Hello\Moto\")]
    [InlineData(@"c:\Hello\World", @"\Moto", @"c:\Moto\")]
    [InlineData(@"ram:\Hello\World", @"\Moto", @"ram:\Moto\")]
    public void ValidAbsoluteFolder(string absoluteDir, string relativeDir, string combinedDir) {
        AbsoluteFolderPath absolute = absoluteDir;
        RelativeFolderPath relative = relativeDir;
        AbsoluteFolderPath combined = combinedDir;
        (absolute / relativeDir).ShouldBe(combined);
        (absolute / relative).ShouldBe(combined);
    }

    [Theory]
    [InlineData(@".\start\", @".\relative", @".\start\relative")]
    [InlineData(@".\start\", @"..\Moto", @".\Moto")]
    [InlineData(@".\start\", @".\..\Moto", @".\Moto")]
    public void ValidRelativeFile(string start, string relativeFile, string combinedFile) {
        RelativeFolderPath startPath = start;
        RelativeFilePath relative = relativeFile;
        RelativeFilePath combined = combinedFile;
        (startPath + relativeFile).ShouldBe(combined);
        (startPath + relative).ShouldBe(combined);
        (startPath / relative).ShouldBe(combined);
    }

    [Theory]
    [InlineData(@".\start\", @".\relative", @".\start\relative\")]
    [InlineData(@".\start\", @"..\Moto", @".\Moto\")]
    [InlineData(@".\start\", @".\..\Moto", @".\Moto\")]
    public void ValidRelativeFolder(string start, string relativeDir, string combinedDir) {
        RelativeFolderPath startPath = start;
        RelativeFolderPath relative = relativeDir;
        RelativeFolderPath combined = combinedDir;
        (startPath / relativeDir).ShouldBe(combined);
        (startPath / relative).ShouldBe(combined);
    }

}