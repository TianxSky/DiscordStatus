using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Core;
using Discord;

namespace DiscordStatus
{
    public class Chores : IChores
    {
        private readonly Globals _g;

        public Chores(Globals globals)
        {
            _g = globals;
        }

        private EmbedConfig EConfig => _g.EConfig;

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

        public void SortPlayers()
        {
            _g.TPlayersName.Clear();
            _g.CtPlayersName.Clear();
            var sorted = _g.PlayerList?.OrderByDescending(i => i.Kills);
            _g.PlayerList = sorted.ToList();
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