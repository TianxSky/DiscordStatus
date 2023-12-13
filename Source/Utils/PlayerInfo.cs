namespace DiscordStatus
{
    public class PlayerInfo
    {
        //onconnect
        public int Index { get; set; }

        public int? UserId { get; set; }
        public string? SteamId { get; set; }
        public string? Name { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }

        //ingame
        public string? Clan { get; set; }

        public int? TeamID { get; set; }
        public int? Kills { get; set; } = 0;
        public int? Deaths { get; set; } = 0;
        public int? Assists { get; set; } = 0;
        public string? KD { get; set; } = "0";
    }
}