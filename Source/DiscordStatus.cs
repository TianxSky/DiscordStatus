using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace DiscordStatus
{
    public partial class DiscordStatus : BasePlugin, IPluginConfig<DSconfig>
    {
        private System.Timers.Timer? _update;

        private readonly IWebhook _webhook;
        private readonly IQuery _query;
        private readonly IChores _chores;
        private readonly Globals _g;
        private bool init = false;
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
            if (config.Version == null || config.Version < _g.Config.Version)
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
            _g.ConnectURL = _chores.IsURLValid(_g.GConfig.PHPURL) ? string.Concat(_g.GConfig.PHPURL, $"?ip={_g.ServerIP}") : "ConnectURL Error";
            //webhookClient.ModifyWebhookAsync(x => x.Image = );
            await _webhook.InitialMessageAsync();
            if (_g.MessageID != 0)
            {
                _update = new System.Timers.Timer(TimeSpan.FromSeconds(_g.GConfig.UpdateInterval).TotalMilliseconds);
                _update.Elapsed += async (sender, e) => await UpdateAsync();
                _update.Start();
                DSLog.Log(1, "Initialization completed successfully!");
            }
        }

        public async Task UpdateAsync()
        {
            await Task.Run(() =>
            {
                Server.NextFrame(() =>
                {
                    var _players = Utilities.GetPlayers().Where(p => _chores.IsPlayerValid(p));
                    foreach (var player in _players)
                    {
                        _chores.UpdatePlayer(player);
                    }

                    var players = _g.PlayerList;

                    if (players.Count > 0)
                    {
                        _chores.SortPlayers();

                        var tPlayerList = players
                            .Where(kv => kv.Value != null && kv.Value.TeamID == 2)
                            .Select(kv => _chores.FormatStats(kv.Value));

                        var ctPlayerList = players
                            .Where(kv => kv.Value != null && kv.Value.TeamID == 3)
                            .Select(kv => _chores.FormatStats(kv.Value));

                        _g.TPlayersName.AddRange(tPlayerList);
                        _g.CtPlayersName.AddRange(ctPlayerList);
                    }
                });
            });

            await _webhook.UpdateEmbed();
        }
    }
}