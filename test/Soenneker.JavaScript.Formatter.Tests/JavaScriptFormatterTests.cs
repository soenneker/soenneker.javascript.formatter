using System;
using System.IO;
using System.Threading.Tasks;
using AwesomeAssertions;
using Soenneker.JavaScript.Formatter.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.JavaScript.Formatter.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class JavaScriptFormatterTests : HostedUnitTest
{
    private readonly IJavaScriptFormatter _util;

    public JavaScriptFormatterTests(Host host) : base(host)
    {
        _util = Resolve<IJavaScriptFormatter>(true);
    }

    [Test]
    public async ValueTask PrettyPrint_should_format_script()
    {
        var cancellationToken = System.Threading.CancellationToken.None;
        const string source = "function test(){const value={a:1,b:2};if(value.a){return value.b+1;}return 0;}";

        string result = await _util.PrettyPrint(source, cancellationToken);

        result.Should().Contain(Environment.NewLine);
        result.Should().Contain("function test()");
        result.Should().Contain("    const value =");
        result.Should().Contain("        return value.b + 1;");
    }

    [Test]
    public async ValueTask Normalize_should_compact_script()
    {
        var cancellationToken = System.Threading.CancellationToken.None;
        const string source = """
                              function test() {
                                  const value = { a: 1, b: 2 };
                                  return value.a + value.b;
                              }
                              """;

        string result = await _util.Normalize(source, cancellationToken);

        result.Should().Be("function test(){const value={a:1,b:2};return value.a+value.b}");
    }

    [Test]
    public async ValueTask SavePrettyPrintedFile_should_write_destination_file()
    {
        var cancellationToken = System.Threading.CancellationToken.None;
        string directory = Path.Combine(Path.GetTempPath(), $"soenneker-js-formatter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            string sourcePath = Path.Combine(directory, "source.js");
            string destinationPath = Path.Combine(directory, "formatted.js");

            await File.WriteAllTextAsync(sourcePath, "if(true){console.log('x')}", cancellationToken);

            await _util.SavePrettyPrintedFile(sourcePath, destinationPath, log: false, cancellationToken);

            string result = await File.ReadAllTextAsync(destinationPath, cancellationToken);

            result.Should().Contain("if (true)");
            result.Should().Contain("console.log('x')");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}

