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
    public class ExcuseModule : ModuleBase<SocketCommandContext>
    {
        private readonly ExcuseService _excuseService;
        private readonly ILogger _logger;

        public ExcuseModule(IServiceProvider services)
        {
            _excuseService = services.GetService<ExcuseService>();
            _logger = services.GetService<ILogger>();
        }

        [Command("excuse")]
        public async Task ExcuseAsync()
        {
            await ReplyAsync($"{await _excuseService.GetExcuseAsync()}");
        }
    }
}
