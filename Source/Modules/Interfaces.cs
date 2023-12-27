using CounterStrikeSharp.API.Core;
using Discord;

namespace DiscordStatus
{
    public interface IWebhook
    {
        Task InitialMessageAsync();

        Embed CreateStatusEmbed();

        Task ServerOffiline();

        Task UpdateEmbed();

        Task RequestPlayers(string name);

        Task GameEnd(string mvp);

        Task NewMap(string mapname, int playercounts);
    }

    public interface IQuery
    {
        /*Task<string> GetIPAsync();*/

        Task<string> GetCountryCodeAsync(string ipAddress);

        Task<string> IPQueryAsync(string ipAddress, string endpoint);
    }

    public interface IChores
    {
        void InitPlayers(CCSPlayerController player);

        bool IsPlayerValid(CCSPlayerController? player);

        bool IsURLValid(string? url);

        Color GetEmbedColor();

        void GetScore(IEnumerable<CCSTeam> teams);

        void SortPlayers();

        string FormatStats(PlayerInfo playerinfo);

        void UpdatePlayer(CCSPlayerController updatedPlayer);
    }
}