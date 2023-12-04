# Discord Status
This CSSharp plugin allows monitoring server status from discord.

### Description


## Requirments
[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) **v90 >=**

## Features
- **Displaying server status on discord**
- Sorting Players by CT and T and with their respective team colors
- Sorting Players by kills on each side
- different displays on Empty and WithPlayers
- Showing Connect ip:port
- Embeded links directing to automatically lauch steam and joins server
- Configurable update intervals to release serverloads
- 
## Config
```{
  "UpdateIntervals": 30,
  "BotToken": "",
  "ChannelID": 0,
  "MessageID": 0,
  "MapImg": "https://elitehvh.000webhostapp.com//maps/{MAPNAME}.jpg",
  "Title": "\u2721 ELITEHVH \u2721",
  "NameFormat": "{NAME} :  {K} - {D}",
  "phpurl": "https://elitehvh.000webhostapp.com/connect.php",
  "EmbedColor": {
    "R": 34,
    "G": 139,
    "B": 34,
    "Random": true
  },
  "Map": "\uD83D\uDDFA\uFE0F Map",
  "Online": "\uD83C\uDF10 Online",
  "Score": "\uD83C\uDFC6 Scoreboard",
  "Players": "\uD83D\uDC65 Players",
  "ConfigVersion": 1
}
```
NAME TAG FORMAT:
{NAME} = NAMES
{K} = KILLS
{D} = DEATHS
{A} = ASSISTS

## Usage
- Install plugin like u use to
- Make a webhost server and put connect.php there if you have one(you can use free webhosting like 000webhost or u can use it mine)* not required
- Do the samething for mag img url (or use mine)
- When config is created at counterstrikesharp/configs ,edit the config with ur bot tokens etc
- RESTART SERVER
- After first init of bot messages, copy the message id to the config
- Restart again sorry

## Server Empty:
![image](https://github.com/Tian7777/DiscordStatus/assets/41808115/6d996299-26a1-4ffd-92de-ef2263c28ce0)

## Server With Players:
![image](https://github.com/Tian7777/DiscordStatus/assets/41808115/ec02086f-2cdb-4137-ad04-6190696e071e)




### Roadmap
- [x] Adding scoreboard snapshots 
- [x] Adding discord steamid bindings
- [x] Adding discord VIP roles bound to steamid in database
