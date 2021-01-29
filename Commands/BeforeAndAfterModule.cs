using System.Threading.Tasks;
using Discord.Commands;

namespace BotforeAndAfters.Commands
{
    public class BeforeAndAfterModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
            => ReplyAsync(echo);
    }
}
