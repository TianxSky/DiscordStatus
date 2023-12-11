using CounterStrikeSharp.API.Core;
using Discord;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordStatus
{
    public class Chores : IChores
    {
        private readonly Globals _g;
        private readonly EmbedConfig EConfig;

        public Chores(Globals globals)
        {
            _g = globals;
            EConfig = globals.EConfig;

        }
        public bool IsPlayerValid(CCSPlayerController? player)
        {
            return (player != null && player.IsValid && !player.IsBot && !player.IsHLTV);
        }
        public bool IsURLValid(string? url)
        {
            string? urlPattern = @"^(https?:\/\/[^\s\/$.?#].[^\s]*)$";
            Regex regex = new(urlPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }

        public Color GetEmbedColor()
        {
            if (EConfig.RandomColor)
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
                return new Color(uint.Parse(EConfig.EmbedColor.TrimStart('#'), System.Globalization.NumberStyles.HexNumber));
            }
        }

        public void GetScore(IEnumerable<CCSTeam> Teams)
        {
            foreach (var team in Teams)
            {
                if (team.TeamNum == 2)
                {
                    _g.TScore = team.Score;
                }
                else if (team.TeamNum == 3)
                {
                    _g.CTScore = team.Score;
                }
            }
        }

        public void SortPlayers()
        {
            _g.TPlayersName.Clear();
            _g.CtPlayersName.Clear();
            var sorted = _g.PlayerList?.OrderByDescending(i => i.Kills);
            _g.PlayerList = sorted.ToList();
            DSLog.Log(1, $"Sorted [{_g.PlayerList?.Count}] players");
        }
        public string FormatStats(PlayerInfo playerinfo)
        {
            var nameBuilder = new StringBuilder(_g.NameFormat);
            nameBuilder.Replace("{NAME}", playerinfo.Name);
            nameBuilder.Replace("{K}", playerinfo.Kills.ToString());
            nameBuilder.Replace("{D}", playerinfo.Deaths.ToString());
            nameBuilder.Replace("{A}", playerinfo.Assists.ToString());
            nameBuilder.Replace("{KD}", playerinfo.KD);
            nameBuilder.Replace("{CLAN}", playerinfo.Clan);
            nameBuilder.Replace("{CC}", playerinfo.Country);
            nameBuilder.Replace("{FLAG}", $":flag_{playerinfo.Country.ToLower()}:");
            nameBuilder.Replace("{RC}", playerinfo.Region);
            var formattedName = nameBuilder.ToString();
            return formattedName;
        }

        public void UpdatePlayer(CCSPlayerController updatedPlayer)
        {
            var kills = updatedPlayer.ActionTrackingServices?.MatchStats.Kills ?? 0;
            var deaths = updatedPlayer.ActionTrackingServices?.MatchStats.Deaths ?? 0;
            var assists = updatedPlayer.ActionTrackingServices?.MatchStats.Assists ?? 0;
            var clan = updatedPlayer.Clan;
            var TeamID = updatedPlayer.TeamNum;
            string kdRatio = deaths != 0 ? (kills / (double)deaths).ToString("G2") : kills.ToString();
            PlayerInfo existingPlayer = _g.PlayerList.Find(player => player.UserId == updatedPlayer.UserId);
            if (existingPlayer != null)
            {
                existingPlayer.Kills = kills;
                existingPlayer.Deaths = deaths;
                existingPlayer.Assists = assists;
                existingPlayer.Clan = clan;
                existingPlayer.KD = kdRatio;
                existingPlayer.TeamID = TeamID;
            }
        }
    }
}