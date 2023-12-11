using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace DiscordStatus
{
    public partial class DiscordStatus
    {
        public int deaths = 0;
        private void RegisterListeners()
        {
            RegisterListener<Listeners.OnMapStart>(OnMapStart);

            RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterEventHandler<EventCsWinPanelMatch>(OnGameEnd);
            //RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventGameNewmap>(OnGameNewmap);
        }

        private void OnMapStart(string mapName)
        {
            DSLog.Log(1, $"Map {mapName} started!");
            _g.MapName = mapName;
            _g.HostPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString();
            _g.MaxPlayers = Server.MaxPlayers;
            Task.Run(async () => await LoadDiscordStatusAsync());
        }
        private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (!_chores.IsPlayerValid(player)) return HookResult.Continue;
            /*AddTimer(2.0f, () =>
            {*/
                PlayerInfo playerInfo = new()
                {
                    UserId = player?.UserId,
                    Index = (int)player?.Index,
                    SteamId = player?.AuthorizedSteamID?.SteamId64.ToString(),
                    Name = player?.PlayerName,
                    IpAddress = player?.IpAddress?.Split(":")[0],
                    Clan = player?.Clan
                };
                if (_g.HasRC)
                {
                    Task.Run(async () => playerInfo.Region = await _query.IPQueryAsync(playerInfo.IpAddress, "region_code").ConfigureAwait(false) ?? string.Empty);
                }

                if (_g.HasCC)
                {
                    Task.Run(async () => playerInfo.Country = await _query.GetCountryCodeAsync(playerInfo.IpAddress).ConfigureAwait(false) ?? string.Empty);
                }

                _g.PlayerList.Add(playerInfo);
           /* });*/
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController _player = @event.Userid;

            if (!_chores.IsPlayerValid(_player)) return HookResult.Continue;
            var theplayer = _g.PlayerList.Find(player => player.UserId == _player.UserId);
            if (theplayer != null)
            {
                _g.PlayerList.Remove(theplayer);
                DSLog.Log(0, $"Removed {theplayer.Name}'s cache");
            }
            else
            {
                DSLog.Log(2, $"Could not find player {_player.UserId}");
            }
            return HookResult.Continue;
        }

        private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
        {
            var _player = @event.Userid;
            var teamid = @event.Team;
            if (teamid == 0) return HookResult.Continue;
            if (!_chores.IsPlayerValid(_player)) return HookResult.Continue;
            var playerList = _g.PlayerList;
            AddTimer(2.0f, () =>
            {
                PlayerInfo existingPlayer = playerList.Find(player => player.UserId == _player.UserId);
                if (existingPlayer != null)
                {
                    existingPlayer.TeamID = teamid;
                }
                else
                {
                    DSLog.Log(0, $"Player not found!");
                }
            });
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!_chores.IsPlayerValid(player)) continue;
                _chores.UpdatePlayer(player);
            }
            var _teams = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
            _chores.GetScore(_teams);

            return HookResult.Continue;
        }
        private HookResult OnGameNewmap(EventGameNewmap @event, GameEventInfo info)
        {
            if (!_g.WConfig.NewMapNotification) return HookResult.Continue;
            _g.MapName = @event.Mapname;
            Task.Run(() => _webhook.NewMap(@event.Mapname));
            return HookResult.Continue;
        }
        private HookResult OnGameEnd(EventCsWinPanelMatch @event, GameEventInfo info)
        {
            if (!_g.WConfig.GameEndScoreboard) return HookResult.Continue;
            var _players = Utilities.GetPlayers();
            foreach (var _player in _players)
            {
                _chores.UpdatePlayer(_player);
            };
            var players = _g.PlayerList;

            if (players.Count > 0)
            {
                _chores.SortPlayers();

                var tPlayerList = players.Where(player => player.TeamID == 2).Select(player => _chores.FormatStats(player));
                var crPlayerList = players.Where(player => player.TeamID == 3).Select(player => _chores.FormatStats(player));

                _g.TPlayersName.AddRange(tPlayerList);
                _g.CtPlayersName.AddRange(crPlayerList);
            }
            var mvp = _g.PlayerList.OrderByDescending(player => player.Kills).First();
            if (mvp == null)
            {
                DSLog.Log(2, "Mvp not found");
                return HookResult.Continue;
            }
            var mvpname = mvp.Name;
            var steamid = mvp.SteamId;
            var steamlink = $"https://steamcommunity.com/profiles/{steamid}";
            DSLog.Log(1, $"Game ended! MVP: {mvpname}");
            Task.Run(() => _webhook.GameEnd(mvp, steamlink));
            return HookResult.Continue;
        }
    }
}
