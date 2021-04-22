using System;
using System.Threading.Tasks;
using System.Timers;
using BotforeAndAfters.Services;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters.Commands
{
    public class BeforeAndAfterModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameService _gameService;
        private readonly ILogger _logger;

        public BeforeAndAfterModule(IServiceProvider services)
        {
            _gameService = services.GetService<GameService>();
            _logger = services.GetService<ILogger>();
        }

        [Command("play")]
        [Summary("Play a round of Before and Afters!")]
        public async Task PlayAsync()
        {
            try
            {
                var (hasCooldown, cooldownRemaining) = _gameService.CheckForCooldown(Context.Message.Author.Id);
                if (hasCooldown)
                {
                    await ReplyAsync($"Sorry {Context.Message.Author.Username} you are on cooldown for {cooldownRemaining:mm\\:ss}");
                    return;
                }

                var message = await ReplyAsync(embed: new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = "Okay",
                    Description = "Fetching Game ..."
                }.Build());

                await _gameService.StartRoundAsync(message.Id, Context.Message.Author.Id);
                var timer = new Timer(10000) { AutoReset = true };
                timer.Elapsed += async (sender, args) =>
                {
                    if (!_gameService.IsActive)
                    {
                        if (!_gameService.WasWon)
                        {
                            if (_gameService.TimesPlayed > 3 && _gameService.TimesPlayed > _gameService.TimesWon && _gameService.Guesses >= 3)
                                await ReplyAsync($"No winners this round. Seems like this is a tough one! ||{_gameService.Answer}||");
                            else
                                await ReplyAsync("No winners this round. Let's play again soon!");
                        }

                        timer.Dispose();
                    }

                    await GenerateBannerAsync();
                };
                timer.Start();

                _logger.Information(
                        $"{Context.Message.Author.Username} started new game {_gameService.Answer}");

                await GenerateBannerAsync();
            }
            catch (Exception e)
            {
                await LogError(e);
            }
        }

        [Command("guess")]
        [Summary("Make a guess during the current round of Before and After")]
        public async Task GuessAsync([Remainder] string guess)
        {
            try
            {
                if (await _gameService.CheckAnswerAsync(Context.Message.Author.Id, guess))
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithTitle($"Congrats {Context.Message.Author.Username}!")
                        .WithDescription("I'm so proud of you")
                        .AddField("Movies", $"{_gameService.Movies}", true)
                        .AddField("Episode", $"{_gameService.Episode}", true)
                        .AddField("Guesses", $"{_gameService.Guesses}", true)
                        .AddField("Time", $"{_gameService.GuessedIn:mm\\:ss}", true)
                        .Build());
                else
                    await Context.Message.AddReactionAsync(new Emoji("❌"));

                await GenerateBannerAsync();
            }
            catch (Exception e)
            {
                await LogError(e);
            }
        }

        [Command("update")]
        [Summary("Update the Before and After database from the parent google sheet")]
        public async Task UpdateAsync()
        {
            try
            {
                if (!Context.User.Id.Equals(152118986996187136) && !Context.User.Id.Equals(224641971233226752))
                    return;

                await ReplyAsync(
                    $"Updated Before and Afters data. {await _gameService.UpdateDataSourceAsync()} total records.");
            }
            catch (Exception e)
            {
                await LogError(e);
            }
        }

        [Command("stats")]
        [Summary("Shows some basic stats for the before and after game")]
        public async Task StatsAsync()
        {
            try
            {
                var stats = await _gameService.GetStats();

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle($"Stats")
                    .AddField("Entries", $"{stats.Total}", true)
                    .AddField("Played", $"{stats.Played}", true)
                    .AddField("Won", $"{stats.Won}", true)
                    .Build());
            }
            catch (Exception e)
            {
                await LogError(e);
            }
        }

        private async Task LogError(Exception e)
        {
            _logger.Error(e.Message, e);
            await ReplyAsync("Sorry I encountered an error. Tech support has been called!");
        }

        private async Task GenerateBannerAsync()
        {
            if (!_gameService.Id.HasValue)
                return;

            var message = await Context.Channel.GetMessageAsync(_gameService.Id.Value) as IUserMessage;

            var banner = new EmbedBuilder()
            {
                Title = "Okay",
                Color = Color.Blue,
                Description = _gameService.Question
            }
                .AddField("Times Played", _gameService.TimesPlayed, true)
                .AddField("Times Won", _gameService.TimesWon, true)
                .AddField("Guesses", _gameService.Guesses, true)
                .AddField("Timer",
                    $"{(_gameService.IsActive ? _gameService.TimeRemaining.ToString("mm\\:ss") : "Finished")}", true);

            if (_gameService.IsActive)
            {
                banner.Footer = new EmbedFooterBuilder()
                {
                    Text = "Use !guess to play"
                };
            }
            else
            {
                banner.Footer = new EmbedFooterBuilder()
                {
                    Text = "Round Over"
                };
            }

            await message.ModifyAsync((m) => m.Embed = banner.Build());
        }
    }
}
