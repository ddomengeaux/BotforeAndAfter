using System;
using System.Threading.Tasks;
using System.Timers;
using BotforeAndAfters.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters.Commands
{
    public class ComplimentModule : ModuleBase<SocketCommandContext>
    {
        private readonly ComplimentService _complimentService;
        private readonly ILogger _logger;

        public ComplimentModule(IServiceProvider services)
        {
            _complimentService = services.GetService<ComplimentService>();
            _logger = services.GetService<ILogger>();
        }

        [Command("compliment")]
        public async Task ComplimentAsync([Remainder] IUser user = null)
        {
            var userInfo = user ?? Context.Message.Author;
            await ReplyAsync($"{userInfo.Mention} {await _complimentService.GetComplimentAsync()}");
        }
    }
}
