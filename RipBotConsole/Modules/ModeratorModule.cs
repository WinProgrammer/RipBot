using Discord.Commands;
using Discord.WebSocket;
using RipBot.Attributes;
using RipBot.Enums;
using System.Threading.Tasks;

namespace RipBot.Modules
{
	[Name("Moderator")]
	[RequireContext(ContextType.Guild)]
	public class ModeratorModule : ModuleBase<SocketCommandContext>
	{
		[Command("kick")]
		[Remarks("Kick the specified user.")]
		[Summary("EX: ripbot kick Troll\n")]
		[MinPermissions(AccessLevel.ServerMod)]
		public async Task Kick([Remainder]SocketGuildUser user)
		{
			await ReplyAsync($"cya {user.Mention} :wave:");
			await user.KickAsync();
		}
	}
}
