namespace BotforeAndAfters
{
    internal static class Constants
    {
        public const string CONFIG_BOT_NAME = "BotforeAndAfter";
        public const string CONFIG_FILENAME = "config.json";
        public const char CONFIG_DEFAULT_COMMAND_PREFIX = '!';
    }

    internal static class Keys
    {
        public const string DISCORD_TOKEN_KEY = "discord_token";
        public const string LOG_FILE_LOCATION_KEY = "log_file";
        public const string BING_KEY = "bing_key";

        public const string COGNITIVE_SERVICES_URI_KEY = "cognitiveservices_uri";
        public const string COGNITIVE_SERVICES_KEY_KEY = "cognitiveservices_key";
        public const string SENTIMENT_ENDPOINT_KEY = "sentiment_endpoint";

        public const string SHEETS_CLIENT_ID_KEY = "sheets_config:client_id";
        public const string SHEETS_CLIENT_SECRET_KEY = "sheets_config:client_secret";
        public const string SHEETS_SHEET_ID = "sheets_config:sheet";
        public const string SHEETS_SHEET_RANGE = "sheets_config:range";

        public const string ROUND_TIMER = "round_timer";
        public const string COOLDOWN_TIMER = "cooldown";
    }
}
