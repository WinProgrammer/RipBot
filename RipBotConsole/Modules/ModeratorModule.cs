using Discord.Commands;
using Discord.WebSocket;
using RipBot.Attributes;
using RipBot.Enums;
using System.Threading.Tasks;

namespace RipBot.Modules
{
	/// <summary>
	/// Handles all moderator commands.
	/// </summary>
	[Name("Moderator")]
	[RequireContext(ContextType.Guild)]
	public class ModeratorModule : ModuleBase<SocketCommandContext>
	{
		/// <summary>
		/// Kick the specified user.
		/// </summary>
		/// <param name="user">The SocketGuildUser to kick.</param>
		/// <returns></returns>
		[Command("kick")]
		[Remarks("Kick the specified user.")]
		[Summary("EX: ripbot kick Troll\n")]
		[MinPermissions(AccessLevel.ServerMod)]
		public async Task KickCmd([Remainder]SocketGuildUser user)
		{
			await ReplyAsync($"cya {user.Mention} :wave:");
			await user.KickAsync();
		}
	}
}
