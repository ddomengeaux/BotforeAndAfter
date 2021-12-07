//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Timers;
//using BotforeAndAfters.Services;
//using Discord;
//using Discord.Commands;
//using Microsoft.Extensions.DependencyInjection;
//using Serilog;

//namespace BotforeAndAfters.Commands
//{
//    [Group("stats")]
//    public class GameStatsModule : ModuleBase<SocketCommandContext>
//    {
//        private readonly GameService _gameService;
//        private readonly ILogger _logger;

//        public GameStatsModule(IServiceProvider services)
//        {
//            _gameService = services.GetService<GameService>();
//            _logger = services.GetService<ILogger>();
//        }

//        [Command("closeuppicgame")]
//        [Summary("Shows some basic stats for the close up pic game")]
//        public async Task CloseUpPicGame()
//        {
//            try
//            {
//                var messages = await Context.Channel.GetMessagesAsync(1000).FlattenAsync();
//                var items = messages.SelectMany(x => x.Reactions.Where(x => x.Key.Name.Equals("NeverSeenItStatue")));

//                await ReplyAsync(embed: new EmbedBuilder()
//                    .WithTitle($"Stats")
//                    .AddField("Games Won", items.Count())
//                    .Build());
//            }
//            catch (Exception e)
//            {
//                await LogError(e);
//            }
//        }

//        [Command("beforeandafters")]
//        [Summary("Shows some basic stats for the before and after game")]
//        public async Task StatsAsync()
//        {
//            try
//            {
//                var stats = await _gameService.GetStats();

//                await ReplyAsync(embed: new EmbedBuilder()
//                    .WithTitle($"Stats")
//                    .AddField("Entries", $"{stats.Total}", true)
//                    .AddField("Played", $"{stats.Played}", true)
//                    .AddField("Won", $"{stats.Won}", true)
//                    .Build());
//            }
//            catch (Exception e)
//            {
//                await LogError(e);
//            }
//        }

//        private async Task LogError(Exception e)
//        {
//            _logger.Error(e.Message, e);
//            await ReplyAsync("Sorry I encountered an error. Tech support has been called!");
//        }
//    }
//}
