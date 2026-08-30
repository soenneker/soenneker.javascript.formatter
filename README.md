[![](https://img.shields.io/nuget/v/soenneker.javascript.formatter.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.javascript.formatter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.javascript.formatter/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.javascript.formatter/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.javascript.formatter/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.javascript.formatter/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.javascript.formatter.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.javascript.formatter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.javascript.formatter/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.javascript.formatter/actions/workflows/codeql.yml)

# Soenneker.JavaScript.Formatter

Parses and serializes JavaScript as readable or compact source, with helpers for files and directories.

## Install

```bash
dotnet add package Soenneker.JavaScript.Formatter
```

## Register

```csharp
using Soenneker.JavaScript.Formatter.Registrars;

services.AddJavaScriptFormatterAsSingleton();
```

A scoped registration is available through `AddJavaScriptFormatterAsScoped()`.

## Format source

```csharp
using Soenneker.JavaScript.Formatter.Abstract;

string readable = await formatter.PrettyPrint(
    "function total(a,b){return a+b}",
    cancellationToken);

string compact = await formatter.Normalize(readable, cancellationToken);
```

`PrettyPrint()` uses four-space indentation and K&R-style braces. `Normalize()` emits compact source; it is a canonical serializer, not an optimizing minifier.

Both methods parse into an AST and generate new source. They are not lossless: comments, original whitespace, quote choices, and semicolon choices may not be preserved. Do not use these APIs when license comments, source-map comments, or exact source text must survive.

The parser accepts JavaScript scripts and modules and retries the alternate source type when needed. Invalid JavaScript and unsupported syntax such as TypeScript annotations cause an `Acornima.ParseErrorException`. Null, empty, or whitespace-only input produces an empty string.

## Format files

```csharp
string preview = await formatter.PrettyPrintFile(
    "src/app.js",
    cancellationToken: cancellationToken);

await formatter.SavePrettyPrintedFile(
    sourcePath: "src/app.js",
    destinationPath: "output/app.js",
    cancellationToken: cancellationToken);

await formatter.SaveNormalizedFile(
    sourcePath: "src/app.js",
    cancellationToken: cancellationToken); // overwrites src/app.js
```

The read methods return transformed source without modifying the file. The `Save` methods write to the destination, or overwrite the source when no destination is supplied.

## Format a directory

```csharp
await formatter.PrettyPrintDirectory(
    "src",
    recursive: true,
    cancellationToken: cancellationToken);
```

This overwrites `.js`, `.mjs`, and `.cjs` files in place. Cancellation does not restore files already written. The token is observed before parsing and throughout file operations, but the synchronous parser itself cannot be interrupted after it starts processing one source string.
