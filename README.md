# BotforeAndAfter

Discord Bot for various activities on Kyle Ayer's discord server (http://fart.kyleayers.com)

Create config.json in base directory with options for the bot

```json
{
  "discord_token": "DISCORDTOKEN",
  "log_file": "logs/bot.log",
  "bing_key": "BING SEARCH API KEY",
  "round_timer": 3,
  "cooldown": 0,
  "sheets_config": {
    "client_id": "GOOGLE SHEETS API CLIENT ID",
    "project_id": "GOOGLE SHEETS API PROJECT ID",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "GOOGLE SHEETS CLIENT SECRET",
    "redirect_uris": [
      "urn:ietf:wg:oauth:2.0:oob",
      "http://localhost"
    ],
    "sheet": "GOOGE SHEET ID FOR BEFORE AND AFTERS",
    "range": "USED!B4:E1444",
    "compliments_sheet": "GOOGLE SHEET ID FOR COMPLIMENTS",
    "compliments_range": "Sheet1!A1:A1000"
  }
}
```

Google Sheets API Docs: https://developers.google.com/sheets/api/quickstart/dotnet

Bing Image Search: https://github.com/microsoft/bing-search-sdk-for-net/tree/main/sdk/ImageSearch
