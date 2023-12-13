using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace DiscordStatus;

internal static class DSLog
{
    internal static void Log(int purpose, string message)
    {
        switch (purpose)
        {
            case 0: LogToConsole(ConsoleColor.Magenta, "[Discord Status] -> " + message); break;
            case 1: LogToConsole(ConsoleColor.Green, "[Discord Status] -> " + message); break;
            case 2: LogToConsole(ConsoleColor.Red, "[Discord Status] -> " + message); break;
        }
    }

    internal static void LogToConsole(ConsoleColor color, string messageToLog)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(messageToLog);
        Console.ResetColor();
    }

    internal static void LogToChat(CCSPlayerController? player, string messageToLog)
    {
        messageToLog = ReplaceTags($"{ChatColors.Red}[Discord Status]: " + messageToLog);
        Server.NextFrame(() => player?.PrintToChat(messageToLog));
    }

    internal static void LogToChatAll(string messageToLog)
    {
        messageToLog = ReplaceTags($"{ChatColors.Red}[Discord Status]: " + messageToLog);
        Server.NextFrame(() => Server.PrintToChatAll(messageToLog));
    }

    internal static string ReplaceTags(this string text)
    {
        text = text.Replace("{DEFAULT}", $"{ChatColors.Default}");
        text = text.Replace("{WHITE}", $"{ChatColors.White}");
        text = text.Replace("{DARKRED}", $"{ChatColors.Darkred}");
        text = text.Replace("{GREEN}", $"{ChatColors.Green}");
        text = text.Replace("{LIGHTYELLOW}", $"{ChatColors.LightYellow}");
        text = text.Replace("{LIGHTBLUE}", $"{ChatColors.LightBlue}");
        text = text.Replace("{OLIVE}", $"{ChatColors.Olive}");
        text = text.Replace("{LIME}", $"{ChatColors.Lime}");
        text = text.Replace("{RED}", $"{ChatColors.Red}");
        text = text.Replace("{PURPLE}", $"{ChatColors.Purple}");
        text = text.Replace("{GREY}", $"{ChatColors.Grey}");
        text = text.Replace("{YELLOW}", $"{ChatColors.Yellow}");
        text = text.Replace("{GOLD}", $"{ChatColors.Gold}");
        text = text.Replace("{SILVER}", $"{ChatColors.Silver}");
        text = text.Replace("{BLUE}", $"{ChatColors.Blue}");
        text = text.Replace("{DARKBLUE}", $"{ChatColors.DarkBlue}");
        text = text.Replace("{BLUEGREY}", $"{ChatColors.BlueGrey}");
        text = text.Replace("{MAGENTA}", $"{ChatColors.Magenta}");
        text = text.Replace("{LIGHTRED}", $"{ChatColors.LightRed}");
        return text;
    }
}