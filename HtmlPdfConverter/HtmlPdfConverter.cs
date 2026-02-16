using System.Collections.Concurrent;
using HandlebarsDotNet;
using PuppeteerSharp;
using Stubble.Core.Builders;
using Stubble.Core.Interfaces;

namespace HtmlPdfConverter
{
    public class PdfConverter
    {
        private static readonly SemaphoreSlim _browserDownloadLock = new(1, 1);
        private static readonly HashSet<SupportedBrowser> _downloadedBrowsers = new();

        private static readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _handlebarsCache = new();
        private static readonly IStubbleRenderer _stubble = new StubbleBuilder().Build();

        private const int HandlebarsCacheLimit = 100;
        public static async Task<byte[]> CreatePDFAsync(
            string template,
            IDictionary<string, object> variables,
            LibOptions libOptions,
            BrowserOptions browserOptions,
            PdfOptions pdfOptions,
            string? browserPath = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(template))
                    throw new HtmlPdfConverter_Exceptions("Template cannot be null or empty.");
                if (variables == null)
                    throw new HtmlPdfConverter_Exceptions("Variables cannot be null.");
                if (pdfOptions == null)
                    throw new HtmlPdfConverter_Exceptions("PDF options cannot be null.");

                string processedTemplate = libOptions switch
                {
                    LibOptions.HandlebarsLib => GetOrAddHandlebarsTemplate(template)(variables),
                    LibOptions.StubbleLib => _stubble.Render(template, variables),
                    _ => throw new HtmlPdfConverter_Exceptions("Invalid library option.")
                };

                cancellationToken.ThrowIfCancellationRequested();

                var browserType = browserOptions == BrowserOptions.Chrome ? SupportedBrowser.Chrome : SupportedBrowser.Firefox;

                var fetcher = new BrowserFetcher(new BrowserFetcherOptions
                {
                    Path = browserPath,
                    Browser = browserType
                });

                await EnsureBrowserDownloadedAsync(fetcher).ConfigureAwait(false);

                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = browserPath,
                    Args = new[] {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--font-render-hinting=none"
                    }
                }).ConfigureAwait(false);

                await using var page = await browser.NewPageAsync().ConfigureAwait(false);

                await page.SetContentAsync(processedTemplate, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.Networkidle2 },
                    Timeout = 30_000
                }).ConfigureAwait(false);

                return await page.PdfDataAsync(pdfOptions).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HtmlPdfConverter_Exceptions("PDF generation failed.", ex);
            }
        }
        public static async Task<byte[]> CreatePDFAsync(
            Stream template,
            IDictionary<string, object> variables,
            LibOptions libOptions,
            BrowserOptions browserOptions,
            PdfOptions pdfOptions,
            string? browserPath = null,
            CancellationToken cancellationToken = default)
        {
            if (template.CanSeek)
                template.Position = 0;

            using var reader = new StreamReader(template);
            string templateContent = await reader.ReadToEndAsync().ConfigureAwait(false);
            return await CreatePDFAsync(templateContent, variables, libOptions, browserOptions, pdfOptions, browserPath, cancellationToken).ConfigureAwait(false);
        }
        public static async Task<Stream> CreatePDFStreamAsync(
            string template,
            IDictionary<string, object> variables,
            LibOptions libOptions,
            BrowserOptions browserOptions,
            PdfOptions pdfOptions,
            string? browserPath = null,
            CancellationToken cancellationToken = default)
        {
            byte[] pdfBytes = await CreatePDFAsync(template, variables, libOptions, browserOptions, pdfOptions, browserPath, cancellationToken).ConfigureAwait(false);
            return new MemoryStream(pdfBytes);
        }

        public static async Task<Stream> CreatePDFStreamAsync(
            Stream template,
            IDictionary<string, object> variables,
            LibOptions libOptions,
            BrowserOptions browserOptions,
            PdfOptions pdfOptions,
            string? browserPath = null,
            CancellationToken cancellationToken = default)
        {
            byte[] pdfBytes = await CreatePDFAsync(template, variables, libOptions, browserOptions, pdfOptions, browserPath, cancellationToken).ConfigureAwait(false);
            return new MemoryStream(pdfBytes);
        }

        private static HandlebarsTemplate<object, object> GetOrAddHandlebarsTemplate(string template)
        {
            if (_handlebarsCache.TryGetValue(template, out var compiled))
                return compiled;

            if (_handlebarsCache.Count >= HandlebarsCacheLimit)
            {
                var keyToRemove = _handlebarsCache.Keys.FirstOrDefault();
                if (keyToRemove != null)
                    _handlebarsCache.TryRemove(keyToRemove, out _);
            }

            return _handlebarsCache.GetOrAdd(template, t => Handlebars.Compile(t));
        }

        private static async Task EnsureBrowserDownloadedAsync(BrowserFetcher browserFetcher)
        {
            if (_downloadedBrowsers.Contains(browserFetcher.Browser))
                return;

            await _browserDownloadLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_downloadedBrowsers.Contains(browserFetcher.Browser))
                    return;

                await browserFetcher.DownloadAsync().ConfigureAwait(false);
                _downloadedBrowsers.Add(browserFetcher.Browser);
            }
            finally
            {
                _browserDownloadLock.Release();
            }
        }
    }
}
