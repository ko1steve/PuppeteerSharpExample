using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace PuppeteerSharpExample
{
    internal class Program
    {
        static int Main()
        {
            bool isSuccess = CovertHtmlToPdf(GetTargetUrl()).GetAwaiter().GetResult();
            return (isSuccess ? 0 : 1);
        }

        static string GetTargetUrl() {
            bool inputUrl = Boolean.Parse(ConfigurationManager.AppSettings["INPUT_URL_FROM_CONSOLE"]);
            if (inputUrl)
            {
                Console.Write("Please enter the url : ");
                return Console.ReadLine();
            }
            return ConfigurationManager.AppSettings["TARGET_URL"];
        }

        static async Task<bool> CovertHtmlToPdf(string url)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = Boolean.Parse(ConfigurationManager.AppSettings["HEADLESS"])
            };
            bool useChromeInOs = Boolean.Parse(ConfigurationManager.AppSettings["USE_CHROME_IN_OS"]);

            if (useChromeInOs)
            {
                Console.WriteLine("Use Chrome installed on system OS.");
                launchOptions.ExecutablePath = ConfigurationManager.AppSettings["CHROME_PATH"];
            }
            else
            {
                Console.WriteLine("Downloading Chromium ...");
                await new BrowserFetcher().DownloadAsync();
            }
            Console.WriteLine("Launching Chrome ...");
            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                try
                {
                    Console.WriteLine("Open target url ...");
                    await page.GoToAsync(url);
                    int waitForTimeout = Int32.Parse(ConfigurationManager.AppSettings["WAIT_FOR_TIMEOUT"]);
                    if (waitForTimeout > 0) {
                        await page.WaitForTimeoutAsync(waitForTimeout);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                    return false;
                }

                string pdfFileDir = ConfigurationManager.AppSettings["PDF_FILE_DIR"];
                string pdfFileName = ConfigurationManager.AppSettings["PDF_FILE_NAME"];

                if (!Directory.Exists(pdfFileDir))
                {
                    Console.WriteLine("Creating PDF folder ...");
                    Directory.CreateDirectory(pdfFileDir);
                }
                string pdfFullPath = pdfFileDir + "\\" + pdfFileName;
                await page.PdfAsync(pdfFullPath);
                Console.WriteLine("PDF has been generated.");
                await browser.CloseAsync();
                return true;
            }
        }
    }
}
