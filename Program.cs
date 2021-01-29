using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters
{
    internal static class Constants
    {
        public const string CONFIG_FILENAME = "config.json";
        public const char CONFIG_DEFAULT_COMMAND_PREFIX = '!';
    }

    internal static class Keys
    {
        public const string DISCORD_TOKEN_KEY = "discord_token";
        public const string LOG_FILE_LOCATION = "log_file";
    }

    internal class Program
    {
        private static Task Main(string[] args)
            => new Bot(args).StartAsync();
    }

    internal static class Helpers
    {
        public static LoggerConfiguration SetupLogging(this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration)
        {
            loggerConfiguration.WriteTo.Console();

            if (!string.IsNullOrEmpty(configuration[Keys.LOG_FILE_LOCATION]))
                loggerConfiguration.WriteTo.File(configuration[Keys.LOG_FILE_LOCATION]);

            return loggerConfiguration;
        }
    }

    internal class Bot
    {
        private readonly IConfiguration _config;

        public Bot(string[] args)
        {
            try
            {
                _config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(Constants.CONFIG_FILENAME, true)
                    .AddCommandLine(args).Build();

                if (string.IsNullOrEmpty(_config[Keys.DISCORD_TOKEN_KEY]))
                    throw new InvalidDataException("No discord token found!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public DiscordSocketClient Client { get; private set; }
        public IServiceProvider Services { get; private set; }
        public ILogger Logger => Services.GetRequiredService<ILogger>();
        public CommandService CommandService => Services.GetRequiredService<CommandService>();

        public async Task StartAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Verbose
            });
            
            Services = new ServiceCollection()
                .AddSingleton<ILogger>(new LoggerConfiguration()
                    .SetupLogging(_config)
                    .CreateLogger())
                .AddSingleton(Client)
                .AddSingleton(new InteractiveService(Client, new InteractiveServiceConfig
                {
                    DefaultTimeout = TimeSpan.FromSeconds(30)
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    IgnoreExtraArgs = false,
                    LogLevel = LogSeverity.Verbose
                }))
                .BuildServiceProvider();

            Client.Log += OnLog;
            CommandService.Log += OnLog;
            Client.Ready += async () =>
            {
                Logger.Information("Connected as {Username} on {Count} server(s)", Client.CurrentUser.Username,
                    Client.Guilds.Count);

                await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

                Client.MessageReceived += async message =>
                {
                    if (!(message is SocketUserMessage msg) || !(message.Channel is SocketGuildChannel))
                        return;

                    var argPos = 0;
                    if (!(msg.HasCharPrefix(Constants.CONFIG_DEFAULT_COMMAND_PREFIX, ref argPos) ||
                          msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) ||
                        message.Author.IsBot)
                        return;

                    var watch = new Stopwatch();
                    watch.Start();

                    var context = new SocketCommandContext(Client, msg);
                    var result = await CommandService.ExecuteAsync(context: context, argPos: argPos,
                        services: Services);

                    watch.Stop();

                    if (result.IsSuccess)
                    {
                        Logger.Information("{Content} successfully executed by {Username} in {ElapsedMilliseconds} ms", 
                            msg.Content,
                            msg.Author.Username,
                            watch.ElapsedMilliseconds);
                        
                        return;
                    }

                    switch (result)
                    {
                        case ExecuteResult execute:
                            Logger.Error(execute.Exception, "{Content} executed by {Username} failed ({ErrorReason}) in {ElapsedMilliseconds} ms", 
                                msg.Content,
                                msg.Author.Username,
                                result.ErrorReason,
                                watch.ElapsedMilliseconds);
                            break;
                        
                        default:
                            Logger.Error("{Content} executed by {Username} failed ({ErrorReason}) in {ElapsedMilliseconds} ms", 
                                msg.Content,
                                msg.Author.Username,
                                result.ErrorReason,
                                watch.ElapsedMilliseconds);
                            break;
                    }
                    
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                };
            };

            await Client.LoginAsync(TokenType.Bot, _config[Keys.DISCORD_TOKEN_KEY]);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private Task OnLog(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Logger.Fatal(message.Exception, "{Message}", message.Message);
                    break;
                case LogSeverity.Error:
                    Logger.Error(message.Exception, "{Message}", message.Message);
                    break;
                case LogSeverity.Warning:
                    Logger.Warning("{Message}", message.Message);
                    break;
                case LogSeverity.Info:
                    Logger.Information("{Message}", message.Message);
                    break;
                case LogSeverity.Verbose:
                    Logger.Verbose("{Message}", message.Message);
                    break;
                case LogSeverity.Debug:
                    Logger.Debug("{Message}", message.Message);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(message.Severity), "Invalid Severity");
            }

            return Task.CompletedTask;
        }
    }
}
