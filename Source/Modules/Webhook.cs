using Discord;
using Discord.Webhook;

namespace DiscordStatus
{
    public class Webhook : IWebhook
    {
        private readonly IChores _chores;
        private readonly Globals _g;
        private WebhookConfig WConfig => _g.WConfig;
        private EmbedConfig EConfig => _g.EConfig;
        private GeneralConfig GConfig => _g.GConfig;

        public Webhook(IChores chores, Globals globals)
        {
            _g = globals;
            _chores = chores;
        }

        private DiscordWebhookClient CreateWebhookClient(string url)
        {
            if (!_chores.IsURLValid(url))
            {
                DSLog.Log(2, "Invalid webhook URL.");
                return null; // or throw a specific exception, return a default value, etc.
            }
            return new DiscordWebhookClient(url);
        }

        public async Task InitialMessageAsync()
        {
            DSLog.Log(0, "Initializing Embed");
            try
            {
                using var webhookClient = CreateWebhookClient(WConfig.StatusWebhookURL);
                if (webhookClient == null) return;
                if (WConfig.StatusMessageID == 0)
                {
                    DSLog.Log(2, "MessageID is not set up yet, Creating a new one now!");
                    var message = await webhookClient.SendMessageAsync(embeds: new[] { CreateStatusEmbed() });
                    _g.MessageID = message;
                    await ConfigManager.SaveAsync("WebhookConfig", "StatusMessageID", _g.MessageID);
                }
                else
                {
                    _g.MessageID = WConfig.StatusMessageID;
                    var embed = CreateStatusEmbed();
                    await webhookClient.ModifyMessageAsync(_g.MessageID, properties =>
                    {
                        properties.Embeds = new[] { embed };
                    });
                }
            }
            catch (Exception ex)
            {
                DSLog.Log(2, "Failed Initializing: " + ex.ToString());
            }
        }

        public Embed CreateStatusEmbed()
        {
            var tplayersName = _g.TPlayersName;
            var ctplayersName = _g.CtPlayersName;
            string tnames;
            string ctnames;

            if (_g.PlayerList.Count > 0)
            {
                if (_g.HasCC)
                {
                    tnames = !tplayersName.Any() ? "ㅤ" : string.Join("\n", tplayersName);
                    ctnames = !ctplayersName.Any() ? "ㅤ" : string.Join("\n", ctplayersName);
                }
                else
                {
                    ctnames = !ctplayersName.Any() ? "```ㅤ```" : $"```ansi\r\n\u001b[0;34m{string.Join("\n", ctplayersName)}\u001b[0m\r\n```";
                    tnames = !tplayersName.Any() ? "```ㅤ```" : $"```ansi\r\n\u001b[0;33m{string.Join("\n", tplayersName)}\u001b[0m\r\n```";
                }
                var builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true);
                _ = EConfig.PlayersInline ? builder.AddField("ㅤ", "​─────────────────────────────────────") : null;
                builder
                    .AddField(EConfig.CTField.Replace("{SCORE}", _g.CTScore.ToString()), ctnames, inline: EConfig.PlayersInline)
                    .AddField(EConfig.TField.Replace("{SCORE}", _g.TScore.ToString()), tnames, inline: EConfig.PlayersInline)
                    .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]({_g.ConnectURL})ㅤ👈 Join Here" : $"**`connect {_g.ServerIP}`**ㅤ👈 Join Here")
                    .WithColor(_chores.GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = _chores.IsURLValid(_g.ConnectURL) ? builder.WithUrl(_g.ConnectURL) : null;
                _ = _chores.IsURLValid(EConfig.MapImg) ? builder.WithImageUrl(EConfig.MapImg.Replace("{MAPNAME}", _g.MapName)) : null;
                return builder.Build();
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .AddField(EConfig.MapField, $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, "```ansi\n[2;33m[2;31mServer Empty[0m[2;33m[0m[2;33m[0m\n```", inline: true)
                    .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]( {_g.ConnectURL})ㅤ👈 Join Here" : $"**`connect {_g.ServerIP}`**ㅤ👈 Join Here")
                    .WithColor(_chores.GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = _chores.IsURLValid(_g.ConnectURL) ? builder.WithUrl(_g.ConnectURL) : null;
                _ = _chores.IsURLValid(EConfig.IdleImg) ? builder.WithImageUrl(EConfig.IdleImg.Replace("{MAPNAME}", _g.MapName)) : null;
                return builder.Build();
            }
        }

        public async Task UpdateEmbed()
        {
            if (_chores.IsURLValid(_g.WConfig.StatusWebhookURL))
            {
                using var webhookClient = CreateWebhookClient(WConfig.StatusWebhookURL);
                if (webhookClient == null) return;
                try
                {
                    await webhookClient.ModifyMessageAsync(_g.MessageID, properties =>
                    {
                        properties.Embeds = new[] { CreateStatusEmbed() };
                    });
                    DSLog.Log(1, $"Updated embed!");
                }
                catch (Exception ex)
                {
                    DSLog.Log(2, $"Error updating embed: {ex.Message}");
                }
            }
            else
            {
                DSLog.Log(2, "Invalid webhook URL!");
            }
        }

