using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using BotforeAndAfters.Models;
using BotforeAndAfters.Services;
using Discord;
using Discord.Commands;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters.Commands
{
    public class BeforeAndAfterModule : ModuleBase<SocketCommandContext>
    {
        private readonly BeforeAndAftersService _beforeAndAfters;
        private readonly ILogger _logger;
        private Dictionary<ulong, BeforeAndAfterGame> _currentGames = new Dictionary<ulong, BeforeAndAfterGame>();

        private readonly LiteDatabase _database;
        private ILiteCollection<BeforeAndAfterGame> _games;
        private ILiteCollection<BeforeAndAfterGame> Games => _games ??= _database.GetCollection<BeforeAndAfterGame>();

        public BeforeAndAfterModule(IServiceProvider services)
        {
            _logger = services.GetService<ILogger>();
            _database = services.GetService<LiteDatabase>();
            _beforeAndAfters = services.GetService<BeforeAndAftersService>();
        }

        [Command("rules")]
        [Summary("Before and Afters: The Bot: The Game: The Rules!")]
        public async Task RulesAsync()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                        .WithTitle($"Hi {Context.Message.Author.Username}!")
                        .WithDescription("Use !play to start a new round. Use !guess \"bad movie pun here\" (without the \"\") to make a guess! Punctuation is ignored in the answers, but doesn't account for errors from my creators. Good Luck!")
                        .Build());
        }

        [Command("play")]
        [Summary("Play a round of Before and Afters!")]
        public async Task PlayAsync()
        {
            try
            {
                var message = await ReplyAsync(embed: new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = "Okay",
                    Description = "Fetching Game ..."
                }.Build());

                var current = new BeforeAndAfterGame(message.Id, Context.Message.Author.Id, await _beforeAndAfters.GetBeforeAndAfterAsync(), 3);
                _currentGames.Add(Context.Guild.Id, current);

                var timer = new Timer(10000) { AutoReset = true };
                timer.Elapsed += async (sender, args) =>
                {

                };
                timer.Start();

                _logger.Information(
                        $"{Context.Message.Author.Username} [{Context.Guild.Name}] started new game {current.Question.Answer}");

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
                var currentGame = _currentGames[Context.Guild.Id];

                if (currentGame == null)
                    return;

                if (currentGame.CheckAnswer(Context.Message.Author.Id, guess))
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithTitle($"Congrats {Context.Message.Author.Username}!")
                        .WithDescription("I'm so proud of you")
                        .AddField("Movies", $"{currentGame.Question.Movies}", true)
                        .AddField("Episode", $"{currentGame.Question.Episode}", true)
                        .AddField("Guesses", $"{currentGame.Guesses}", true)
                        .AddField("Time", $"{currentGame.GuessedIn:mm\\:ss}", true)
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
                    $"Updated Before and Afters data. {await _beforeAndAfters.UpdateDataSourceAsync()} total records.");
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
            var currentGame = _currentGames[Context.Guild.Id];

            if (currentGame == null)
                return;

            var message = await Context.Channel.GetMessageAsync(currentGame.Id) as IUserMessage;

            if (message == null)
            {
                await LogError(new Exception("message was null?"));
                return;
            }

            var timesPlayed = Games.Query().Where(x => x.Question.Answer == currentGame.Question.Answer).Count();
            var timesWon = Games.Query().Where(x => x.Question.Answer == currentGame.Question.Answer && x.WonBy > 0).Count();

            var banner = new EmbedBuilder()
            {
                Title = "Okay",
                Color = Color.Blue,
                Description = currentGame.Question.Plot
            }
                    .AddField("Times Played", timesPlayed, true)
                    .AddField("Times Won", timesWon, true)
                    .AddField("Guesses", currentGame.Guesses, true)
                    .AddField("Timer",
                        $"{(currentGame.IsActive ? currentGame.TimeRemaining.ToString("mm\\:ss") : "Finished")}", true);

            if (currentGame.IsActive)
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
