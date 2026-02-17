# HtmlPdfConverter

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/HtmlPdfConverter)](https://www.nuget.org/packages/HtmlPdfConverter)
[![Tests](https://img.shields.io/badge/tests-xUnit-green)](https://github.com/username/HtmlPdfConverter/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)


**HtmlPdfConverter** is a .NET 8 library for generating PDF documents from HTML templates. It supports **Handlebars** and **Stubble** template engines, rendering PDFs via **headless Chrome** or **Firefox** using PuppeteerSharp. The library allows dynamic data binding by passing a **dictionary** of placeholder keys and values to replace content in your templates.

---

## Features

- Generate PDFs from **HTML strings**, **streams**, or **files**  
- Replace template placeholders dynamically using a **`Dictionary<string, object>`**  
- Supports **Handlebars** and **Stubble** template engines  
- Render PDFs using **Chrome** or **Firefox** browsers  
- Configure PDF options: paper size, margins, etc. via `PdfOptions`  
- Robust error handling with custom exceptions  

---
## Usage Example

```csharp
using HtmlPdfConverter;
using PuppeteerSharp.Media;

var template = "<html><body>Hello {{name}}, Age: {{age}}</body></html>";
var variables = new Dictionary<string, object>
{
    { "name", "John" },
    { "age", 30 }
};

var pdfOptions = new PdfOptions { Format = PaperFormat.A4 };

var pdfBytes = await PdfConverter.CreatePDFAsync(
    template,
    variables,                  // Dictionary replaces {{name}} and {{age}}
    LibOptions.HandlebarsLib,   // or LibOptions.StubbleLib
    BrowserOptions.Chrome,       // or BrowserOptions.Firefox
    pdfOptions
);
```

---
## Running Tests

Tests are written using xUnit

Run tests with:

```bash
dotnet test
```

Tests cover:

Generating PDFs from strings, streams, and files

Using different template engines

Rendering with Chrome and Firefox

Handling errors and edge cases

---

## License

This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.

