using Shouldly;

namespace SmartPaths;

public class QueryParsingTests
{

    [Theory]
    [InlineData(@"")]
    [InlineData(@" ")]
    [InlineData("\t")]
    [InlineData(@"/../nope")]
    //todo: [InlineData(@"...")]
    public Task InvalidQuery(string source) {
        Should.Throw<Exception>(() => new AbsoluteQuery(source));
        Should.Throw<Exception>(() => new RelativeQuery(source));
        Should.Throw<Exception>(() => SmartPath.ParseQuery(source));
        SmartPath.TryParse(source, out IPath? _).ShouldBeFalse();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(@"c:")]
    [InlineData(@"c:\")]
    [InlineData(@"c:\some\path")]
    [InlineData(@"/")]
    [InlineData(@"/some/path")]
    [InlineData(@"/some/../path")]
    public Task ValidAbsoluteQuery(string source) {
        IQuery query = SmartPath.ParseQuery(source);
        query.ShouldBe(new AbsoluteQuery(source));
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(@".")]
    [InlineData(@"./")]
    [InlineData(@".\")]
    public Task ValidRelativeQuery(string source) {
        IQuery query = SmartPath.ParseQuery(source);
        query.ShouldBe(new RelativeQuery(source));
        return Task.CompletedTask;
    }

}