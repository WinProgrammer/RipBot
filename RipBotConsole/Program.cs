using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using RipBot.Types;
using System.IO;
using Discord.Commands;

namespace RipBot
{
	public class Program
	{
		//// Convert our sync main to an async main.
		//public static void Main(string[] args) =>
		//	new Program().Start().GetAwaiter().GetResult();

		//private DiscordSocketClient client;
		//private CommandHandler handler;

		//public async Task Start()
		//{
		//	EnsureBotConfigExists();                            // Ensure the configuration file has been created.

		//	EnsureWoWConfigExists();



		//	client = new DiscordSocketClient(new DiscordSocketConfig()
		//	{
		//		LogLevel = LogSeverity.Verbose                  // Specify console verbose information level.
		//	});

		//	client.Log += (l)                               // Register the console log event.
		//		=> Task.Run(()
		//		=> Console.WriteLine($"[{l.Severity}] {l.Source}: {l.Exception?.ToString() ?? l.Message}"));

		//	client.Log += Logger;

		//	await client.LoginAsync(TokenType.Bot, BotConfiguration.Load().Token);
		//	await client.ConnectAsync();

		//	//	_commands = new CommandHandler();               // Initialize the command handler service
		//	//	await _commands.Install(_client);

		//	//	await Task.Delay(-1);                            // Prevent the console window from closing.
		//	//}







		//	//// Define the DiscordSocketClient
		//	//client = new DiscordSocketClient();

		//	//var token = "token here";

		//	//// Login and connect to Discord.
		//	//await client.LoginAsync(TokenType.Bot, token);
		//	//await client.ConnectAsync();

		//	var map = new DependencyMap();
		//	map.Add(client);

		//	handler = new CommandHandler();
		//	await handler.Install(map);

		//	// Block this program until it is closed.
		//	await Task.Delay(-1);
		//}

		////private Task Log(LogMessage msg)
		////{
		////	Console.WriteLine(msg.ToString());
		////	return Task.CompletedTask;
		////}











		public static void Main(string[] args)
			=> new Program().Start().GetAwaiter().GetResult();


		private DiscordSocketClient _client;
		private CommandHandler _commands;


		public async Task Start()
		{
			EnsureBotConfigExists();                            // Ensure the configuration file has been created.
			EnsureWoWConfigExists();
			EnsureMOTDConfigExists();


			// Create a new instance of DiscordSocketClient.
			_client = new DiscordSocketClient(new DiscordSocketConfig()
			{
				LogLevel = LogSeverity.Verbose                  // Specify console verbose information level.
			});

			//_client.Log += (l)                               // Register the console log event.
			//	=> Task.Run(()
			//	=> Console.WriteLine($"[{l.Severity}] {l.Source}: {l.Exception?.ToString() ?? l.Message}"));

			_client.Log += Logger;

			await _client.LoginAsync(TokenType.Bot, BotConfiguration.Load().Token);
			await _client.ConnectAsync();

			_commands = new CommandHandler();               // Initialize the command handler service
			await _commands.Install(_client);


			await Task.Delay(-1);                            // Prevent the console window from closing.
		}

		
		
		// Create a named logging handler, so it can be re-used by addons that ask for a Func<LogMessage, Task>.
		private static Task Logger(LogMessage lmsg)
		{
			var cc = Console.ForegroundColor;
			switch (lmsg.Severity)
			{
				case LogSeverity.Critical:
				case LogSeverity.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LogSeverity.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogSeverity.Info:
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case LogSeverity.Verbose:
				case LogSeverity.Debug:
					Console.ForegroundColor = ConsoleColor.DarkGray;
					break;
			}
			Console.WriteLine($"{DateTime.Now,-19} [{lmsg.Severity,8}] {lmsg.Source}: {lmsg.Message}");
			Console.ForegroundColor = cc;
			return Task.CompletedTask;
		}




		public static void EnsureBotConfigExists()
		{
			if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")))
				Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));

			string loc = Path.Combine(AppContext.BaseDirectory, "data/botconfiguration.json");

			if (!File.Exists(loc))                              // Check if the configuration file exists.
			{
				var config = new BotConfiguration();               // Create a new configuration object.

				Console.WriteLine("The bot configuration file has been created at 'data\\botconfiguration.json', " +
							  "please enter your information and restart the bot.");

				Console.Write("Bot Token: ");
				config.Token = Console.ReadLine();              // Read the bot token from console.

				Console.Write("Bot Prefix: ");
				config.Prefix = Console.ReadLine();              // Read the bot prefix from console.

				config.Save();                                  // Save the new configuration object to file.
			}
			Console.WriteLine("Bot configuration Loaded...");
		}


		public static void EnsureWoWConfigExists()
		{
			if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")))
				Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));

			string loc = Path.Combine(AppContext.BaseDirectory, "data/wowconfiguration.json");

			if (!File.Exists(loc))                              // Check if the configuration file exists.
			{
				var config = new WoWConfiguration();               // Create a new configuration object.

				Console.WriteLine("The WoW configuration file has been created at 'data\\wowconfiguration.json', " +
							  "please enter your information and restart the bot.");

				Console.Write("Mashery Key: ");
				config.MasheryKey = Console.ReadLine();              // Read the Mashery token from console.

				Console.Write("Default Guild name: ");
				config.DefaultGuildName = Console.ReadLine();              // Read the default guild name from console.

				Console.Write("Default Guild realm: ");
				config.DefaultRealm = Console.ReadLine();              // Read the default realm from console.

				config.Save();                                  // Save the new configuration object to file.
			}
			Console.WriteLine("WoW configuration Loaded...");
		}


		public static void EnsureMOTDConfigExists()
		{
			if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")))
				Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));

			string loc = Path.Combine(AppContext.BaseDirectory, "data/motdconfiguration.json");

			if (!File.Exists(loc))                              // Check if the configuration file exists.
			{
				var config = new MOTDConfiguration();               // Create a new configuration object.

				config.Message = "Default MOTD message.";	// Set the default message.
				config.IntervalInMinutes = "5";			// set the default timer to 5 minutes.

				config.Save();                                  // Save the new configuration object to file.
			}
			Console.WriteLine("MOTD configuration Loaded...");
		}

	}
}
