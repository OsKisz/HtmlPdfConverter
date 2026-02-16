using HtmlPdfConverter;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text;
using Xunit;

namespace HtmlPdfConverter_Tests
{
    [Collection("Serial")]
    public class UnitTests
    {
        private readonly string filePath = "../../../templateHTML.html";
        private readonly Dictionary<string, object> variables = new()
        {
            { "name", "John" },
            { "age", "30" }
        };

        private readonly PdfOptions pdfOptions = new()
        {
            Format = PaperFormat.A4
        };

        private static Stream ToStream(string content) =>
            new MemoryStream(Encoding.UTF8.GetBytes(content));

        [Fact]
        public async Task CreatePDFAsync_WithHandlebarsTemplateAndChrome_ReturnsNonEmptyByteArray()
        {
            string template = "<html><body>Hello {{name}}, Age: {{age}}</body></html>";

            var result = await PdfConverter.CreatePDFAsync(
                template,
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Chrome,
                pdfOptions);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task CreatePDFAsync_WithStubbleTemplate_ReturnsNonEmptyByteArray()
        {
            string template = "<html><body>Hello {{name}}, Age: {{age}}</body></html>";

            var result = await PdfConverter.CreatePDFAsync(
                template,
                variables,
                LibOptions.StubbleLib,
                BrowserOptions.Chrome,
                pdfOptions);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task CreatePDFStreamAsync_ReturnsValidStream()
        {
            string template = "<html><body>User: {{name}}</body></html>";

            await using var stream = await PdfConverter.CreatePDFStreamAsync(
                template,
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Chrome,
                pdfOptions);

            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task CreatePDFAsync_UnsupportedLib_ThrowsException()
        {
            string template = "<html><body>Hello</body></html>";

            await Assert.ThrowsAsync<HtmlPdfConverter_Exceptions>(async () =>
                await PdfConverter.CreatePDFAsync(
                    template,
                    variables,
                    (LibOptions)99,
                    BrowserOptions.Chrome,
                    pdfOptions));
        }

        [Fact]
        public async Task CreatePDFAsync_FromStream_ReturnsValidBytes()
        {
            using var stream = ToStream("<html><body>{{name}}</body></html>");

            var result = await PdfConverter.CreatePDFAsync(
                stream,
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Chrome,
                pdfOptions);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task CreatePDFAsync_FromFile_ReturnsValidBytes()
        {
            // Używamy FileStream bezpośrednio
            using Stream fileTemplate = File.OpenRead(filePath);

            var result = await PdfConverter.CreatePDFAsync(
                fileTemplate,
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Chrome,
                pdfOptions);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task CreatePDFAsync_WithFirefox_ReturnsNonEmptyByteArray()
        {
            string template = "<html><body>Hello {{name}}</body></html>";

            var result = await PdfConverter.CreatePDFAsync(
                template,
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Firefox,
                pdfOptions);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task CreatePDFStreamAsync_DifferentBrowsers_ReturnsValidStream()
        {
            string template = "<html><body>Browser Test</body></html>";

            await using var resultChrome = await PdfConverter.CreatePDFStreamAsync(
                ToStream(template),
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Chrome,
                pdfOptions);

            await using var resultFirefox = await PdfConverter.CreatePDFStreamAsync(
                ToStream(template),
                variables,
                LibOptions.HandlebarsLib,
                BrowserOptions.Firefox,
                pdfOptions);

            Assert.True(resultChrome.Length > 0);
            Assert.True(resultFirefox.Length > 0);
        }

        [Fact]
        public async Task CreatePDFAsync_EmptyTemplate_ThrowsException()
        {
            using var stream = ToStream(string.Empty);

            await Assert.ThrowsAsync<HtmlPdfConverter_Exceptions>(async () =>
                await PdfConverter.CreatePDFAsync(
                    stream,
                    variables,
                    LibOptions.HandlebarsLib,
                    BrowserOptions.Chrome,
                    pdfOptions));
        }

        [Fact]
        public async Task CreatePDFAsync_WithPolishCharacters_RendersCorrectly()
        {
            string template = "<html><body>Zażółć gęślą jaźń: {{value}}</body></html>";
            var vars = new Dictionary<string, object> { { "value", "Łódź" } };

            var result = await PdfConverter.CreatePDFAsync(
                template,
                vars,
                LibOptions.HandlebarsLib,
                BrowserOptions.Chrome,
                pdfOptions);

            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task CreatePDFAsync_WithCancelledToken_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await PdfConverter.CreatePDFAsync(
                    "<html></html>",
                    variables,
                    LibOptions.HandlebarsLib,
                    BrowserOptions.Chrome,
                    pdfOptions,
                    cancellationToken: cts.Token));
        }

        [Fact]
        public async Task CreatePDFAsync_NullVariables_ThrowsHtmlPdfConverter_Exceptions()
        {
            await Assert.ThrowsAsync<HtmlPdfConverter_Exceptions>(async () =>
                await PdfConverter.CreatePDFAsync(
                    "<html></html>",
                    null!,
                    LibOptions.HandlebarsLib,
                    BrowserOptions.Chrome,
                    pdfOptions));
        }
    }
}
