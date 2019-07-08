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
        private static string BaseAddress = "https://kubeui-ci.netlify.com";

        private Browser Browser { get; set; }

        public async Task InitializeAsync()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
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
                if (e.Message.Type == ConsoleType.Error)
                {
                    hasError = true;
                }
            }

            await page.GoToAsync(BaseAddress);
            await page.WaitForTimeoutAsync(10000);
            hasError.ShouldBeFalse();
        }

        [Fact]
        public async Task CreateDeployment()
        {
            bool hasError = false;
            var page = await Browser.NewPageAsync();

            page.Console += Page_Console;

            void Page_Console(object sender, ConsoleEventArgs e)
            {
                if (e.Message.Type == ConsoleType.Error)
                {
                    hasError = true;
                }
            }

            await page.GoToAsync(BaseAddress + "/Deployment");
            await page.WaitForSelectorAsync(".main h2 #AddNew");

            await page.ClickAsync(".main h2 #AddNew");
            await page.WaitForTimeoutAsync(5000);

            hasError.ShouldBeFalse();
        }

        [Fact]
        public async Task LoadDeployment()
        {
            bool hasError = false;
            var page = await Browser.NewPageAsync();

            page.Console += Page_Console;

            void Page_Console(object sender, ConsoleEventArgs e)
            {
                if (e.Message.Type == ConsoleType.Error)
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
        }
    }
}
