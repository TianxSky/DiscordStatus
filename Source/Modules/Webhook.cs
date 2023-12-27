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

        private List<DiscordWebhookClient> CreateWebhookClients(string url)
        {
            List<DiscordWebhookClient> clients = new();

            string[] list = url.Split(',', StringSplitOptions.TrimEntries);
            foreach (string u in list)
            {
                if (_chores.IsURLValid(u))
                {
                    clients.Add(new DiscordWebhookClient(u));
                }
                else
                {
                    DSLog.Log(2, "Invalid webhook URL.");
                }
            }

            if (clients.Count == 0)
            {
                DSLog.Log(2, "No valid webhook URLs provided.");
            }

            return clients;
        }

        public async Task InitialMessageAsync()
        {
            DSLog.Log(0, "Initializing Embed");
            try
            {
                List<DiscordWebhookClient> webhookClients = CreateWebhookClients(WConfig.StatusWebhookURL);
                foreach (DiscordWebhookClient webhookClient in webhookClients)
                {
                    if (webhookClient == null)
                    {
                        continue;
                    }

                    if (WConfig.StatusMessageID == 0)
                    {
                        DSLog.Log(2, "MessageID is not set up yet, Creating a new one now!");
                        ulong message = await webhookClient.SendMessageAsync(embeds: new[] { CreateStatusEmbed() });
                        _g.MessageID = message;
                        await ConfigManager.SaveAsync("WebhookConfig", "StatusMessageID", _g.MessageID);
                    }
                    else
                    {
                        _g.MessageID = WConfig.StatusMessageID;
                        Embed embed = CreateStatusEmbed();
                        using (webhookClient) // Dispose of the client after use
                        {
                            await webhookClient.ModifyMessageAsync(_g.MessageID, properties =>
                            {
                                properties.Embeds = new[] { embed };
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DSLog.Log(2, "Failed Initializing: " + ex.ToString());
            }
        }

        public async Task UpdateEmbed()
        {
            List<DiscordWebhookClient> webhookClients = CreateWebhookClients(WConfig.StatusWebhookURL);
            foreach (DiscordWebhookClient webhookClient in webhookClients)
            {
                if (webhookClient == null)
                {
                    continue;
                }

                try
                {
                    using (webhookClient) // Dispose of the client after use
                    {
                        await webhookClient.ModifyMessageAsync(_g.MessageID, properties =>
                        {
                            properties.Embeds = new[] { CreateStatusEmbed() };
                        });
                    }
                    DSLog.Log(1, $"Updated embed!");
                }
                catch (Exception ex)
                {
                    DSLog.Log(2, $"Error updating embed: {ex.Message}");
                }
            }
        }

        public Embed CreateStatusEmbed()
        {
            List<string> tplayersName = _g.TPlayersName;
            List<string> ctplayersName = _g.CtPlayersName;
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
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true);
                _ = EConfig.PlayersInline ? builder.AddField("ㅤ", "​─────────────────────────────────────") : null;
                _ = builder
                    .AddField(EConfig.CTField.Replace("{SCORE}", _g.CTScore.ToString()), ctnames, inline: EConfig.PlayersInline)
                    .AddField(EConfig.TField.Replace("{SCORE}", _g.TScore.ToString()), tnames, inline: EConfig.PlayersInline)
                    .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]({_g.ConnectURL})ㅤ{EConfig.JoinHere}" : $"**`connect {_g.ServerIP}`**ㅤ{EConfig.JoinHere}")
                    .WithColor(_chores.GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = _chores.IsURLValid(_g.ConnectURL) ? builder.WithUrl(_g.ConnectURL) : null;
                _ = _chores.IsURLValid(EConfig.MapImg) ? builder.WithImageUrl(EConfig.MapImg.Replace("{MAPNAME}", _g.MapName)) : null;
                return builder.Build();
            }
            else
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .AddField(EConfig.MapField, $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, $"```ansi\n[2;33m[2;31m{EConfig.ServerEmpty}[0m[2;33m[0m[2;33m[0m\n```", inline: true)
                    .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]( {_g.ConnectURL})ㅤ{EConfig.JoinHere}" : $"**`connect {_g.ServerIP}`**ㅤ{EConfig.JoinHere}")
                    .WithColor(_chores.GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = _chores.IsURLValid(_g.ConnectURL) ? builder.WithUrl(_g.ConnectURL) : null;
                _ = _chores.IsURLValid(EConfig.IdleImg) ? builder.WithImageUrl(EConfig.IdleImg.Replace("{MAPNAME}", _g.MapName)) : null;
                return builder.Build();
            }
        }

        public async Task RequestPlayers(string name)
        {
            List<DiscordWebhookClient> webhookClients = CreateWebhookClients(WConfig.RequestPlayersURL);
            foreach (DiscordWebhookClient webhookClient in webhookClients)
            {
                if (webhookClient == null)
                {
                    continue;
                }

                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .WithDescription($"||<@&{WConfig.NotifyMembersRoleID}>||\n```ansi\r\n\u001b[2;31m{name} {EConfig.RequestPlayers}\u001b[0m\r\n```")
                    .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true)
                    .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]({_g.ConnectURL})ㅤ{EConfig.JoinHere}" : $"**`connect {_g.ServerIP}`**ㅤ{EConfig.JoinHere}")
                    .WithColor(new Color(255, 0, 0))
                    .WithCurrentTimestamp();
                _ = _chores.IsURLValid(EConfig.RequestImg) ? builder.WithImageUrl(EConfig.RequestImg.Replace("{MAPNAME}", _g.MapName)) : null;
                _ = builder.Build();
                using (webhookClient) // Dispose of the client after use
                {
                    await webhookClient.SendMessageAsync(embeds: new[] { builder.Build() });
                }
            }
        }

        public async Task NewMap(string mapname, int counts)
        {
            List<DiscordWebhookClient> webhookClients = CreateWebhookClients(WConfig.NotifyWebhookURL);
            foreach (DiscordWebhookClient webhookClient in webhookClients)
            {
                if (webhookClient == null)
                {
                    continue;
                }

                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title)
                    .WithDescription($"```ansi\r\n\u001b[2;31m{EConfig.MapChange.Replace("{mapname}", mapname)}\u001b[0m\r\n```")
                    .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{mapname}\u001b[0m\r\n```", inline: true)
                    .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{counts}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true)
                    .AddField("ㅤ", _chores.IsURLValid(GConfig.PHPURL) ? $"[**`connect {_g.ServerIP}`**]({_g.ConnectURL})ㅤ{EConfig.JoinHere}" : $"**`connect {_g.ServerIP}`**ㅤ{EConfig.JoinHere}")
                    .WithColor(_chores.GetEmbedColor())
                    .WithCurrentTimestamp();
                _ = _chores.IsURLValid(_g.ConnectURL) ? builder.WithUrl(_g.ConnectURL) : null;
                _ = _chores.IsURLValid(EConfig.MapImg) ? builder.WithImageUrl(EConfig.MapImg.Replace("{MAPNAME}", mapname)) : null;
                _ = builder.Build();
                using (webhookClient) // Dispose of the client after use
                {
                    _ = await webhookClient.SendMessageAsync(embeds: new[] { builder.Build() });
                }
            }
        }

        public async Task GameEnd(string mvp)
        {
            List<DiscordWebhookClient> webhookClients = CreateWebhookClients(WConfig.ScoreboardURL);
            foreach (DiscordWebhookClient webhookClient in webhookClients)
            {
                if (webhookClient == null)
                {
                    continue;
                }

                List<string> tplayersName = _g.TPlayersName;
                List<string> ctplayersName = _g.CtPlayersName;
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
                    EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle(EConfig.Title)
                        .WithDescription($"```ansi\r\n\u001b[2;31mServer: {_g.ServerIP}\nGameID: {timestamp} \u001b[0m\r\n```Time: <t:{timestamp}:f>")
                        .AddField($"{EConfig.MapField}", $"```ansi\r\n\u001b[2;31m{_g.MapName}\u001b[0m\r\n```", inline: true)
                        .AddField(EConfig.OnlineField, $"```ansi\r\n\u001b[2;31m{_g.PlayerList.Count}\u001b[0m/\u001b[2;32m{_g.MaxPlayers}\u001b[0m\r\n```", inline: true)
                        .AddField($"{EConfig.MVPField}", $"{mvp}", inline: false)
                        .AddField(EConfig.CTField.Replace("{SCORE}", _g.CTScore.ToString()), ctnames, inline: EConfig.PlayersInline)
                        .AddField(EConfig.TField.Replace("{SCORE}", _g.TScore.ToString()), tnames, inline: EConfig.PlayersInline)
                        .WithColor(_chores.GetEmbedColor())
                        .WithCurrentTimestamp();
                    using (webhookClient) // Dispose of the client after use
                    {
                        _ = await webhookClient.SendMessageAsync(embeds: new[] { builder.Build() });
                    }
                }
            }
        }

        public async Task ServerOffiline()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            List<DiscordWebhookClient> webhookClients = CreateWebhookClients(WConfig.StatusWebhookURL);
            foreach (DiscordWebhookClient webhookClient in webhookClients)
            {
                if (webhookClient == null)
                {
                    continue;
                }
                await webhookClient.ModifyMessageAsync(_g.MessageID, properties =>
                {
                    EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(EConfig.Title + " (Offline)")
                    .WithDescription($"```ansi\r\n\u001b[2;31mOffline since: \u001b[0m\r\n```<t:{timestamp}:R>")
                    .WithColor(new Color(255, 0, 0))
                    .WithCurrentTimestamp();
                    _ = _chores.IsURLValid(EConfig.OfflineImg) ? builder.WithImageUrl(EConfig.OfflineImg) : null;
                    properties.Embeds = new[] { builder.Build() };
                });
                using (webhookClient) // Dispose of the client after use
                {
                    // Additional logic if needed after modifying the message
                }
            }
        }
    }
}