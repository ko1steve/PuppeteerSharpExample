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
            Console.Write("Please enter the url : ");
            string url = Console.ReadLine();
            bool isSuccess = CovertHtmlToPdf(url).GetAwaiter().GetResult();
            return (isSuccess ? 0 : 1);
        }

        static async Task<bool> CovertHtmlToPdf(string url)
        {
            string pdfFileDir = ConfigurationManager.AppSettings["PDF_FILE_DIR"];
            string pdfFileName = ConfigurationManager.AppSettings["PDF_FILE_NAME"];

            var launchOptions = new LaunchOptions
            {
                Headless = true
            };

            bool useChromeInOs = Boolean.Parse(ConfigurationManager.AppSettings["USE_CHROME_IN_OS"]);
            if (useChromeInOs)
            {
                launchOptions.ExecutablePath = ConfigurationManager.AppSettings["CHROME_PATH"];
            }

            await new BrowserFetcher().DownloadAsync();

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                try
                {
                    await page.GoToAsync(url);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                    return false;
                }
                if (!Directory.Exists(pdfFileDir)) { 
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
