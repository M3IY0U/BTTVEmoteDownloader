using System;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BTTVEmoteDownloader
{
    class Program
    {
        private static string _channelName;
        private static bool _discordCompatible;

        private static async Task Main()
        {
            Console.Write("Enter channel name: ");
            _channelName = Console.ReadLine();
            Console.Write("Would you like to save images discord-compatible (below 256kb)? (y/n): ");
            _discordCompatible = Console.ReadLine()?.ToLower() == "y";

            var id = ConvertNameToTwitchId(_channelName);
            var channel = GetBttvChannel(id);

            var emotes = channel.ChannelEmotes
                .Select(x => new Emote(x.Code, x.Id, x.ImageType)).ToList();

            emotes.AddRange(channel.SharedEmotes
                .Select(x => new Emote(x.Code, x.Id, x.ImageType)));

            // Create folder
            Directory.CreateDirectory(_channelName);

            var tasks = emotes.Select(DownloadEmoteAsync);
            await Task.WhenAll(tasks);
        }

        private static Channel GetBttvChannel(long id)
        {
            var request = (HttpWebRequest) WebRequest.Create($"https://api.betterttv.net/3/cached/users/twitch/{id}");
            var response = (HttpWebResponse) request.GetResponse();
            using var sr = new StreamReader(response.GetResponseStream() ?? throw new Exception("was"));
            var result = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<Channel>(result);
        }

        private static long ConvertNameToTwitchId(string name)
        {
            // tytyty https://github.com/swiftyspiffy/Twitch-Username-and-User-ID-Translator/
            var request = (HttpWebRequest) WebRequest.Create($"https://api.twitch.tv/kraken/users?login={name}");
            request.Headers["Accept"] = "application/vnd.twitchtv.v5+json";
            request.Headers["Client-ID"] = "abe7gtyxbr7wfcdftwyi9i5kej3jnq"; // not my client id lmao
            var response = (HttpWebResponse) request.GetResponse();
            using var sr = new StreamReader(response.GetResponseStream() ??
                                            throw new Exception(
                                                "what (something went wrong trying to read the response stream)"));
            var result = sr.ReadToEnd();
            var twitchResponse = JsonConvert.DeserializeObject<dynamic>(result);
            return twitchResponse["users"][0]["_id"];
        }

        private static Task DownloadEmoteAsync(Emote emote)
        {
            Console.WriteLine($"Downloading emote \"{emote.Name}\" with size {emote.Url[^2]}.");
            // Retrieve emote data
            var request = (HttpWebRequest) WebRequest.Create(emote.Url);
            using var response = (HttpWebResponse) request.GetResponse();
            var responseStream = response.GetResponseStream();
            var basePath = $"{_channelName}";
            var fn = $"{emote.Name}.{emote.Type}";
            // Save emote data
            using var fileStream = File.Create(Path.Combine(basePath, fn));
            responseStream?.CopyTo(fileStream);

            // Check if emote is intended for discord
            if (!_discordCompatible)
                return Task.CompletedTask;
            if (fileStream.Length <= 256000)
                return Task.CompletedTask;

            Console.WriteLine("Larger than 256kb. Retrying...");
            fileStream.Close();
            var smallerSize = int.Parse(emote.Url[^2].ToString()) - 1;
            if (smallerSize == 0)
            {
                // Give up if already at the smallest size
                Console.WriteLine($"Creating Emote {emote.Name} failed!");
                return Task.CompletedTask;
            }

            // Update URL and try again
            emote.Url = emote.Url.Replace($"{smallerSize + 1}x", $"{smallerSize}x");
            DownloadEmoteAsync(emote);
            return Task.CompletedTask;
        }
    }

    internal class Emote
    {
        public readonly string Name;
        public readonly string Type;
        public string Url;

        public Emote(string name, string id, string type)
        {
            Name = name;
            Url = $"https://cdn.betterttv.net/emote/{id}/3x";
            Type = type;
        }
    }
}