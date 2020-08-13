using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace BTTVEmoteDownloader
{
    class Program
    {
        private static string _channel;
        private static bool _discordCompatible;
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("ID missing. Usage: dotnet run [id on bttv]");
                return;
            }

            if (args.Length == 2 && args[1] == "discord")
                _discordCompatible = true;

            // Fetch names and urls
            var driver = new FirefoxDriver();
            driver.Navigate().GoToUrl($@"https://betterttv.com/users/{args[0]}");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(x => x.PageSource.Contains("EmoteCards_emoteCards__1lpxg"));
            var emotes = driver.FindElementByClassName("EmoteCards_emoteCards__1lpxg");
            var cards = emotes.FindElements(By.ClassName("EmoteCards_emoteCard__Z9ryt"));
            var list = cards.Select(card => new Emote(card)).ToList();
            _channel = $"{driver.Title.Substring(driver.Title.IndexOf('-') + 1)}";
            driver.Close();

            // Create folder
            Directory.CreateDirectory(_channel);

            var tasks = list.Select(DownloadEmoteAsync);
            await Task.WhenAll(tasks);
        }

        private static Task DownloadEmoteAsync(Emote emote)
        {
            Console.WriteLine($"Downloading emote \"{emote.Name}\" with size {emote.Link[^2]}.");
           // Retrieve emote data
            var request = (HttpWebRequest) WebRequest.Create(emote.Link);
            using var response = (HttpWebResponse) request.GetResponse();
            var responseStream = response.GetResponseStream();
            var basePath = $"{_channel}";
            var fn = $"{emote.Name}.{response.ContentType.Substring(response.ContentType.LastIndexOf('/') + 1)}";
            // Save emote data
            using var fileStream = File.Create(Path.Combine(basePath, fn));
            responseStream?.CopyTo(fileStream);
            
            // Check if emote is intended for discord
            if(!_discordCompatible)
                return Task.CompletedTask;
            if (fileStream.Length <= 256000) 
                return Task.CompletedTask;
            
            Console.WriteLine("Larger than 256kb. Retrying...");
            fileStream.Close();
            var smallerSize = int.Parse(emote.Link[^2].ToString()) - 1;
            if (smallerSize == 0)
            {
                // Give up if we already were at the smallest size
                Console.WriteLine($"Creating Emote {emote.Name} failed!");
                return Task.CompletedTask;
            }
            // Update URL and try again
            emote.Link = emote.Link.Replace($"{smallerSize + 1}x", $"{smallerSize}x");
            DownloadEmoteAsync(emote);
            return Task.CompletedTask;
        }
    }

    internal class Emote
    {
        public readonly string Name;
        public string Link;

        public Emote(IWebElement element)
        {
            Name = element.Text.Contains("\r") ? element.Text.Remove(element.Text.IndexOf('\r')) : element.Text;
            var id = element.GetProperty("href");
            if (id.Contains('/')) id = id.Substring(id.LastIndexOf('/') + 1);
            Link = $"https://cdn.betterttv.net/emote/{id}/3x";
        }
    }
}