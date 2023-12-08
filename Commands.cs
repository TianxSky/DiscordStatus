using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Discord;
using Discord.Webhook;

namespace DiscordStatus
{
    public partial class DiscordStatus
    {
        [ConsoleCommand("request", "DiscordStaatus")]
        [RequiresPermissions("@css/kick")]
        public async void RequestPlayers(CCSPlayerController? player, CommandInfo command)
        {
            if (IsURLValid(Config.WebhookURL))
            {
                var webhookClient = new DiscordWebhookClient(Config.WebhookURL);
                var builder = new EmbedBuilder()
                    .WithTitle(Config.Title)
                    .WithAuthor(player?.PlayerName)
                    .WithDescription($"||<@&{Config.NotifyMembersRoleID}>||\n```ansi\r\n\u001b[2;31mAdmin {player?.PlayerName} is requesting players to join the server\"\u001b[0m\r\n```")
                    .WithColor(new Color(255, 0, 0))
                    .WithCurrentTimestamp();
                _ = IsURLValid(Config.RequestImg) ? builder.WithImageUrl(Config.RequestImg) : null;
                builder.Build();
                await webhookClient.SendMessageAsync(embeds: new [] { builder.Build()});
            }
            else
            {
                DSLog.Log(2, "Invalid webhook URL!");
            }
        }
    }
}
