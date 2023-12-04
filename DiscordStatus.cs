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
        public override string ModuleVersion => "v1.1";
        public override string ModuleAuthor => "Tian";
        public DSconfig Config { get; set; } = new();
        public int _UpdateIntervals;
        public string _BotToken;
        public ulong _ChannelID;
        public ulong _MessageID;
        public string _MapImg;
        public string _Title;
        public string _NameFormat;
        public string _phpurl;

        public EmbedColorConfig _EmbedColor;

        public string _Map;
        public string _Online;
        public string _Score;
        public string _Players; 
        public string IPAddress { get; private set; }
        private System.Timers.Timer _update;
        private DiscordSocketClient _client;
        private IUserMessage _message;
        private int PlayerCounts = 0;
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
                await LoadDiscordStatus();
                LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> {ModuleName} version {ModuleVersion} loaded");
            }
        }
        public override void Unload(bool hotReload)
        {
            LogHelper.LogToConsole(ConsoleColor.Green, $"[Discord Status] -> {ModuleName} version {ModuleVersion} unloaded");
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
        private async Task LoadDiscordStatus()
        {
            try
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
                    LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Client is now Online");
                   /*
                    var playerEntities = Utilities.GetPlayers();
                    PlayerCounts = 0;
                    ctplayersName.Clear();
                    tplayersName.Clear();

                    foreach (var player in playerEntities)
                    {
                        if (player.IsValid && player.PlayerPawn.IsValid && !player.IsBot && player.AuthorizedSteamID != null)
                        {
                            PlayerCounts++;
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

                    if (_MessageID == 0)
                    {
                        var message = await channel!.SendMessageAsync(embed: CreateEmbed(IPAddress, PlayerCounts, ctplayersName, tplayersName));
                        var message_id = message.Id;
                        _message = message;
                        await channel!.SendMessageAsync($"Please save this message id in the config file: {message_id}");
                    }
                    else
                    {
                        _message = await channel.GetMessageAsync(_MessageID) as IUserMessage;
                        await _message.ModifyAsync(msg => msg.Embed = CreateEmbed(IPAddress, PlayerCounts, ctplayersName, tplayersName));
                    }
                    LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> Finished initializing Message");
                    _update = new System.Timers.Timer(TimeSpan.FromSeconds(_UpdateIntervals).TotalMilliseconds);
                    _update.Elapsed += async (sender, e) => await UpdateEmbed(sender, e).ConfigureAwait(false);
                    _update.Start();
                };
            }
            catch (Exception ex)
            {
                LogHelper.LogToConsole(ConsoleColor.Green, "[Discord Status] -> " + ex.Message);
            }
        }


        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public Embed CreateEmbed(string IPAddress, int PlayerCounts, List<string> ctplayersName, List<string> tplayersName)
        {
            string connectUrl = String.Concat(_phpurl, $"?ip={IPAddress}:{ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString()}");               
            if (PlayerCounts > 0)
            {
                var builder = new EmbedBuilder()
                    .WithTitle(_Title)
                    .AddField(_Map, $"```{NativeAPI.GetMapName()}```", inline: true)
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
                    .AddField(_Map, $"```{NativeAPI.GetMapName()}```", inline: true)
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
            var playerEntities = Utilities.GetPlayers();
            PlayerCounts = 0;
            tplayersName.Clear();
            ctplayersName.Clear();

            foreach (var player in playerEntities)
            {
                if (player.IsValid && player.PlayerPawn.IsValid && !player.IsBot)
                {
                    PlayerCounts++;
                    var playerName = FormatName(player);
                    if (player.PlayerPawn.Value.TeamNum == 2)
                            tplayersName.Add(playerName);
                    else if (player.PlayerPawn.Value.TeamNum == 3)
                            ctplayersName.Add(playerName);
                }       
            }

            if (_client != null && _message != null)
            {
                var channel = _client.GetChannel(_ChannelID) as SocketTextChannel;
                if (channel != null)
                {
                    await _message.ModifyAsync(msg => { msg.Embed = CreateEmbed(IPAddress, PlayerCounts, ctplayersName, tplayersName); return; });
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

        public string FormatName(CCSPlayerController? Player)
        {
            return _NameFormat
                        .Replace("{NAME}", Player.PlayerName.Length > 9 ? Player.PlayerName.Substring(0, 9) : Player.PlayerName)
                        .Replace("{K}", Player.ActionTrackingServices.MatchStats.Kills.ToString())
                        .Replace("{D}", Player.ActionTrackingServices.MatchStats.Deaths.ToString())
                        .Replace("{A}", Player.ActionTrackingServices.MatchStats.Assists.ToString());
        }
        
    }
}