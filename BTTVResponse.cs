using System.Collections.Generic;

namespace BTTVEmoteDownloader
{
    public class ChannelEmote    {
        public string Id { get; set; } 
        public string Code { get; set; } 
        public string ImageType { get; set; } 
        public string UserId { get; set; } 
    }

    public class User    {
        public string Id { get; set; } 
        public string Name { get; set; } 
        public string DisplayName { get; set; } 
        public string ProviderId { get; set; } 
    }

    public class SharedEmote    {
        public string Id { get; set; } 
        public string Code { get; set; } 
        public string ImageType { get; set; } 
        public User User { get; set; } 
    }

    public class Channel    {
        public string Id { get; set; } 
        public List<object> Bots { get; set; } 
        public List<ChannelEmote> ChannelEmotes { get; set; } 
        public List<SharedEmote> SharedEmotes { get; set; } 
    }

}