using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Discord.Webhook;
using Newtonsoft.Json;


namespace DiscordStatus
{
    public partial class DiscordStatus : BasePlugin, IPluginConfig<DSconfig>
    {
        public override string ModuleName => "DiscordStatus";
        public override string ModuleVersion => "v2.0";
        public override string ModuleAuthor => "Tian";
        public override string ModuleDescription => "Showing Server Status on Discord";
        public DSconfig Config { get; set; } = new();
        List<PlayerInfo> PlayerList = new();
        public string? ServerIP;
        public string? HostPort;
        public string? ConnectURL;

        public int MaxPlayers { get; private set; }
        public int TScore;
        public int CTScore;
        public string? MapName;
        public ulong MessageID;
        public string? FileDir;
        public string? FilePath;
        private System.Timers.Timer? _update;
        public bool HasCC = false;
        public bool HasRC = false;
        List<string> tplayersName = new();
        List<string> ctplayersName = new();

        public void ConfigManager()
        {
            string? parentDirectory = Directory.GetParent(path: Directory.GetParent(ModuleDirectory).FullName)?.FullName;
            FileDir = Path.Combine(parentDirectory, @$"configs/plugins/{ModuleName}");
            FilePath = Path.Combine(FileDir, $"{ModuleName}.json");
        }

        public async Task SaveConfigAsync()
        {
            var json = File.ReadAllText(FilePath);
            var ReadconfigData = JsonConvert.DeserializeObject<DSconfig>(json);
            ReadconfigData.MessageID = MessageID;
            var updatedJson = JsonConvert.SerializeObject(ReadconfigData, Formatting.Indented);
            await File.WriteAllTextAsync(FilePath, updatedJson);
            DSLog.Log(1, "Saved MessageID to config");
        }
        public void RenameConfig()
        {
            var oldConfigName = Path.GetFileNameWithoutExtension(FilePath) + "(old).json";
            var oldConfigPath = Path.Combine(FileDir, oldConfigName);
            File.Move(FilePath, oldConfigPath);
            string? json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
        public override async void Load(bool hotReload)
        {
            Server.NextFrame(() =>
            {
                MapName = NativeAPI.GetMapName();
            });
            RegisterListeners();
            if (!hotReload)
            {
                if (MapName == null || MapName == "")
                {
                    DSLog.Log(2, "Map Invalid, Waiting Listeners");
                }
                else
                {
                    await LoadDiscordStatusAsync();
                    DSLog.Log(0, $"Map valid ({MapName}), starting bot!");
                }
                DSLog.Log(1, $"{ModuleName} {ModuleVersion} Loaded");
            }
            else
            {
                HostPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString();
                DSLog.Log(0, "Hot Reloading, try starting bot!");
                await LoadDiscordStatusAsync();
            }
        }

        public override void Unload(bool hotReload)
        {
            ServerOffiline();
            _update?.Stop();
            _update?.Dispose();
            DSLog.Log(2, $"{ModuleName} version {ModuleVersion} unloaded");
        }

        public void OnConfigParsed(DSconfig config)
        {
            ConfigManager();
            if (config.Version < Config.Version)
            {
                DSLog.Log(2, $"Config version mismatch (Expected: {Config.Version} | Current: {config.Version})");
                RenameConfig();
                DSLog.Log(1, "Renamed old one, go update your config");
            }
            else
            {
                Config = config;
                HasCC = Config.NameFormat.Contains("{CC}") || Config.PlayersFlagMode;
                HasRC = Config.NameFormat.Contains("{RC}");
                DSLog.Log(1, "Finished loading config file");
            }
        }


        private async Task LoadDiscordStatusAsync()
        {
            DSLog.Log(0, "Starting~");
            await GetIP();
            ConnectURL = IsURLValid(Config.PHPURL) ? string.Concat(Config.PHPURL, $"?ip={ServerIP}:{HostPort}") : "ConnectURL Error";
            if (IsURLValid(Config.WebhookURL))
            {
                var webhookClient = new DiscordWebhookClient(Config.WebhookURL);
                //webhookClient.ModifyWebhookAsync(x => x.Image = );

                if (Config.MessageID == null || Config.MessageID == 0)
                {
                    DSLog.Log(2, "MessageID is null, Creating a new one now!");
                    var id = await webhookClient.SendMessageAsync(embeds: new[] { CreateStatusEmbed(tplayersName, ctplayersName) });
                    MessageID = id;
                    SaveConfigAsync();
                }
                else
                {
                    MessageID = Config.MessageID;
                    await webhookClient.ModifyMessageAsync(MessageID, properties =>
                    {
                        properties.Embeds = new[] { CreateStatusEmbed(tplayersName, ctplayersName) };
                    });

                    _update = new System.Timers.Timer(TimeSpan.FromSeconds(Config.UpdateInterval).TotalMilliseconds);
                    _update.Elapsed += (sender, e) => Update();
                    _update.Start();
                    DSLog.Log(1, "Initialization completed successfully!");
                }
                webhookClient.Dispose();
            }
            else
            {
                DSLog.Log(2, "Webhook URL is not set up");
            }
        }

        public void Update()
        {
            Server.NextFrame(() =>
            {
                var _players = Utilities.GetPlayers();
                foreach (var _player in _players)
                {
                    UpdatePlayer(_player);
                }
            });
            var players = PlayerList;
            if (players.Count > 0)
            {
                SortPlayers();
                var tPlayerList = players.Where(player => player.TeamID == 2).Select(player => FormatStats(player));
                var crPlayerList = players.Where(player => player.TeamID == 3).Select(player => FormatStats(player));
                tplayersName.AddRange(tPlayerList);
                ctplayersName.AddRange(crPlayerList);
            };
            UpdateEmbed(tplayersName, ctplayersName);
        }








        /* alternative api for getting region code but i dont wanna use it
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
                    DSLog.Log(2, $"Failed to get region code: {ex.Message}");
                    return "Error";
                }
            }
        }
        */
        /*
                private void SetupGameEvents()
                {
                    RegisterEventHandler<EventGameEnd>((@event , info) =>
                    {

                    });
                    DSLog.Log(0, $"EventGameEnd: {info}");
                    return HookResult.Continue;
                }

                private async Task SnapShot()
                {
                    var players = Utilities.GetPlayers();
                    await SortPlayers(players);


                }
                public Embed CreateSnapshotEmbed(List<string> tplayersName, List<string> ctplayersName)
                {  
                    var builder = new EmbedBuilder()
                        .WithTitle(_SnapShotTitle)
                        .AddField($"ㅤCT : {GetScore(CsTeam.CounterTerrorist)}", $"```ansi\r\n\u001b[0;34m{string.Join("\n", ctplayersName)}\u001b[0;0m\r\n```", inline: _PlayersInline)
                        .AddField($"ㅤT : {GetScore(CsTeam.Terrorist)}", $"```ansi\r\n\u001b[0;33m{string.Join("\n", tplayersName)}\u001b[0;0m\r\n```", inline: _PlayersInline)
                        .AddField("ㅤ", !string.IsNullOrWhiteSpace(_PHPURL) ? $"[**`connect {ServerIP}:{HostPort.ToString()}`**]({ConnectURL})ㅤ👈 Join Here" : $"**`connect {ConVar.Find("ip")!.StringValue}:{HostPort.ToString()}`**ㅤ👈 Join Here")
                        .WithImageUrl(_MapImg.Replace("{MAPNAME}", NativeAPI.GetMapName()))
                        .WithColor(GetEmbedColor())
                        .WithCurrentTimestamp();
                    return builder.Build();
                }
        */


    }
}