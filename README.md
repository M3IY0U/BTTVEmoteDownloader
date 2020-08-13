# BTTV Emote Downloader
Basic tool to download all bttv emotes of a channel at once.
### Usage
Clone the repo and run 
`dotnet run <id> [discord]` in the cloned folder.
> Where `id` is the id at the end of a bttv channel page. ![Example ID](https://i.imgur.com/XACL5YH.png)\
And `discord` is literally just the optional string "discord" to indicate whether you want emotes that are <256kb so you can upload them to a discord server or not.

So `dotnet run 5b0ff7e982acd052d9f5c7d1 discord` would download all bttv emotes from [Nalian](https://twitch.tv/nalian)'s channel and save them with a size lower than 256kb.