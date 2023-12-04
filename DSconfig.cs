    namespace DiscordStatus
{
    using System.Text.Json.Serialization;
    using CounterStrikeSharp.API.Core;

    public class DSconfig : BasePluginConfig
    {
        [JsonPropertyName("UpdateIntervals")]
        public int UpdateIntervals { get; set; } = 60;

        [JsonPropertyName("BotToken")]
        public string BotToken { get; set; } = "putyourtoken";

        [JsonPropertyName("ChannelID")]
        public ulong ChannelID { get; set; } = 0;

        [JsonPropertyName("MessageID")]
        public ulong MessageID { get; set; } = 0;

        [JsonPropertyName("MapImg")]
        public string MapImg { get; set; } = "https://elitehvh.000webhostapp.com//maps/{MAPNAME}.jpg";

        [JsonPropertyName("Title")]
        public string Title { get; set; } = "✡ ELITEHVH ✡";

        [JsonPropertyName("NameFormat")]
        public string NameFormat { get; set; } = "{CLAN}{NAME}: KD | {KD}";

        [JsonPropertyName("phpurl")]
        public string phpurl { get; set; } = "elitehvh.000webhostapp.com/connect.php";

        [JsonPropertyName("EmbedColor")]
        public EmbedColorConfig EmbedColor { get; set; } = new EmbedColorConfig
        {
            R = 34,
            G = 139,
            B = 34,
            Random = false
        };

        [JsonPropertyName("Map")]
        public string Map { get; set; } = "🗺️ Map";

        [JsonPropertyName("Online")]
        public string Online { get; set; } = "🌐 Online";

        [JsonPropertyName("Score")]
        public string Score { get; set; } = "🏆 Scoreboard";

        [JsonPropertyName("Players")]
        public string Players { get; set; } = "👥 Players";

        [JsonPropertyName("PlayersInline")]
        public bool PlayersInline { get; set; } = true;
        
        public class EmbedColorConfig
        {
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
            public bool Random { get; set; }
        }

    }
}
