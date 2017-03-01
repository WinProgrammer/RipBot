using Discord.Commands;
using Discord.WebSocket;
using RipBot.Attributes;
using RipBot.Enums;
using System.Threading.Tasks;
using Discord;
using System.Linq;

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


		/// <summary>
		/// Deletes messages in the channnel the command is issued from.
		/// </summary>
		/// <param name="numberofmessagestodelete">The number of messages to delete in the channel.</param>
		/// <returns></returns>
		[Command("purgechannel"), Alias("pc")]
		[Remarks("Deletes x messages in the channnel the command is issued from.")]
		[Summary("EX: ripbot purgechannel 10\n")]
		[MinPermissions(AccessLevel.ServerMod)]
		public async Task PurgeMessagesCmd(int numberofmessagestodelete = 100)
		{
			// make sure it isn't over 100
			if (numberofmessagestodelete > 100)
			{
				numberofmessagestodelete = 100;
				//await ReplyAsync("The number of messages to delete can not be over 100.");
				//return;
			}

			// delete n messages
			var messagesToDelete = await Context.Channel.GetMessagesAsync(numberofmessagestodelete, CacheMode.AllowDownload).Flatten();
			await Context.Channel.DeleteMessagesAsync(messagesToDelete);

			// notify channel
			var tempMsg = await ReplyAsync($"Deleted **{messagesToDelete.Count()}** message(s)");
			// wait 5 seconds
			await Task.Delay(5000);
			// now remove our notification
			await tempMsg.DeleteAsync();
		}
	}
}
