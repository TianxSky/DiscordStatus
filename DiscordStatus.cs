using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using Discord;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using Discord.Rest;
using System.Net;
using System.Timers;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace DiscordStatus
{
    public class DiscordStatus : BasePlugin, IPluginConfig<DSconfig>
    {
        public override string ModuleName => "DiscordStatus";
        public override string ModuleVersion => "v1.3";
        public override string ModuleAuthor => "Tian";
        public override string ModuleDescription => "Server Status on Discord";
        public int _UpdateIntervals;
        public string _BotToken;
        public ulong _ChannelID;
        public ulong _MessageID;
        public string _MapImg;
        public string _Title;
        public string _NameFormat;
        public string _phpurl;

        public DSconfig Config { get; set; } = new();
        public EmbedColorConfig _EmbedColor;
        private System.Timers.Timer _update;
        private DiscordSocketClient _client;
        private IUserMessage _message;

        public string _Map;
        public string _Online;
        public string _Score;
        public string _Players; 
        public string IPAddress { get; private set; }
        public int PlayerCounts = 0;

        List<string> tplayersName = new List<string>();
        List<string> ctplayersName = new List<string>();

        public void OnConfigParsed(DSconfig config)
        {
            LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Loading config file");
            this.Config = config;
            this._UpdateIntervals = config.UpdateIntervals;
            this._BotToken = config.BotToken;
            this._ChannelID = config.ChannelID;
            this._MessageID = config.MessageID;
            this._MapImg = config.MapImg;
            this._Title = config.Title;
            this._NameFormat = config.NameFormat;
            this._phpurl = config.phpurl;
            this._EmbedColor = new EmbedColorConfig
            {
                R = config.EmbedColor.R,
                G = config.EmbedColor.G,
                B = config.EmbedColor.B,
                Random = config.EmbedColor.Random
            };
            this._Map = config.Map;
            this._Online = config.Online;
            this._Score = config.Score;
            this._Players = config.Players;
            LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Finished Loading config file");
        }

        public class EmbedColorConfig
        {
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
            public bool Random { get; set; }

        }

        public override async void Load(bool hotReload)
        {
            if (!hotReload)
            {
                if (NativeAPI.IsMapValid(NativeAPI.GetMapName()))
                {
                    LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> Map invalid, starting listeners!");
                    StartListeners();
                }
                else {
                    LoadDiscordStatusAsync(NativeAPI.GetMapName());
                    LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> Map valid, starting bot!");
                }
                LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> {ModuleName} version {ModuleVersion} loaded");
            }
        }

        private void StartListeners()
        {  
            RegisterListener<Listeners.OnMapStart>(mapName =>
            {   
                LoadDiscordStatusAsync(mapName);
                LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> Map {mapName} started!");
            });
        }

        public override void Unload(bool hotReload)
        {
            LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> {ModuleName} version {ModuleVersion} unloaded");
        }

        private async Task LoadDiscordStatusAsync(string mapName)
        {
            LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Trying to connect to Discord");
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, _BotToken);
            await _client.StartAsync();
            await getIP();
            _client.Ready += async () =>
            {                    
                try
                {
                    LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Client is now Online");
                    /*
                        foreach (var player in players)
                        {
                            if (!player.IsValid || player == null || !player.PlayerPawn.IsValid || player.IsBot)
                            {
                                var playerName = FormatName(player);
                                if (player.PlayerPawn.Value.TeamNum == 2)
                                    tplayersName.Add(playerName);
                                else
                                {
                                    ctplayersName.Add(playerName);
                                }
                            }
                        }
                    */

                    var channel = await _client.GetChannelAsync(_ChannelID) as IMessageChannel;
                    var players = Utilities.GetPlayers();
                    await SortPlayers(players);

                    if (_MessageID == 0)
                    {
                        var message = await channel!.SendMessageAsync(embed: CreateEmbed(IPAddress, tplayersName, ctplayersName, mapName));
                        var message_id = message.Id;
                        _message = message;
                        await channel!.SendMessageAsync($"Please save this message id in the config file: {message_id}");
                    }
                    else
                    {
                        _message = await channel.GetMessageAsync(_MessageID) as IUserMessage;
                        await _message.ModifyAsync(msg => msg.Embed = CreateEmbed(IPAddress, tplayersName, ctplayersName, mapName));
                    }
                    LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Finished initializing Message");
                    _update = new System.Timers.Timer(TimeSpan.FromSeconds(_UpdateIntervals).TotalMilliseconds);
                    _update.Elapsed += async (sender, e) => UpdateEmbed(sender, e).ConfigureAwait(false);
                    _update.Start();
                }
                catch (Exception ex)
                {
                    LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> " + ex.Message);
                }
            };   
        }

        private Task Log(LogMessage arg)
        {
            LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> {arg.Message}");
            return Task.CompletedTask;
        }

        public Embed CreateEmbed(string IPAddress, List<string> tplayersName, List<string> ctplayersName, string mapName)
        {
            string connectUrl = String.Concat(_phpurl, $"?ip={IPAddress}:{ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString()}");   
            var players = Utilities.GetPlayers();
            var PlayerCounts = players.Count();
            if (PlayerCounts > 0)
            {
                var builder = new EmbedBuilder()
                    .WithTitle(_Title)
                    .AddField(_Map, $"```{mapName}```", inline: true)
                    .AddField(_Online, $"```{PlayerCounts}/{Server.MaxPlayers}```", inline: true)
                    .AddField("---------------------------------------------------", "ㅤ")
                    .AddField($"ㅤCT : {GetScore(CsTeam.CounterTerrorist)}", $"```ansi\r\n\u001b[0;34m{string.Join("\n", ctplayersName)}\u001b[0;0m\r\n```", inline: true)
                    .AddField($"ㅤT : {GetScore(CsTeam.Terrorist)}", $"```ansi\r\n\u001b[0;33m{string.Join("\n", tplayersName)}\u001b[0;0m\r\n```", inline: true)
                    .AddField("ㅤ", !string.IsNullOrWhiteSpace(_phpurl) ? $"[**`connect {IPAddress}:{ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString()}`**]({connectUrl})ㅤ👈 Join Here" : $"**`connect {ConVar.Find("ip")!.StringValue}:{ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString()}`**ㅤ👈 Join Here")
                    .WithImageUrl(_MapImg.Replace("{MAPNAME}", NativeAPI.GetMapName()))
                    .WithColor(GetEmbedColor())
                    .WithCurrentTimestamp();
                return builder.Build();
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle(_Title)
                    .AddField(_Map, $"```{mapName}```", inline: true)
                    .AddField(_Online, "```ansi\n[2;33m[2;31mServer Empty[0m[2;33m[0m[2;33m[0m\n```", inline: true)
                    .AddField("ㅤ", !string.IsNullOrWhiteSpace(_phpurl) ? $"[**`connect {IPAddress}:{ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString()}`**]({connectUrl})ㅤ👈 Join Here" : $"**`connect {ConVar.Find("ip")!.StringValue}:{ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString()}`**ㅤ👈 Join Here")
                    .WithImageUrl(_MapImg.Replace("{MAPNAME}", NativeAPI.GetMapName()))
                    .WithColor(GetEmbedColor())
                    .WithCurrentTimestamp();
                return builder.Build();
            }
        }

        public Color GetEmbedColor()
        {
            if (_EmbedColor.Random)
            {
                byte[] randomBytes = new byte[3];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }

                return new Color(randomBytes[0], randomBytes[1], randomBytes[2]);
            }
            else
            {
                return new Color(_EmbedColor.R, _EmbedColor.G, _EmbedColor.B);
            }
        }


        private async Task UpdateEmbed(object sender, ElapsedEventArgs e)
        {
            LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Updating Embed");
            var players = Utilities.GetPlayers();
            if (_client != null && _message != null)
            {
                var channel = await _client.GetChannelAsync(_ChannelID) as SocketTextChannel;
                var mapName = NativeAPI.GetMapName();
                if (channel != null)
                {
                    await SortPlayers(players);
                    await _message.ModifyAsync(msg => msg.Embed = CreateEmbed(IPAddress, tplayersName, ctplayersName, mapName));
                }
            }
            LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Finished Updating Embed");
        }

        public int GetScore(CsTeam team)
        {
            var teamManagers = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");

            foreach (var teamManager in teamManagers)
            {
                if ((int)team == teamManager.TeamNum)
                {
                    return teamManager.Score;
                }
            }

            return 0;
        }
        
        public async Task SortPlayers(List<CCSPlayerController> SortPlayers)
        {
            tplayersName.Clear();
            ctplayersName.Clear();
            var validPlayers = SortPlayers
                .Where(player => player.IsValid && player.PlayerPawn.IsValid && !player.IsBot);


            var sortedPlayers = validPlayers
                .OrderByDescending(player => player.ActionTrackingServices.MatchStats.Kills);


            foreach (var player in sortedPlayers)
            {
                var playerName = await FormatNameAsync(player);
                if (player.PlayerPawn.Value.TeamNum == 2)
                    tplayersName.Add(playerName);
                else if (player.PlayerPawn.Value.TeamNum == 3)
                    ctplayersName.Add(playerName);
            }
        }


        public async Task<string> FormatNameAsync(CCSPlayerController? Player)
        {
            string RC = await GetRegionCodeAsync(Player.IpAddress.Split(':')[0]);
            int kills = Player.ActionTrackingServices.MatchStats.Kills;
            int deaths = Player.ActionTrackingServices.MatchStats.Deaths;
            string kdRatio = deaths != 0 ? (kills / (double)deaths).ToString("G2") : kills.ToString();

            return _NameFormat
                .Replace("{NAME}", Player.PlayerName.Length > 9 ? Player.PlayerName.Substring(0, 9) : Player.PlayerName)
                .Replace("{K}", kills.ToString())
                .Replace("{D}", deaths.ToString())
                .Replace("{A}", Player.ActionTrackingServices.MatchStats.Assists.ToString())
                .Replace("{KD}", kdRatio)
                .Replace("{CLAN}", Player.Clan)
                .Replace("{RC}", RC);
        }

        public async Task getIP()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.ipify.org";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                IPAddress = await response.Content.ReadAsStringAsync();
                LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> Finished getting IP Address: {IPAddress}");
            }
        }
        
        static async Task<string> GetRegionCodeAsync(string ipAddress)
        {
            string apiUrl = $"https://ipapi.co/{ipAddress}/region_code/";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(apiUrl);
                    return response.Trim();
                }
                catch (HttpRequestException ex)
                {
                    // Handle the exception appropriately
                    Console.WriteLine($"Error: {ex.Message}");
                    return "Error";
                }
            }
        }
        /*
        public async Task<string> GetRegionCodeAsync(string apiKey, string ipAddress)
        {
            string apiUrl = $"http://ipinfo.io/{ipAddress}/json?token={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(apiUrl);
                    JObject jsonResponse = JObject.Parse(response);

                    // Extract the region code (adjust key based on actual JSON structure)
                    string regionCode = jsonResponse["region"]?.ToString();

                    return regionCode?.Trim() ?? "N/A";
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.LogToConsole(ConsoleColor.Red, $"[Discord Status] -> Failed to get region code: {ex.Message}");
                    return "Error";
                }
            }
        }
        */

        /*private HookResult EventGameEnd(EventGameEnd @event, GameEventInfo info)
        {
            _countRounds++;
            var maxrounds = ConVar.Find("mp_maxrounds").GetPrimitiveValue<int>();
            if (_countRounds == (maxrounds - _config.VotingRoundInterval))
            {
                VoteMap(false);
            }
            else if (_countRounds == maxrounds)
            {
                Server.ExecuteCommand(!IsWsMaps(_selectedMap)
                    ? $"map {_selectedMap}"
                    : $"ds_workshop_changelevel {_selectedMap}");
            }

            return HookResult.Continue;
        }
        */
    }
}