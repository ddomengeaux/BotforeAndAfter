using System;
using System.Linq;
using System.Threading.Tasks;
using Dice;
using Discord;
using Discord.Commands;

namespace BotforeAndAfters.Commands
{
    public class DiceRollerModule : ModuleBase<SocketCommandContext>
    {
        [Command("roll")]
        [Summary("roll x number of y sided dice")]
        public async Task Add(string expression)
        {
            if (expression.ToLower().Contains("help"))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                                .WithTitle($"Roller Help")
                                .WithDescription("I understand a lot of different expressions. Check out https://skizzerz.net/DiceRoller/Dice_Reference for details.")
                                    .Build());

                return;
            }

            var result = Roller.Roll(expression);

            await ReplyAsync(embed: new EmbedBuilder()
                .WithTitle($"Rolling {result.Expression}!")
                .AddField("Result", result.ToString(), false)
                .Build());
        }
    }
}
