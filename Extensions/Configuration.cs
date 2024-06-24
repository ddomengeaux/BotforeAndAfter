using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BotforeAndAfters.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters.Extensions
{
    internal static class Configuration
    {
        public static LoggerConfiguration SetupLogging(this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration)
        {
            loggerConfiguration.WriteTo.Console();

            if (!string.IsNullOrEmpty(configuration[Keys.LOG_FILE_LOCATION_KEY]))
                loggerConfiguration.WriteTo.File(configuration[Keys.LOG_FILE_LOCATION_KEY]);

            return loggerConfiguration;
        }

        public static async Task<SheetsService> SetupGoogleSheets(IConfiguration configuration, ILogger logger)
        {
            if (string.IsNullOrEmpty(configuration[Keys.SHEETS_CLIENT_ID_KEY]))
            {
                logger.Warning("No google sheets auth config found. Before and Afters will be disabled");
                return null;
            }

            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "config", "token");
                Console.WriteLine(path);
                return new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = configuration[Keys.SHEETS_CLIENT_ID_KEY],
                            ClientSecret = configuration[Keys.SHEETS_CLIENT_SECRET_KEY]
                        },
                        new[] {SheetsService.ScopeConstants.SpreadsheetsReadonly},
                        Constants.CONFIG_BOT_NAME,
                        CancellationToken.None,
                        new FileDataStore(path, true)),
                    ApplicationName = Constants.CONFIG_BOT_NAME
                });
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Unable to authenticate with google sheets. Before and Afters will be disabled");
                return null;
            }
        }
    }
}
