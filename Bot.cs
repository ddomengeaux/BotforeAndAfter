using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using BotforeAndAfters.Extensions;
using BotforeAndAfters.Services;
using Dice;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Flurl.Http;
using LiteDB;
using Microsoft.Bing.ImageSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace BotforeAndAfters
{
    internal class Bot
    {
        private readonly IConfiguration _config;
        private bool _isLoaded = false;

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
        public ILogger Logger { get; private set; }
        public TextAnalyticsClient AnalyticsClient { get; private set; }
        public CommandService CommandService => Services.GetRequiredService<CommandService>();

        public async Task StartAsync()
        {
            Logger = new LoggerConfiguration()
                .SetupLogging(_config)
                .CreateLogger();

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Verbose
            });

            AnalyticsClient = new TextAnalyticsClient(new Uri(_config[Keys.COGNITIVE_SERVICES_URI_KEY]),
                        new AzureKeyCredential(_config[Keys.COGNITIVE_SERVICES_KEY_KEY]));

            Services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(Logger)
                .AddSingleton(Client)
                .AddSingleton(AnalyticsClient)
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
                .AddSingleton(new LiteDatabase($"{Constants.CONFIG_BOT_NAME}.db"))
                .AddSingleton<GameService>()
                .AddSingleton<ComplimentService>()
                .AddSingleton<ExcuseService>()
                .AddSingleton(new ImageSearchClient(new ApiKeyServiceClientCredentials(_config[Keys.BING_KEY])))
                .BuildServiceProvider();

            Client.Log += OnLog;
            CommandService.Log += OnLog;
            Client.Ready += async () =>
            {
                Logger.Information("Connected as {Username} on {Count} server(s)", Client.CurrentUser.Username,
                    Client.Guilds.Count);

                if (_isLoaded)
                    return;

                await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

                Client.MessageReceived += async message =>
                {
                    if (message is not SocketUserMessage msg || !(message.Channel is SocketGuildChannel))
                        return;

                    var argPos = 0;
                    if (message.Author.IsBot)
                        return;

                    var context = new SocketCommandContext(Client, msg);

                    if (msg.HasCharPrefix(Constants.CONFIG_DEFAULT_COMMAND_PREFIX, ref argPos))
                    {
                        var watch = new Stopwatch();
                        watch.Start();

                        var result = await CommandService.ExecuteAsync(context, argPos,
                                Services);

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
                                Logger.Error(execute.Exception,
                                    "{Content} executed by {Username} failed ({ErrorReason}) in {ElapsedMilliseconds} ms",
                                    msg.Content,
                                    msg.Author.Username,
                                    result.ErrorReason,
                                    watch.ElapsedMilliseconds);
                                break;

                            default:
                                Logger.Error(
                                    "{Content} executed by {Username} failed ({ErrorReason}) in {ElapsedMilliseconds} ms",
                                    msg.Content,
                                    msg.Author.Username,
                                    result.ErrorReason,
                                    watch.ElapsedMilliseconds);
                                break;
                        }

                        if (!result.ErrorReason.Equals("Unknown command."))
                            await context.Channel.SendMessageAsync(result.ErrorReason);
                    }
                    else if (msg.HasMentionPrefix(Client.CurrentUser, ref argPos))
                    {
                        DocumentSentiment documentSentiment = AnalyticsClient.AnalyzeSentiment(msg.Content);

                        switch (documentSentiment.Sentiment)
                        {
                            case TextSentiment.Neutral:
                            default:
                                Logger.Information("Neutral sentiment from {0}", context.Message.Author.Username);
                                break;

                            case TextSentiment.Positive:
                                await context.Channel.SendMessageAsync($"Awwww thank you {context.Message.Author.Username}");
                                break;

                            case TextSentiment.Negative:
                                await context.Channel.SendMessageAsync($"That's not very nice {context.Message.Author.Username}");
                                break;

                            case TextSentiment.Mixed:
                                await context.Channel.SendMessageAsync($"I'm not sure how I feel about that ...");
                                break;
                        }
                    }
                };

                _isLoaded = true;
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
