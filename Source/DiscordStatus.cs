using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;


namespace DiscordStatus
{
    public partial class DiscordStatus : BasePlugin, IPluginConfig<DSconfig>
    {
        private System.Timers.Timer? _update;

        private readonly IWebhook _webhook;
        private readonly IQuery _query;
        private readonly IChores _chores;
        private readonly Globals _g;
        public DSconfig Config { get; set; }
        public DiscordStatus(IWebhook webhook, IQuery query, IChores chores, Globals g)
        {
            _webhook = webhook;
            _query = query;
            _chores = chores;
            _g = g;
        }
        public override async void Load(bool hotReload)
        {
            Server.NextFrame(() =>
            {
                _g.MapName = NativeAPI.GetMapName();
            });
            RegisterListeners();
            if (!hotReload)
            {
                if (_g.MapName == null || _g.MapName == string.Empty)
                {
                    DSLog.Log(2, "Map Invalid, Waiting Listeners");
                }
                else
                {
                    await LoadDiscordStatusAsync();
                    DSLog.Log(0, $"Map valid ({_g.MapName}), starting bot!");
                }
                DSLog.Log(1, $"{ModuleName} {ModuleVersion} Loaded");
            }
            else
            {
                _g.HostPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString();
                DSLog.Log(0, "Hot Reloading, try starting bot!");
                await LoadDiscordStatusAsync();
            }
        }

        public override void Unload(bool hotReload)
        {
            _webhook.ServerOffiline();
            _update?.Stop();
            _update?.Dispose();
            DSLog.Log(2, $"{ModuleName} version {ModuleVersion} unloaded");
        }

        public void OnConfigParsed(DSconfig config)
        {
            ConfigManager.GetPath(ModuleDirectory, ModuleName);
            if (config.Version < _g.Config.Version)
            {
                DSLog.Log(2, $"Config version mismatch (Expected: {_g.Config.Version} | Current: {config.Version})");
                Task.Run(async () => await ConfigManager.RenameAsync(_g.Config));
                DSLog.Log(1, "Renamed old one, go update your config");
            }
            else
            {
                Config = config;
                _g.Config = Config;
                _g.GConfig = Config.GeneralConfig;
                _g.WConfig = Config.WebhookConfig;
                _g.EConfig = Config.EmbedConfig;
                _g.NameFormat = _g.EConfig.NameFormat;
                _g.HasCC = _g.EConfig.NameFormat.Contains("{CC}") || _g.EConfig.NameFormat.Contains("{FLAG}");
                _g.HasRC = _g.EConfig.NameFormat.Contains("{RC}");
                _g.ServerIP = _g.GConfig.ServerIP;
                DSLog.Log(1, "Finished loading config file");
            }
        }


        private async Task LoadDiscordStatusAsync()
        {
            DSLog.Log(0, "Starting~");
            _g.ConnectURL = _chores.IsURLValid(_g.GConfig.PHPURL) ? string.Concat(_g.GConfig.PHPURL, $"?ip={_g.ServerIP}:{_g.HostPort}") : "ConnectURL Error";
            if (_chores.IsURLValid(_g.WConfig.StatusWebhookURL))
            {
                //webhookClient.ModifyWebhookAsync(x => x.Image = );
                await _webhook.InitialMessageAsync();
                if (_g.WConfig.StatusMessageID != 0)
                {
                    _update = new System.Timers.Timer(TimeSpan.FromSeconds(_g.GConfig.UpdateInterval).TotalMilliseconds);
                    _update.Elapsed += async (sender, e) => await UpdateAsync();
                    _update.Start();
                    DSLog.Log(1, "Initialization completed successfully!");
                }
            }
            else
            {
                DSLog.Log(2, "Webhook URL is not set up");
            }
        }

        public async Task UpdateAsync()
        {
            await Task.Run(() =>
            {
                Server.NextFrame(() =>
                {
                    AddTimer(2.0f, () =>
                    {
                        var _players = Utilities.GetPlayers();
                        foreach(var _player in _players)
                        {
                            _chores.UpdatePlayer(_player);
                        };
                    });
                });
            });

            var players = _g.PlayerList;

            if (players.Count > 0)
            {
                _chores.SortPlayers();

                var tPlayerList = players.Where(player => player.TeamID == 2).Select(player => _chores.FormatStats(player));
                var crPlayerList = players.Where(player => player.TeamID == 3).Select(player => _chores.FormatStats(player));

                _g.TPlayersName.AddRange(tPlayerList);
                _g.CtPlayersName.AddRange(crPlayerList);
            }

            await _webhook.UpdateEmbed();
        }
    }
}