using Discord;
using Discord.Commands;
using RipBot.Types;
using System.Linq;
using System.Threading.Tasks;

namespace RipBot.Modules
{
	/// <summary>
	/// Handles the help commands.
	/// </summary>
	[Name("Help")]
	//[RequireContext(ContextType.Guild)]
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		private CommandService _service;

		/// <summary>
		/// CStor.
		/// </summary>
		/// <param name="service"></param>
		public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
		{
			_service = service;
		}


		/// <summary>
		/// Display available commands and thier usage.
		/// </summary>
		/// <returns></returns>
		[Command("help")]
		[Remarks("Display available commands and thier usage.\n")]
		[Summary("EX: ripbot help\n")]
		public async Task HelpAsync()
		{
			string prefix = BotConfiguration.Load().Prefix;
			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = "These are the commands you can use:\n(THIS IS STILL BEING WORKED ON AND IS NOT FINISHED)\n\nDefault guild is: __*" + WoWConfiguration.Load().DefaultGuildName + "*__\nDefault realm is: __*" + WoWConfiguration.Load().DefaultRealm + "*__\n\n",
			};
			
			foreach (var module in _service.Modules)
			{
				string currentline = null;
				foreach (var cmd in module.Commands)
				{
					var result = await cmd.CheckPreconditionsAsync(Context);
					if (result.IsSuccess)
						//currentline += $"{cmd.Aliases.First()}\n" + $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\t" + $"Remarks: {cmd.Remarks}\t" + $"Usage: {cmd.Summary}\n";
						currentline += $"__*{cmd.Aliases.First()}*__\n" + cmd.Remarks + "\n" + $"{cmd.Summary}\n\n";
						//currentline += $"__*{cmd.Aliases.First()}*__\n" + cmd.Remarks + "\n" + $"EX: {cmd.Summary}\n\n";
					//description += $"{prefix}{cmd.Aliases.First()}\n";
				}

				if (!string.IsNullOrWhiteSpace(currentline))
				{
					builder.AddField(x =>
					{
						x.Name = "__**" + module.Name + "**__";
						x.Value = currentline;
						//x.Value = currentline + $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\t" + $"Remarks: {cmd.Remarks}\t" + $"Usage: {cmd.Summary}\n";
						x.IsInline = false;
					});
				}
			}

			await ReplyAsync("", false, builder.Build());
		}



		//[Command("help")]
		//public async Task HelpAsync(string command)
		//{
		//	//var result = _service.Search(Context, command);

		//	var result = _service.Commands;


		//	//if (!result.IsSuccess)
		//	//{
		//	//	await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
		//	//	return;
		//	//}

		//	string prefix = Configuration.Load().Prefix;
		//	var builder = new EmbedBuilder()
		//	{
		//		Color = new Color(114, 137, 218),
		//		//Description = $"Here are some commands like **{command}**"
		//	};

		//	foreach (var match in result)
		//	{
		//		//var cmd = match.Command;

		//		builder.AddField(x =>
		//		{
		//			x.Name = string.Join(", ", match.Aliases);
		//			//x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + $"Remarks: {cmd.Remarks}";
		//			//x.Value = $"Parameters: {string.Join(", ", match.Parameters.Select(p => p.Name))}\t" + $"Remarks: {match.Remarks}\t" + $"Usage: {match.Summary}\n";
		//			x.Value = $"Desc: {match.Remarks}\t" + $"Ex: {match.Summary}\n";
		//			x.IsInline = false;
		//		});
		//	}

		//	await ReplyAsync("", false, builder.Build());
		//}




		//[Command("help")]
		//public async Task HelpAsync(string command)
		//{
		//	var result = _service.Search(Context, command);
		//	if (!result.IsSuccess)
		//	{
		//		await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
		//		return;
		//	}

		//	string prefix = Configuration.Load().Prefix;
		//	var builder = new EmbedBuilder()
		//	{
		//		Color = new Color(114, 137, 218),
		//		Description = $"Here are some commands like **{command}**"
		//	};

		//	foreach (var match in result.Commands)
		//	{
		//		var cmd = match.Command;

		//		builder.AddField(x =>
		//		{
		//			x.Name = string.Join(", ", cmd.Aliases);
		//			//x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + $"Remarks: {cmd.Remarks}";
		//			x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\t" + $"Remarks: {cmd.Remarks}\t" + $"Usage: {cmd.Summary}\n";
		//			x.IsInline = false;
		//		});
		//	}

		//	await ReplyAsync("", false, builder.Build());
		//}
	}
}
