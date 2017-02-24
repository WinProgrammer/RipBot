using Discord.Commands;
using Discord.WebSocket;
using RipBot.Types;
using System.Reflection;
using System.Threading.Tasks;
using RipBot.Services;

namespace RipBot
{
	/// <summary> Detect whether a message is a command, then execute it. </summary>
	public class CommandHandler
	{
		private DiscordSocketClient _client;
		private CommandService _cmds;
		private MOTDTimerService _timer;


		/// <summary>
		/// Loads the bots modules and sets some events.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public async Task Install(DiscordSocketClient c)
		{
			_client = c;                                                 // Save an instance of the discord client.
			_cmds = new CommandService();                                // Create a new instance of the commandservice.

			_timer = new MOTDTimerService();                             // Create an instance of our MOTD timer service. 

			await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());    // Load all modules from the assembly.

			_client.MessageReceived += MessageReceived;                    // Register the messagereceived event to handle commands.

			_client.Connected += _client_Connected;
			_client.Disconnected += _client_Disconnected;
		}

		/// <summary>
		/// We're doing it this way so we don't lose heartbeats (and get disconnected) in long running tasks.
		/// </summary>
		/// <param name="s">The message that was recieved.</param>
		/// <returns></returns>
		private Task MessageReceived(SocketMessage s)
		{
			Task.Run(async () =>
			{
				await HandleCommand(s);
			});

			return Task.FromResult(0);
		}


		private async Task HandleCommand(SocketMessage s)
		{
			// Don't handle the command if it is a system message
			var msg = s as SocketUserMessage;
			if (msg == null)                                    // Check if the received message is from a user.
				return;

			var map = new DependencyMap();                      // Create a new dependecy map.
			map.Add(_cmds);										// Add the command service to the dependency map

			map.Add(_timer);									// Add our services to the dependency map

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




		private Task _client_Disconnected(System.Exception arg)
		{
			return Task.CompletedTask;
		}


		private Task _client_Connected()
		{
			//Console.WriteLine("Connected");
			return Task.CompletedTask;
		}



	}
}
