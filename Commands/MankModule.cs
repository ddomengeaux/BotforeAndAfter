using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Bing.ImageSearch;
using Microsoft.Extensions.DependencyInjection;

namespace BotforeAndAfters.Commands
{
    public class MankModule : ModuleBase<SocketCommandContext>
    {
        private readonly ImageSearchClient _client;

        public MankModule(IServiceProvider services)
        {
            _client = services.GetService<ImageSearchClient>();
        }

        [Command("mank")]
        [Summary("I'm toiling with you in spirit.")]
        public async Task MankAsync()
        {
            var resp = await _client.Images.SearchAsync(query: "mank");
            await ReplyAsync("", false, embed: new EmbedBuilder()
            {
                ImageUrl = resp.Value[new Random().Next(0, resp.Value.Count())].ContentUrl
            }.Build());
        }
    }
}
