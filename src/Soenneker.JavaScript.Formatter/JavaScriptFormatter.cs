using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acornima;
using Acornima.Ast;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.JavaScript.Formatter.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Extensions.String;
using Soenneker.Utils.File.Abstract;

namespace Soenneker.JavaScript.Formatter;

/// <inheritdoc cref="IJavaScriptFormatter"/>
public sealed class JavaScriptFormatter : IJavaScriptFormatter
{
    private static readonly ParserOptions _parserOptions = new()
    {
        EcmaVersion = EcmaVersion.Latest,
        AllowHashBang = true
    };

    private static readonly KnRJavaScriptTextFormatterOptions _prettyFormatterOptions = new()
    {
        Indent = "    ",
        KeepSingleStatementBodyInLine = false,
        KeepEmptyBlockBodyInLine = true,
        UseEgyptianBraces = true
    };

    private static readonly JavaScriptTextWriterOptions _normalizeWriterOptions = new();
    private static readonly AstToJavaScriptOptions _converterOptions = new();

    private readonly IFileUtil _fileUtil;
    private readonly IDirectoryUtil _directoryUtil;

    public JavaScriptFormatter(IFileUtil fileUtil, IDirectoryUtil directoryUtil)
    {
        _fileUtil = fileUtil;
        _directoryUtil = directoryUtil;
    }

    public ValueTask<string> PrettyPrint(string? javaScript, CancellationToken cancellationToken = default) =>
        Process(javaScript, pretty: true, cancellationToken);

    public ValueTask<string> Normalize(string? javaScript, CancellationToken cancellationToken = default) =>
        Process(javaScript, pretty: false, cancellationToken);

    public async ValueTask<string> PrettyPrintFile(string filePath, bool log = true, CancellationToken cancellationToken = default)
    {
        string javaScript = await ReadFile(filePath, log, cancellationToken).NoSync();
        return await PrettyPrint(javaScript, cancellationToken).NoSync();
    }

    public async ValueTask<string> NormalizeFile(string filePath, bool log = true, CancellationToken cancellationToken = default)
    {
        string javaScript = await ReadFile(filePath, log, cancellationToken).NoSync();
        return await Normalize(javaScript, cancellationToken).NoSync();
    }

    public async ValueTask SavePrettyPrintedFile(string sourcePath, string? destinationPath = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        string formatted = await PrettyPrintFile(sourcePath, log, cancellationToken).NoSync();
        await Save(sourcePath, destinationPath, formatted, log, cancellationToken).NoSync();
    }

    public async ValueTask SaveNormalizedFile(string sourcePath, string? destinationPath = null, bool log = true, CancellationToken cancellationToken = default)
    {
        string normalized = await NormalizeFile(sourcePath, log, cancellationToken).NoSync();
        await Save(sourcePath, destinationPath, normalized, log, cancellationToken).NoSync();
    }

    public async ValueTask PrettyPrintDirectory(string directoryPath, bool recursive = false, bool log = true, CancellationToken cancellationToken = default)
    {
        directoryPath.ThrowIfNullOrWhiteSpace();

        List<string> javaScriptFiles = await GetJavaScriptFiles(directoryPath, recursive, cancellationToken).NoSync();

        foreach (string javaScriptFile in javaScriptFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SavePrettyPrintedFile(javaScriptFile, log: log, cancellationToken: cancellationToken).NoSync();
        }
    }

    private Task<string> ReadFile(string filePath, bool log, CancellationToken cancellationToken)
    {
        filePath.ThrowIfNullOrWhiteSpace();

        return _fileUtil.Read(filePath, log, cancellationToken);
    }

    private async ValueTask Save(string sourcePath, string? destinationPath, string content, bool log, CancellationToken cancellationToken)
    {
        string targetPath = string.IsNullOrWhiteSpace(destinationPath) ? sourcePath : destinationPath;

        targetPath.ThrowIfNullOrWhiteSpace();

        string? directory = Path.GetDirectoryName(targetPath);

        if (!string.IsNullOrWhiteSpace(directory))
            await _directoryUtil.Create(directory, log, cancellationToken).NoSync();

        await _fileUtil.Write(targetPath, content, log, cancellationToken).NoSync();
    }

    private async ValueTask<List<string>> GetJavaScriptFiles(string directoryPath, bool recursive, CancellationToken cancellationToken)
    {
        List<string> jsFiles = await _directoryUtil.GetFilesByExtension(directoryPath, ".js", recursive, cancellationToken).NoSync();
        List<string> mjsFiles = await _directoryUtil.GetFilesByExtension(directoryPath, ".mjs", recursive, cancellationToken).NoSync();
        List<string> cjsFiles = await _directoryUtil.GetFilesByExtension(directoryPath, ".cjs", recursive, cancellationToken).NoSync();

        if (mjsFiles.Count > 0)
            jsFiles.AddRange(mjsFiles);

        if (cjsFiles.Count > 0)
            jsFiles.AddRange(cjsFiles);

        return jsFiles;
    }

    private static async ValueTask<string> Process(string? javaScript, bool pretty, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (javaScript.IsNullOrWhiteSpace())
            return string.Empty;

        string input = StripBom(javaScript);
        Program program = ParseProgram(input);
        string output = await Serialize(program, pretty).NoSync();

        return TrimTrailingLineEndings(output);
    }

    private static Program ParseProgram(string input)
    {
        var parser = new Parser(_parserOptions);

        bool looksLikeModule = LooksLikeModule(input);

        if (looksLikeModule)
            return ParseWithFallback(parser, input, parseModuleFirst: true);

        return ParseWithFallback(parser, input, parseModuleFirst: false);
    }

    private static Program ParseWithFallback(Parser parser, string input, bool parseModuleFirst)
    {
        try
        {
            return parseModuleFirst ? parser.ParseModule(input, null) : parser.ParseScript(input, null, strict: false);
        }
        catch (ParseErrorException)
        {
            return parseModuleFirst ? parser.ParseScript(input, null, strict: false) : parser.ParseModule(input, null);
        }
    }

    private static async ValueTask<string> Serialize(Program program, bool pretty)
    {
        var builder = new StringBuilder(256);

        await using var writer = new StringWriter(builder, CultureInfo.InvariantCulture);

        JavaScriptTextWriter textWriter = pretty
            ? new KnRJavaScriptTextFormatter(writer, _prettyFormatterOptions)
            : new JavaScriptTextWriter(writer, _normalizeWriterOptions);

        var converter = new AstToJavaScriptConverter(textWriter, _converterOptions);
        converter.Convert(program);
        textWriter.Finish();

        return builder.ToString();
    }

    private static bool LooksLikeModule(string javaScript)
    {
        return javaScript.Contains("export ", StringComparison.Ordinal) || javaScript.Contains("import ", StringComparison.Ordinal) ||
               javaScript.Contains("import{", StringComparison.Ordinal) || javaScript.Contains("import*", StringComparison.Ordinal);
    }

    private static string StripBom(string value)
    {
        return value.Length > 0 && value[0] == '\uFEFF' ? value[1..] : value;
    }

    private static string TrimTrailingLineEndings(string value)
    {
        return value.TrimEnd('\r', '\n');
    }
}