using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace DiscordStatus
{
    public partial class DiscordStatus
    {
        private DateTime _globalCooldown = DateTime.MinValue;
        private readonly TimeSpan _globalCooldownDuration = TimeSpan.FromSeconds(60);

        [ConsoleCommand("css_request", "Request players from discord")]
        public async void RequestPlayers(CCSPlayerController? player, CommandInfo command)
        {
            if (_chores.IsPlayerValid(player))
            {
                if (IsGlobalCooldownActive())
                {
                    DSLog.LogToChat(player, "{RED}Command is on global cooldown. Please wait.");
                    return;
                }
                await _webhook.RequestPlayers(player.PlayerName);
                SetGlobalCooldown();
                DSLog.LogToChat(player, "{GREEN}Request Sent");
            }
            else
            {
                await _webhook.RequestPlayers("Admin");
                DSLog.Log(1, $"Request sent");
            }
        }

        [ConsoleCommand("css_update_names", "Update Name formats and save it to config")]
        [CommandHelper(minArgs: 1, usage: "[css_update_names {FLAG} {NAME}: KD | {KD}]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        [RequiresPermissions("@css/root")]
        public async void UpdateNames(CCSPlayerController? player, CommandInfo command)
        {
            _update.Stop();
            var names = command.ArgString;
            _g.NameFormat = names;
            await ConfigManager.SaveAsync("EmbedConfig", "NameFormat", names);
            await UpdateAsync();
            _update.Start();
            if (!_chores.IsPlayerValid(player)) return;
            DSLog.LogToChat(player, $"{{GREEN}}Name format updated to '{names}'!");
        }

        [ConsoleCommand("css_update_settings", "update config settings")]
        [RequiresPermissions("@css/root")]
        public async void UpdateSettings(CCSPlayerController? player, CommandInfo command)
        {
            _update.Stop();
            await ConfigManager.UpdateAsync(_g);
            await UpdateAsync();
            _update.Start();
            if (!_chores.IsPlayerValid(player)) return;
            DSLog.LogToChat(player, $"color: {_g.EConfig.RandomColor}{_chores.GetEmbedColor()}");
            DSLog.LogToChat(player, "{GREEN}Updated config settings!");
        }

        private bool IsGlobalCooldownActive()
        {
            return DateTime.Now - _globalCooldown < _globalCooldownDuration;
        }

        private void SetGlobalCooldown()
        {
            _globalCooldown = DateTime.Now;
        }
    }
}