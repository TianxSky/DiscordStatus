using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace DiscordStatus
{
    public partial class DiscordStatus
    {
        public int deaths = 0;

        private void RegisterListeners()
        {
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnClientAuthorized>((slot, steamid) =>
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
                if (!_chores.IsPlayerValid(player)) return;
                AddTimer(1.0f, () =>
                {
                    _chores.InitPlayers(player);
                });
            });
            //RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterEventHandler<EventCsWinPanelMatch>(OnGameEnd);
            //RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            //RegisterEventHandler<EventGameNewmap>(OnGameNewmap); somehow this doesnt work
        }

        private void OnMapStart(string mapName)
        {
            if (!init)
            {
                DSLog.Log(1, $"Map {mapName} started!");
                _g.MapName = mapName;
                _g.MaxPlayers = Server.MaxPlayers;
                Task.Run(LoadDiscordStatusAsync);
                init = true;
            }
            else
            {
                DSLog.Log(1, $"Map {_g.MapName} changed to {mapName}!");
                if (!_g.WConfig.NewMapNotification) return;
                var playercounts = Utilities.GetPlayers().Where(_chores.IsPlayerValid).Count();
                _webhook.NewMap(mapName, playercounts);
                _g.MapName = mapName;
            }
        }

        /*private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (!_chores.IsPlayerValid(player)) return HookResult.Continue;
            AddTimer(1.0f, () =>
            {
                _chores.InitPlayers(player);
            });
            return HookResult.Continue;
        }*/

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController _player = @event.Userid;

            if (!_chores.IsPlayerValid(_player)) return HookResult.Continue;
            if (_g.PlayerList.TryGetValue(_player.Slot, out var theplayer))
            {
                _g.PlayerList.Remove(_player.Slot);
                DSLog.Log(0, $"Removed {theplayer.Name}'s cache");
            }
            else
            {
                DSLog.Log(2, $"Could not find player {_player.PlayerName}");
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
            AddTimer(1.0f, () =>
            {
                if (playerList.TryGetValue(_player.Slot, out var value))
                {
                    value.TeamID = teamid;
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

                var tPlayerList = players
                    .Where(kv => kv.Value != null && kv.Value.TeamID == 2)
                    .Select(kv => _chores.FormatStats(kv.Value));

                var crPlayerList = players
                    .Where(kv => kv.Value != null && kv.Value.TeamID == 3)
                    .Select(kv => _chores.FormatStats(kv.Value));

                _g.TPlayersName.AddRange(tPlayerList);
                _g.CtPlayersName.AddRange(crPlayerList);
            }

            var _teams = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
            _chores.GetScore(_teams);
            var mvp = _g.TScore >= _g.CTScore ? _g.TPlayersName.FirstOrDefault() : _g.CtPlayersName.FirstOrDefault();
            Task.Run(() => _webhook.GameEnd(mvp));
            return HookResult.Continue;
        }
    }
}