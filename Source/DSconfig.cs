namespace DiscordStatus
{
    using System.Text.Json.Serialization;
    using CounterStrikeSharp.API.Core;

    public sealed class DSconfig : BasePluginConfig
    {
        [JsonPropertyName("GeneralConfig")]
        public GeneralConfig GeneralConfig { get; set; } = new GeneralConfig();

        [JsonPropertyName("WebhookConfig")]
        public WebhookConfig WebhookConfig { get; set; } = new WebhookConfig();

        [JsonPropertyName("EmbedConfig")]
        public EmbedConfig EmbedConfig { get; set; } = new EmbedConfig();

        [JsonPropertyName("ConfigVersion")]
        public override int Version { get; set; } = 5;
    }

    public sealed class GeneralConfig
    {
        [JsonPropertyName("ServerIP")]
        public string ServerIP { get; set; } = string.Empty;

        [JsonPropertyName("UpdateInterval")]
        public int UpdateInterval { get; set; } = 45;

        [JsonPropertyName("PHPURL")]
        public string PHPURL { get; set; } = string.Empty;
    }

    public sealed class EmbedConfig
    {
        [JsonPropertyName("Title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("MapImg")]
        public string MapImg { get; set; } = "{MAPNAME}.jpg";

        [JsonPropertyName("OfflineImg")]
        public string OfflineImg { get; set; } = "not_required";

        [JsonPropertyName("IdleImg")]
        public string IdleImg { get; set; } = "not_required";

        [JsonPropertyName("RequestImg")]
        public string RequestImg { get; set; } = "not_required";

        [JsonPropertyName("EmbedColor")]
        public string EmbedColor { get; set; } = "#00ffff";

        [JsonPropertyName("RandomColor")]
        public bool RandomColor { get; set; } = true;

        [JsonPropertyName("MapField")]
        public string MapField { get; set; } = "🗺️ㅤMap";

        [JsonPropertyName("OnlineField")]
        public string OnlineField { get; set; } = "👥ㅤOnline";

        [JsonPropertyName("CTField")]
        public string CTField { get; set; } = " CT :ㅤ{SCORE}";

        [JsonPropertyName("TField")]
        public string TField { get; set; } = " T :ㅤ{SCORE}";

        [JsonPropertyName("MVPField")]
        public string MVPField { get; set; } = " 👑ㅤMVP ";

        [JsonPropertyName("NameFormat")]
        public string NameFormat { get; set; } = "{FLAG}{CLAN} {NAME}: {K} - {D}";

        [JsonPropertyName("EmbedSteamLink")]
        public bool EmbedSteamLink { get; set; } = false;

        [JsonPropertyName("RequestPlayers")]
        public string RequestPlayers { get; set; } = "is requesting players to join the server";

        [JsonPropertyName("MagChange")]
        public string MapChange { get; set; } = "Map changed to {mapname}, Join Now";

        [JsonPropertyName("ServerEmpty")]
        public string ServerEmpty { get; set; } = "Server Empty";

        [JsonPropertyName("JoinHere")]
        public string JoinHere { get; set; } = "👈 Join Here";

        [JsonPropertyName("PlayersInline")]
        public bool PlayersInline { get; set; } = true;
    }

    public sealed class WebhookConfig
    {
        [JsonPropertyName("NotifyMembersRoleID")]
        public ulong NotifyMembersRoleID { get; set; } = 0;

        [JsonPropertyName("NewMapNotification")]
        public bool NewMapNotification { get; set; } = false;

        [JsonPropertyName("GameEndScoreboard")]
        public bool GameEndScoreboard { get; set; } = false;

        [JsonPropertyName("NotifyWebhookURL")]
        public string NotifyWebhookURL { get; set; } = "";

        [JsonPropertyName("RequestPlayersURL")]
        public string RequestPlayersURL { get; set; } = "";

        [JsonPropertyName("ScoreboardURL")]
        public string ScoreboardURL { get; set; } = "";

        [JsonPropertyName("StatusWebhookURL")]
        public string StatusWebhookURL { get; set; } = "";

        [JsonPropertyName("StatusMessageID")]
        public ulong StatusMessageID { get; set; } = 0;
    }
}