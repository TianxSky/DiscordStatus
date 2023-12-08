namespace DiscordStatus
{
    using System.Text.Json.Serialization;
    using CounterStrikeSharp.API.Core;

    public class DSconfig : BasePluginConfig
    {

        [JsonPropertyName("Title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("UpdateInterval")]
        public int UpdateInterval { get; set; } = 30;

        [JsonPropertyName("NotifyMembersRoleID")]
        public ulong NotifyMembersRoleID { get; set; } = 0;

        [JsonPropertyName("WebhookURL")]
        public string WebhookURL { get; set; } = "puturwebhookurlhere";

        [JsonPropertyName("MessageID")]
        public ulong MessageID { get; set; } = 0;

        [JsonPropertyName("PHPURL")]
        public string PHPURL { get; set; } = "";

        [JsonPropertyName("MapImg")]
        public string MapImg { get; set; } = "{MAPNAME}.jpg";

        [JsonPropertyName("OfflineImg")]
        public string OfflineImg { get; set; } = "not required";

        [JsonPropertyName("IdleImg")]
        public string IdleImg { get; set; } = "not required";

        [JsonPropertyName("RequestImg")]
        public string RequestImg { get; set; } = "not required";

        [JsonPropertyName("EmbedColor")]
        public string EmbedColor { get; set; } = "#00ffff";

        [JsonPropertyName("RandomColor")]
        public bool RandomColor { get; set; } = true;

        [JsonPropertyName("MapField")]
        public string MapField { get; set; } = "🗺️ Map";

        [JsonPropertyName("OnlineField")]
        public string OnlineField { get; set; } = "👥 Online";

        [JsonPropertyName("CTField")]
        public string CTField { get; set; } = "CT : {SCORE}";

        [JsonPropertyName("TField")]
        public string TField { get; set; } = "T : {SCORE}";

        [JsonPropertyName("NameFormat")]
        public string NameFormat { get; set; } = "{CCFlag} {NAME}: KD | {KD}";

        [JsonPropertyName("PlayersFlagMode")]
        public bool PlayersFlagMode { get; set; } = true;

        [JsonPropertyName("PlayersInline")]
        public bool PlayersInline { get; set; } = true;

        [JsonPropertyName("Version")]
        public override int Version { get; set; } = 3;
    }
}
