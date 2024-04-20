using System.Text.RegularExpressions;
using Shouldly;

namespace SmartPaths;

public class PatternTests
{

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(" X ")]
    [InlineData("\t")]
    //todo: other invalid formats
    public void InvalidPaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.Unknown);
        match.ShouldBe(Match.Empty);
    }

    [Fact]
    public void NullInputThrows() {
        Assert.Throws<ArgumentNullException>(() => PathPatterns.DeterminePathType(null!));
    }

    [Theory]
    [InlineData(@"c:")]
    [InlineData(@"c:\")]
    [InlineData(@"c:\hello")]
    [InlineData(@"c:\hello\world")]
    [InlineData(@"c:\hello.world")]
    [InlineData(@"c:\.hello")]
    [InlineData(@"c:\..hello")]
    public void ValidDriveLetterPaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.DriveLetter);
        match.Success.ShouldBeTrue();
    }

    [Theory]
    [InlineData(@"\\server\share")]
    [InlineData(@"\\server\share\")]
    [InlineData(@"\\server\share\hello")]
    [InlineData(@"\\server\share\hello\world")]
    [InlineData(@"\\server\share\hello.world")]
    [InlineData(@"\\server\share\.hello")]
    [InlineData(@"\\server\share\..hello")]
    public void ValidNetworkSharePaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.NetworkShare);
        match.Success.ShouldBeTrue();
    }

    [Theory]
    [InlineData(@"ram:")]
    [InlineData(@"ram:\")]
    [InlineData(@"ram:\hello")]
    [InlineData(@"ram:\hello\world")]
    [InlineData(@"ram:\hello.world")]
    [InlineData(@"ram:\.hello")]
    [InlineData(@"ram:\..hello")]
    public void ValidRamDrivePaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.RamDrive);
        match.Success.ShouldBeTrue();
    }

    [Theory]
    [InlineData(@"hello")]
    [InlineData(@"hello.world")]
    [InlineData(@".hello")]
    [InlineData(@"..hello")]
    [InlineData(@"...")] //triple dots is a valid name
    //windows
    [InlineData(@"hello\world")]
    //linux
    [InlineData(@"hello/world")]
    public void ValidRelative_GeneralPaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.Relative);
        match.Success.ShouldBeTrue();
    }

    [Theory]
    //windows
    [InlineData(@"\")]
    [InlineData(@"\hello")]
    [InlineData(@"\hello\world")]
    [InlineData(@"\hello.world")]
    [InlineData(@"\.hello")]
    [InlineData(@"\..hello")]
    //linux
    [InlineData(@"/")]
    [InlineData(@"/hello")]
    [InlineData(@"/hello/world")]
    [InlineData(@"/hello.world")]
    [InlineData(@"/.hello")]
    [InlineData(@"/..hello")]
    public void ValidRelative_RootedPaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.RootRelative);
        match.Success.ShouldBeTrue();
    }

    [Theory]
    [InlineData(@".")]
    [InlineData(@"..")]
    //windows
    [InlineData(@".\")]
    [InlineData(@".\hello")]
    [InlineData(@".\hello\world")]
    [InlineData(@".\hello.world")]
    [InlineData(@".\.hello")]
    [InlineData(@".\..hello")]
    [InlineData(@"..\")]
    [InlineData(@"..\hello")]
    [InlineData(@"..\hello\world")]
    [InlineData(@"..\hello.world")]
    [InlineData(@"..\.hello")]
    [InlineData(@"..\..hello")]
    //linux
    [InlineData(@"./")]
    [InlineData(@"./hello")]
    [InlineData(@"./hello/world")]
    [InlineData(@"./hello.world")]
    [InlineData(@"./.hello")]
    [InlineData(@"./..hello")]
    [InlineData(@"../")]
    [InlineData(@"../hello")]
    [InlineData(@"../hello/world")]
    [InlineData(@"../hello.world")]
    [InlineData(@"../.hello")]
    [InlineData(@"../..hello")]
    public void ValidRelative_SpecialPaths(string value) {
        (PathType pathType, Match? match) = PathPatterns.DeterminePathType(value);
        pathType.ShouldBe(PathType.Relative);
        match.Success.ShouldBeTrue();
    }

}