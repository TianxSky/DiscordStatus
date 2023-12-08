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
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            //RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        }

        private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {      
            CCSPlayerController ? player = @event.Userid;
            if (!IsPlayerValid(player)) return HookResult.Continue;
            AddTimer(2.0f, () =>
            {
                PlayerInfo playerInfo = new()
                {
                    UserId = player?.UserId,
                    Index = (int)player?.Index,
                    SteamId = player?.AuthorizedSteamID?.SteamId64.ToString(),
                    Name = player?.PlayerName,
                    IpAddress = player?.IpAddress?.Split(":")[0],
                    Clan = player?.Clan
                };
                if (HasRC)
                {
                    Task.Run(async () => playerInfo.Region = await IPQueryAsync(playerInfo.IpAddress, "region_code") ?? string.Empty);
                }

                if (HasCC)
                {
                    var geoService = new GeoLocationService(new HttpClient());
                    Task.Run(async () => playerInfo.Country = await geoService.GetCountryCodeFromIpAsync(playerInfo.IpAddress));
                    //Task.Run(async () => playerInfo.Country = await IPQueryAsync(playerInfo.IpAddress, "country_code") ?? string.Empty);
                }

                PlayerList.Add(playerInfo);
            });
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController _player = @event.Userid;

            if (!IsPlayerValid(_player)) return HookResult.Continue;
            var theplayer = PlayerList.Find(player => player.UserId == _player.UserId);
            if (theplayer != null)
            {
                PlayerList.Remove(theplayer);
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
            if (!IsPlayerValid(_player)) return HookResult.Continue;
            var playerList = PlayerList;
            AddTimer(5.0f, () =>
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
                if (!IsPlayerValid(player)) continue;
                UpdatePlayer(player);
            }
            var _teams = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
            GetScore(_teams);

            return HookResult.Continue;
        }

        private void OnMapStart(string mapName)
        {
            DSLog.Log(1, $"Map {mapName} started!");
            MapName = mapName;
            HostPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString();
            MaxPlayers = Server.MaxPlayers;
            LoadDiscordStatusAsync().Wait();
        }
    }
}
