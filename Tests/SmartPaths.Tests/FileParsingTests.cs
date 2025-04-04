﻿using Shouldly;

namespace SmartPaths;

public class FileParsingTests
{

    [Theory]
    [InlineData(@"")]
    [InlineData(@".\")]
    [InlineData(@".\filename.ext")]
    [InlineData(@"c:")]
    [InlineData(@"c:\")]
    [InlineData(@"c:\filename\")]
    [InlineData(@"c:\filename.ext\")]
    public void InvalidAbsoluteFile(string source) {
        Should.Throw<Exception>(() => new AbsoluteFilePath(source));
    }

    [Theory]
    [InlineData(@"")]
    [InlineData(@".\")]
    [InlineData(@"c:\filename")]
    public void InvalidRelativeFile(string source) {
        Should.Throw<Exception>(() => new RelativeFilePath(source));
    }

    [Theory]
    //Drive Rooted
    [InlineData(@"c:\filename", @"c:\filename", "")]
    [InlineData(@"c:\filename.ext", @"c:\filename.ext", "ext")]
    [InlineData(@"c:\filename.ext.gz", @"c:\filename.ext.gz", "gz")]
    //network share
    [InlineData(@"\\server\share\filename", @"\\server\share\filename", "")]
    [InlineData(@"\\server\share\filename.ext", @"\\server\share\filename.ext", "ext")]
    //RAM
    [InlineData(@"ram:\filename", @"ram:\filename", "")]
    [InlineData(@"ram:\filename.ext", @"ram:\filename.ext", "ext")]
    public void ValidAbsoluteFile(string source, string clean, string extension) {
        AbsoluteFilePath file = source;
        file.ToString().ShouldBe(clean);
        file.FileExtension.ShouldBe(extension);
    }

    [Theory]
    //explicit current directory
    [InlineData(@".\filename", @".\filename", "")]
    [InlineData(@".\filename.ext", @".\filename.ext", "ext")]
    [InlineData(@".\filename.ext.gz", @".\filename.ext.gz", "gz")]
    //parent directory
    [InlineData(@"..\filename", @"..\filename", "")]
    [InlineData(@"..\filename.ext", @"..\filename.ext", "ext")]
    [InlineData(@"..\filename.ext.gz", @"..\filename.ext.gz", "gz")]
    //current directory
    [InlineData(@"filename", @".\filename", "")]
    [InlineData(@"filename.ext", @".\filename.ext", "ext")]
    //Root Relative
    [InlineData(@"\filename", @"\filename", "")]
    [InlineData(@"\filename.ext", @"\filename.ext", "ext")]
    public void ValidRelativeFile(string source, string clean, string extension) {
        RelativeFilePath file = source;
        file.ToString().ShouldBe(clean);
        file.FileExtension.ShouldBe(extension);
    }

}