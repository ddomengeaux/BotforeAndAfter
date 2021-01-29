using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BotforeAndAfters.Commands
{
    [Group("role")]
    public class RolesModule : ModuleBase<SocketCommandContext>
    {
        [Command("create")]
        [Summary("Add a new role to the server.")]
        public async Task Create(string roleName)
        {
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals(roleName)) == null)
            {
                var role = await Context.Guild.CreateRoleAsync(roleName, GuildPermissions.None, null, false, false);
                if (role != null)
                    await Context.Channel.SendMessageAsync($"Role {roleName} created");
                return;
            }

            await Context.Channel.SendMessageAsync($"Role {roleName} already exists");
        }
        
        [Command("delete")]
        [Summary("Delete a new role to the server.")]
        public async Task Delete(string roleName)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals(roleName));

            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"Role {roleName} does not exist");
                return;
            }

            await role.DeleteAsync();
            await Context.Channel.SendMessageAsync($"Role {roleName} deleted");
        }

        [Command("list")]
        [Summary("Lists available roles")]
        public async Task List()
        {
            var eb = new EmbedBuilder();
            eb.WithDescription("Roles");

            foreach (var role in Context.Guild.Roles)
            {
                eb.AddField(role.Name, "a", false);
            }
            
            
            await Context.Channel.SendMessageAsync(string.Empty, false, eb.Build());
        }
    }
}
