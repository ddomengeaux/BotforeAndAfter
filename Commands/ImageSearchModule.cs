using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Bing.ImageSearch;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BotforeAndAfters.Commands
{
    public class ImageSearchModule : ModuleBase<SocketCommandContext>
    {
        private readonly ImageSearchClient _client;
        private readonly ILogger _logger;

        public ImageSearchModule(IServiceProvider services)
        {
            _client = services.GetService<ImageSearchClient>();
            _logger = services.GetService<ILogger>();
        }

        [Command("mank")]
        [Summary("I'm toiling with you in spirit.")]
        public async Task MankAsync()
        {
            var searchString = "mank";
            var title = "";

            var a = new Random().Next(0, 69);

            if (a % 2 == 0)
            {
                searchString = "Matt Rogers Las Culturistas";
                title = "The Mank boner was throbbing";
            }

            var resp = await _client.Images.SearchAsync(query: searchString);
            await ReplyAsync(title, false, embed: new EmbedBuilder()
            {
                ImageUrl = resp.Value[new Random().Next(0, a % 2 == 0 ? 10 : resp.Value.Count())].ContentUrl
            }.Build());
        }

        [Command("hank")]
        [Summary("that boy ain't right")]
        public async Task HankAsync()
        {
            var resp = await _client.Images.SearchAsync(query: "king of the hill quotes hank");
            await ReplyAsync("", false, embed: new EmbedBuilder()
            {
                ImageUrl = resp.Value[new Random().Next(0, resp.Value.Count())].ContentUrl
            }.Build());
        }
    }
}
