namespace DiscordStatus
{
    public class Globals
    {
        public GeneralConfig GConfig { get; set; } = new();
        public WebhookConfig WConfig { get; set; } = new();
        public EmbedConfig EConfig { get; set; } = new();
        public DSconfig Config { get; set; } = new();
        public string? ServerIP { get; set; }
        public string? ConnectURL { get; set; }
        public string? NameFormat { get; set; }
        public int MaxPlayers { get; set; }
        public int TScore { get; set; }
        public int CTScore { get; set; }
        public string? MapName { get; set; }
        public ulong MessageID { get; set; }
        public bool HasCC { get; set; } = false;
        public bool HasRC { get; set; } = false;

        public List<string> TPlayersName { get; set; } = new List<string>();
        public List<string> CtPlayersName { get; set; } = new List<string>();
        public Dictionary<int, PlayerInfo> PlayerList { get; set; } = new();
    }
}