        public async Task RequestPlayers(string name)
        {
            using var webhookClient = CreateWebhookClient(WConfig.RequestPlayersURL);
            if (webhookClient == null) return;
            var builder = new EmbedBuilder()
                .WithTitle(EConfig.Title)
                .WithDescription($"||<@&{WConfig.NotifyMembersRoleID}>||\n```ansi\r\n\u001b[2;31m{name} is requesting players to join the server\u001b[0m\r\n```")
                .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true)
                .WithColor(new Color(255, 0, 0))
                .WithCurrentTimestamp();
            _ = _chores.IsURLValid(EConfig.RequestImg) ? builder.WithImageUrl(EConfig.RequestImg.Replace("{MAPNAME}", _g.MapName)) : null;
            builder.Build();
            await webhookClient.SendMessageAsync(embeds: new[] { builder.Build() });
        }

        public async Task NewMap(string mapname)
        {
            using var webhookClient = CreateWebhookClient(WConfig.NotifyWebhookURL);
            if (webhookClient == null) return;
            var builder = new EmbedBuilder()
                .WithTitle(EConfig.Title)
                .WithDescription($"```ansi\r\n\u001b[2;31mMap changed to {mapname}, Join Now\u001b[0m\r\n```")
                .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{mapname}\u001b[0m\r\n```", inline: true)
                .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true)
                .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]({_g.ConnectURL})ㅤ👈 Join Here" : $"**`connect {_g.ServerIP}`**ㅤ👈 Join Here")
                .WithColor(_chores.GetEmbedColor())
                .WithCurrentTimestamp();
            _ = _chores.IsURLValid(_g.ConnectURL) ? builder.WithUrl(_g.ConnectURL) : null;
            _ = _chores.IsURLValid(EConfig.MapImg) ? builder.WithImageUrl(EConfig.MapImg.Replace("{MAPNAME}", mapname)) : null;
            builder.Build();
            await webhookClient.SendMessageAsync(embeds: new[] { builder.Build() });
        }

        public async Task GameEnd(PlayerInfo mvp, string steamlink)
        {
            using var webhookClient = CreateWebhookClient(WConfig.ScoreboardURL);
            if (webhookClient == null) return;
            var tplayersName = _g.TPlayersName;
            var ctplayersName = _g.CtPlayersName;
            string tnames;
            string ctnames;

            if (_g.PlayerList.Count > 0)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (_g.HasCC)
                {
                    tnames = !tplayersName.Any() ? "ㅤ" : string.Join("\n", tplayersName);
                    ctnames = !ctplayersName.Any() ? "ㅤ" : string.Join("\n", ctplayersName);
                }
                else
                {
                    ctnames = !ctplayersName.Any() ? "```ㅤ```" : $"```ansi\r\n\u001b[0;34m{string.Join("\n", ctplayersName)}\u001b[0m\r\n```";
                    tnames = !tplayersName.Any() ? "```ㅤ```" : $"```ansi\r\n\u001b[0;33m{string.Join("\n", tplayersName)}\u001b[0m\r\n```";
                }
                var builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .WithDescription($"```ansi\r\n\u001b[2;31mServer: {_g.ServerIP}\nGameID: {timestamp} \u001b[0m\r\n```Time: <t:{timestamp}:f>")
                    .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true)
                    .AddField($"{EConfig.MVPField}", $"[{_chores.FormatStats(mvp)}]({steamlink})", inline: false)
                    .AddField(EConfig.CTField.Replace("{SCORE}", _g.CTScore.ToString()), ctnames, inline: EConfig.PlayersInline)
                    .AddField(EConfig.TField.Replace("{SCORE}", _g.TScore.ToString()), tnames, inline: EConfig.PlayersInline)
                    .WithColor(_chores.GetEmbedColor())
                    .WithCurrentTimestamp();
                await webhookClient.SendMessageAsync(embeds: new[] { builder.Build() });
            }
        }

        public async Task ServerOffiline()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            using var webhookClient = CreateWebhookClient(WConfig.StatusWebhookURL);
            if (webhookClient == null)
            {
                return;
            }
            await webhookClient.ModifyMessageAsync(_g.MessageID, properties =>
            {
                var builder = new EmbedBuilder()
                .WithTitle(EConfig.Title + " (Offline)")
                .WithDescription($"```ansi\r\n\u001b[2;31mOffline since: \u001b[0m\r\n```<t:{timestamp}:R>")
                .WithColor(new Color(255, 0, 0))
                .WithCurrentTimestamp();
                _ = _chores.IsURLValid(EConfig.OfflineImg) ? builder.WithImageUrl(EConfig.OfflineImg) : null;
                properties.Embeds = new[] { builder.Build() };
            });
        }
    }
}