using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Contrib.Extensions;
using PuppeteerSharp.Contrib.Should;
using Shouldly;
using Xunit;

namespace KubeUI.Tests
{
    public class BrowserTests : IAsyncLifetime
    {
        private string BaseAddress;

        private Browser Browser { get; set; }

        public async Task InitializeAsync()
        {
            string filename = "BrowserTestsAddress.config";
            if (File.Exists(filename))
            {
                BaseAddress = File.ReadAllText(filename);
            }
            else
            {
                throw new System.Exception($"Missing {filename}");
            }

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            Browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
        }

        public async Task DisposeAsync()
        {
            await Browser.CloseAsync();
        }

        [Fact]
        public async Task CheckRoot()
        {
            bool hasError = false;
            var page = await Browser.NewPageAsync();

            page.Console += Page_Console;

            void Page_Console(object sender, ConsoleEventArgs e)
            {
                if (e.Message.Type == ConsoleType.Error
                    || e.Message.Text.StartsWith("WASM: [Error]"))// https://github.com/aspnet/AspNetCore/blob/master/src/Components/Blazor/Blazor/src/Services/WebAssemblyConsoleLogger.cs
                {
                    hasError = true;
                }
            }

            await page.GoToAsync(BaseAddress);

            (await page.WaitForSelectorAsync(".content > h2:nth-child(1)")).InnerText().ShouldBe("Welcome to KubeUI!");

            hasError.ShouldBeFalse();

            var perf = await page.EvaluateExpressionAsync<long>("window.performance.timing.domContentLoadedEventEnd - window.performance.timing.navigationStart");
            System.Console.WriteLine($"Load Time: {perf}ms");
        }

        [Fact]
        public async Task CreateDeployment()
        {
            bool hasError = false;
            var page = await Browser.NewPageAsync();

            page.Console += Page_Console;

            void Page_Console(object sender, ConsoleEventArgs e)
            {
                if (e.Message.Type == ConsoleType.Error
                    || e.Message.Text.StartsWith("WASM: [Error]"))// https://github.com/aspnet/AspNetCore/blob/master/src/Components/Blazor/Blazor/src/Services/WebAssemblyConsoleLogger.cs
                {
                    hasError = true;
                }
            }

            await page.GoToAsync(BaseAddress + "/Deployment");

            await page.WaitForSelectorAsync(".main h2 #AddNew");

            await page.ClickAsync(".main h2 #AddNew");

            await page.WaitForXPathAsync("//div[contains(@class,'uic-tree')]/div/div/span[text()='Metadata']");

            var ele = await page.XPathAsync("//div[contains(@class,'uic-tree')]/div/div/span[text()='Metadata']");

            await ele[0].ClickAsync();

            await page.WaitForSelectorAsync("#Name");

            (await page.QuerySelectorAsync("div.col-sm-3:nth-child(3) > div:nth-child(1) > div:nth-child(2)")).TextContent().Trim().ShouldBe("Standard object metadata.");

            (await (await page.QuerySelectorAsync("#Name")).EvaluateFunctionAsync<string>("node => node.value")).ShouldBe("Deployment 0");

            hasError.ShouldBeFalse();

            var perf = await page.EvaluateExpressionAsync<long>("window.performance.timing.domContentLoadedEventEnd - window.performance.timing.navigationStart");
            System.Console.WriteLine($"Load Time: {perf}ms");
        }

        [Fact]
        public async Task LoadDeployment()
        {
            bool hasError = false;
            var page = await Browser.NewPageAsync();

            page.Console += Page_Console;

            void Page_Console(object sender, ConsoleEventArgs e)
            {
                if (e.Message.Type == ConsoleType.Error 
                    || e.Message.Text.StartsWith("WASM: [Error]"))// https://github.com/aspnet/AspNetCore/blob/master/src/Components/Blazor/Blazor/src/Services/WebAssemblyConsoleLogger.cs
                {
                    hasError = true;
                }
            }

            await page.GoToAsync(BaseAddress + "/ImportExport");

            await page.WaitForSelectorAsync("#inputFiles");

            var input = await page.QuerySelectorAsync("#inputFiles");

            await input.UploadFileAsync("Input/LoadDeployment.yaml");

            await page.WaitForSelectorAsync("#readFiles");

            await page.ClickAsync("#readFiles");

            await page.ClickAsync(".sidebar .nav li a[href=\'Deployment\']");

            await page.WaitForSelectorAsync(".main h2 #AddNew");

            (await page.QuerySelectorAsync(".table > tbody:nth-child(2) > tr:nth-child(1) > td:nth-child(1) > a:nth-child(1)")).TextContent().Trim().ShouldBe("postgres");

            (await page.QuerySelectorAsync(".table > tbody:nth-child(2) > tr:nth-child(1) > td:nth-child(2)")).TextContent().Trim().ShouldBe("bot");

            hasError.ShouldBeFalse();

            var perf = await page.EvaluateExpressionAsync<long>("window.performance.timing.domContentLoadedEventEnd - window.performance.timing.navigationStart");
            System.Console.WriteLine($"Load Time: {perf}ms");
        }
    }
}
