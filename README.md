[![](https://img.shields.io/nuget/v/soenneker.javascript.formatter.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.javascript.formatter/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.javascript.formatter/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.javascript.formatter/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.javascript.formatter.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.javascript.formatter/)

# Soenneker.JavaScript.Formatter

Provides utilities for formatting, pretty-printing, normalizing, reading, and saving JavaScript content.

## Install

```bash
dotnet add package Soenneker.JavaScript.Formatter
```

## Quick start

```csharp
using Soenneker.JavaScript.Formatter.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddJavaScriptFormatterAsSingleton();
```

Adds `IJavaScriptFormatter` as a singleton service.

## What you get

- `IJavaScriptFormatter` — Provides utilities for formatting, pretty-printing, normalizing, reading, and saving JavaScript content.
- `JavaScriptFormatterRegistrar` — A utility library that formats and normalizes JavaScript strings and files.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IJavaScriptFormatter.PrettyPrint(javaScript, cancellationToken)` | Pretty-prints the specified JavaScript with indentation and readable formatting. | The pretty-printed JavaScript. |
| `IJavaScriptFormatter.Normalize(javaScript, cancellationToken)` | Normalizes the specified JavaScript into a consistent serialized form without pretty-print indentation. | The normalized JavaScript. |
| `IJavaScriptFormatter.PrettyPrintFile(filePath, log, cancellationToken)` | Reads JavaScript from the specified file and pretty-prints it. | The pretty-printed JavaScript. |
| `IJavaScriptFormatter.NormalizeFile(filePath, log, cancellationToken)` | Reads JavaScript from the specified file and normalizes it. | The normalized JavaScript. |
| `IJavaScriptFormatter.SavePrettyPrintedFile(sourcePath, destinationPath, log, cancellationToken)` | Reads JavaScript from the source file, pretty-prints it, and saves the result. | A task representing the asynchronous save operation. |
| `IJavaScriptFormatter.SaveNormalizedFile(sourcePath, destinationPath, log, cancellationToken)` | Reads JavaScript from the source file, normalizes it, and saves the result. | A task representing the asynchronous save operation. |
| `IJavaScriptFormatter.PrettyPrintDirectory(directoryPath, recursive, log, cancellationToken)` | Formats all JavaScript files in the specified directory and saves the results in place. | A task representing the asynchronous formatting operation. |
| `JavaScriptFormatterRegistrar.AddJavaScriptFormatterAsSingleton(services)` | Adds `IJavaScriptFormatter` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `JavaScriptFormatterRegistrar.AddJavaScriptFormatterAsScoped(services)` | Adds `IJavaScriptFormatter` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
