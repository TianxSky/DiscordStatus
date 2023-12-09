# Discord Status
This CSSharp plugin allows monitoring server status from discord with cute embeds and nameformat support.

## Requirments
[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) **>= 101**

## Usage
- Install plugin like u use to
- Make a webhost server and put connect.php there if you have one(you can use free webhosting like 000webhost or u can use it mine)* not required
- Do the samething for map img url (or use mine or see below)
- Load the plugin
- Change the config
- RESTART SERVER (required if had old version loaded)
- You can try hotloading after changing config but it is not recommended
## Commands
- request - @notify members to join the server
## Features
- **Displaying server status on discord**
- Showing Country Flags
- Custom Name Formatting
- Sorting Players by CT and T and with their respective team colors (not working if using flags)
- Sorting Players by kills
- different displays on Idle, with players, with players not inlined, requesting players, offline.
- Showing Connect ip:port
- Embeded links directing to automatically lauch steam and joins server
- Configurable update intervals to release serverloads
- Automatically update config and rename the old config

## Config
```json
{
  "Title": "",
  "UpdateInterval": 30,
  "NotifyMembersRoleID": 0,
  "WebhookURL": "",
  "MessageID": 0,
  "PHPURL": "https://something.com/connect.php",
  "MapImg": "https://something.com//maps/{MAPNAME}.jpg",
  "OfflineImg": "", //any of these img url can be gif, jpg, png, etc.
  "IdleImg": "",
  "RequestImg":  "",
  "EmbedColor": "#00ffff", //this dont matter if RandomColor = true
  "RandomColor": true, 
  "MapField": "🗺️ Map",
  "OnlineField": "👥 Online",
  "CTField": "CT : {SCORE}", // you can have {SCORE} or dont
  "TField": "T : {SCORE}",
  "NameFormat": "{FLAG} {NAME}: KD | {KD}", //see below
  "PlayersFlagMode": true, //if want to show flags
  "PlayersInline": false, //if want to show players vertically (false)
  "Version": 3
}
```
```
NAME TAG FORMAT:
{NAME} = NAMES
{K} = KILLS
{D} = DEATHS
{A} = ASSISTS
{KD} = KD Ratio
{CLAN} = Clantag
{RC} = Region Code
{CC} = Country Code
{FLAG} = Country Flag
```
## Note
For imageurl u can also use "https://image.gametracker.com/images/maps/160x120/csgo/{MAPNAME}.jpg"

## Server Empty(Custom Img):
![image](https://github.com/Tian7777/DiscordStatus/assets/41808115/ec6b771e-8518-4bcc-8965-6c575e584f76)
## Server With Players(Custom Map Img / Custom Img):
![image](https://github.com/Tian7777/DiscordStatus/assets/41808115/236c572d-84a3-4faf-b37e-985b58388e16)
## PlayerInline false:
![image](https://github.com/Tian7777/DiscordStatus/assets/41808115/ba1d5075-094f-405c-8c44-326fa7d1f69d)

### Roadmap
- [x] Adding scoreboard snapshots 
