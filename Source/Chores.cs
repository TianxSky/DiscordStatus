using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Core;
using Discord;
using Discord.Webhook;

namespace DiscordStatus
{
    public partial class DiscordStatus
    {
        internal static bool IsPlayerValid(CCSPlayerController? player)
        {
            return (player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.AuthorizedSteamID != null);
        }
        internal static bool IsURLValid(string? url)
        {
            string? urlPattern = @"^(https?:\/\/[^\s\/$.?#].[^\s]*)$";
            Regex regex = new(urlPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }

        public Embed CreateStatusEmbed(List<string> tplayersName, List<string> ctplayersName)
        {
            string tnames;
            string ctnames;

            if (PlayerList.Count > 0)
            {
                if (HasCC)
                {
                    tnames = !tplayersName.Any() ? "ㅤ" : string.Join("\n", tplayersName);
                    ctnames = !ctplayersName.Any() ? "ㅤ" : string.Join("\n", ctplayersName);
                }
                else
                {
                    ctnames = !ctplayersName.Any() ? "```ㅤ```" : $"```ansi\r\n\u001b[0;34m{string.Join("\n", ctplayersName)}\u001b[0m\r\n```";
                    tnames = !tplayersName.Any() ? "```ㅤ```" : $"```ansi\r\n\u001b[0;33m{string.Join("\n", tplayersName)}\u001b[0m\r\n```";
                }           
                DSLog.Log(0, tnames + " | " + ctnames);

            var builder = new EmbedBuilder()
                    .WithTitle(Config.Title)
                    .AddField($"{Config.MapField}", $"```ansi\r\n\u001b[2;31m{MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(Config.OnlineField, $"```ansi\r\n\u001b[2;31m{PlayerList.Count}\u001b[0m/\u001b[2;32m{MaxPlayers}\u001b[0m\r\n```", inline: true)
                    .AddField("ㅤ", "​─────────────────────────────────────")
                    .AddField(Config.CTField.Replace("{SCORE}", CTScore.ToString()), ctnames, inline: Config.PlayersInline)
                    .AddField(Config.TField.Replace("{SCORE}", TScore.ToString()), tnames, inline: Config.PlayersInline)
                    .AddField("ㅤ", IsURLValid(Config.PHPURL) ? $"[**`connect {ServerIP}:{HostPort}`**]({ConnectURL})ㅤ👈 Join Here" : $"**`connect {ServerIP}:{HostPort}`**ㅤ👈 Join Here")
                    .WithColor(GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = IsURLValid(ConnectURL) ? builder.WithUrl(ConnectURL) : null;
                _ = IsURLValid(Config.MapImg) ? builder.WithImageUrl(Config.MapImg.Replace("{MAPNAME}", MapName)) : null;
                return builder.Build();
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle(Config.Title)
                    .AddField(Config.MapField, $"```ansi\r\n\u001b[2;31m{MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(Config.OnlineField, "```ansi\n[2;33m[2;31mServer Empty[0m[2;33m[0m[2;33m[0m\n```", inline: true)
                    .AddField("ㅤ", IsURLValid(Config.PHPURL) ? $"[**`connect {ServerIP}:{HostPort}`**]({ConnectURL})ㅤ👈 Join Here" : $"**`connect {ServerIP}:{HostPort}`**ㅤ👈 Join Here")
                    .WithColor(GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = IsURLValid(ConnectURL) ? builder.WithUrl(ConnectURL) : null;
                _ = IsURLValid(Config.IdleImg) ? builder.WithImageUrl(Config.IdleImg) : null;
                return builder.Build();
            }
        }

        public Color GetEmbedColor()
        {
            if (Config.RandomColor)
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
                return new Color(uint.Parse(Config.EmbedColor.TrimStart('#'), System.Globalization.NumberStyles.HexNumber));
            }
        }

        public async Task GetIP()
        {
            using HttpClient client = new();
            string? apiUrl = "https://api.ipify.org";
            HttpResponseMessage response = await client.GetAsync(apiUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            ServerIP = await response.Content.ReadAsStringAsync();
            DSLog.Log(0, $"Finished getting IP Address: {ServerIP}");
        }

        public void GetScore(IEnumerable<CCSTeam> Teams)
        {
            foreach (var team in Teams)
            {
                if (team.TeamNum == 2)
                {
                    TScore = team.Score;
                }
                else if (team.TeamNum == 3)
                {
                    CTScore = team.Score;
                }
            }
        }

        public static async Task<string> IPQueryAsync(string ipAddress, string endpoint)
        {
            string apiUrl = $"https://ipapi.co/{ipAddress}/{endpoint}/";

            using HttpClient client = new();
            try
            {
                string response = await client.GetStringAsync(apiUrl).ConfigureAwait(false);
                return response.Trim();
            }
            catch (HttpRequestException ex)
            {
                DSLog.Log(2, ex.Message);
                return "Error";
            }
        }

        public async Task ServerOffiline()
        {
            var webhookClient = new DiscordWebhookClient(Config.WebhookURL);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await webhookClient.ModifyMessageAsync(MessageID, properties =>
            {
                var builder = new EmbedBuilder()
                .WithTitle(Config.Title + " (Offline)")
                .WithDescription($"```ansi\r\n\u001b[2;31mOffline since: \u001b[0m\r\n```<t:{timestamp}:R>")
                .WithColor(new Color(255, 0, 0))
                .WithCurrentTimestamp();
                _ = IsURLValid(Config.OfflineImg) ? builder.WithImageUrl(Config.OfflineImg) : null;
                properties.Embeds = new[] { builder.Build() };
            });
        }

        public void SortPlayers()
        {
            tplayersName.Clear();
            ctplayersName.Clear();
            var sorted = PlayerList?.OrderByDescending(i => i.Kills);
            PlayerList = sorted.ToList();
            DSLog.Log(1, $"Sorted [{PlayerList?.Count}] players");
        }
        public string FormatStats(PlayerInfo playerinfo)
        {
            var nameBuilder = new StringBuilder(Config.NameFormat);
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
        public async Task UpdateEmbed(List<string> tplayers, List<string> ctplayers)
        {
            if (IsURLValid(Config.WebhookURL))
            {
                var webhookClient = new DiscordWebhookClient(Config.WebhookURL);
                try
                {
                    await webhookClient.ModifyMessageAsync(MessageID, properties =>
                    {
                        properties.Embeds = new[] { CreateStatusEmbed(tplayers, ctplayers) };
                    });
                }
                catch (Exception ex)
                {
                    DSLog.Log(2, $"Error updating embed: {ex.Message}");
                }
                webhookClient.Dispose();
            }
            else
            {
                DSLog.Log(2, "Invalid webhook URL!");
            }
        }

        public void UpdatePlayer(CCSPlayerController updatedPlayer)
        {
            AddTimer(2.0f, () =>
            {
                var kills = updatedPlayer.ActionTrackingServices?.MatchStats.Kills ?? 0;
                var deaths = updatedPlayer.ActionTrackingServices?.MatchStats.Deaths ?? 0;
                var assists = updatedPlayer.ActionTrackingServices?.MatchStats.Assists ?? 0;
                var clan = updatedPlayer.Clan;
                var TeamID = updatedPlayer.TeamNum;
                string kdRatio = deaths != 0 ? (kills / (double)deaths).ToString("G2") : kills.ToString();

                if (PlayerList != null)
                {
                    var playerList = PlayerList;
                    PlayerInfo existingPlayer = playerList.Find(player => player.UserId == updatedPlayer.UserId);
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
            });
        }
    }
    public class GeoLocationService
    {
        private readonly HttpClient _httpClient;

        public GeoLocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetCountryCodeFromIpAsync(string ipAddress)
        {
            string requestUri = $"http://ip-api.com/json/{ipAddress}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                return data.countryCode;
            }

            return null; // Or handle errors as needed
        }
    }
}
