using Discord.Commands;
using Discord.WebSocket;
using RipBot.Types;
using System.Reflection;
using System.Threading.Tasks;

namespace RipBot
{
	/// <summary> Detect whether a message is a command, then execute it. </summary>
	public class CommandHandler
	{
		private DiscordSocketClient _client;
		private CommandService _cmds;


		//private IDependencyMap map;




		public async Task Install(DiscordSocketClient c)
		{
			_client = c;                                                 // Save an instance of the discord client.
			_cmds = new CommandService();                                // Create a new instance of the commandservice.                              

			await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());    // Load all modules from the assembly.
			
			_client.MessageReceived += HandleCommand;                    // Register the messagereceived event to handle commands.

			_client.Connected += _client_Connected;
			_client.Disconnected += _client_Disconnected;
		}

		private Task _client_Disconnected(System.Exception arg)
		{
			return Task.CompletedTask;
		}


		//private Task _client_Disconnected(Exception arg)
		//{
		//	//Console.WriteLine("Disconnected");
		//	return Task.CompletedTask;
		//}

		private Task _client_Connected()
		{
			//Console.WriteLine("Connected");
			return Task.CompletedTask;
		}




		//public async Task Install(IDependencyMap _map)
		//{
		//	// Create Command Service, inject it into Dependency Map
		//	_client = _map.Get<DiscordSocketClient>();
		//	_cmds = new CommandService();
		//	_map.Add(_cmds);
		//	map = _map;

		//	await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());

		//	_client.MessageReceived += HandleCommand;
		//}




		private async Task HandleCommand(SocketMessage s)
		{
			//// Don't handle the command if it is a system message
			//var msg = s as SocketUserMessage;
			//if (msg == null) return;

			//// Mark where the prefix ends and the command begins
			//int argPos = 0;
			//// Determine if the message has a valid prefix, adjust argPos 
			////if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos))) return;
			//if (!(msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(BotConfiguration.Load().Prefix, ref argPos))) return;

			//// Create a Command Context
			//var context = new CommandContext(_client, msg);
			//// Execute the Command, store the result
			//var result = await _cmds.ExecuteAsync(context, argPos, map);

			//// If the command failed, notify the user
			//if (!result.IsSuccess)
			//	await msg.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");






			// Don't handle the command if it is a system message
			var msg = s as SocketUserMessage;
			if (msg == null)                                    // Check if the received message is from a user.
				return;

			var map = new DependencyMap();                      // Create a new dependecy map.
			map.Add(_cmds);
			var context = new SocketCommandContext(_client, msg);     // Create a new command context.

			int argPos = 0;                                     // Check if the message has either a string or mention prefix.
			if (msg.HasStringPrefix(BotConfiguration.Load().Prefix, ref argPos) ||
				msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{                                                   // Try and execute a command with the given context.
				var result = await _cmds.ExecuteAsync(context, argPos, map);

				if (!result.IsSuccess)                          // If execution failed, reply with the error message.
					await context.Channel.SendMessageAsync(result.ToString());
			}
		}
	}
}
