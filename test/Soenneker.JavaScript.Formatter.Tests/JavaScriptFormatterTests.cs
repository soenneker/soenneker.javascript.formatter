using System;
using System.IO;
using System.Threading.Tasks;
using Soenneker.JavaScript.Formatter.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.JavaScript.Formatter.Tests;

[Collection("Collection")]
public sealed class JavaScriptFormatterTests : FixturedUnitTest
{
    private readonly IJavaScriptFormatter _util;

    public JavaScriptFormatterTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IJavaScriptFormatter>(true);
    }

    [Fact]
    public async ValueTask PrettyPrint_should_format_script()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        const string source = "function test(){const value={a:1,b:2};if(value.a){return value.b+1;}return 0;}";

        string result = await _util.PrettyPrint(source, cancellationToken);

        Assert.Contains(Environment.NewLine, result);
        Assert.Contains("function test()", result);
        Assert.Contains("    const value =", result);
        Assert.Contains("        return value.b + 1;", result);
    }

    [Fact]
    public async ValueTask Normalize_should_compact_script()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        const string source = """
                              function test() {
                                  const value = { a: 1, b: 2 };
                                  return value.a + value.b;
                              }
                              """;

        string result = await _util.Normalize(source, cancellationToken);

        Assert.Equal("function test(){const value={a:1,b:2};return value.a+value.b}", result);
    }

    [Fact]
    public async ValueTask SavePrettyPrintedFile_should_write_destination_file()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-js-formatter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            string sourcePath = Path.Combine(directory, "source.js");
            string destinationPath = Path.Combine(directory, "formatted.js");

            await File.WriteAllTextAsync(sourcePath, "if(true){console.log('x')}", cancellationToken);

            await _util.SavePrettyPrintedFile(sourcePath, destinationPath, log: false, cancellationToken);

            string result = await File.ReadAllTextAsync(destinationPath, cancellationToken);

            Assert.Contains("if (true)", result);
            Assert.Contains("console.log('x')", result);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
