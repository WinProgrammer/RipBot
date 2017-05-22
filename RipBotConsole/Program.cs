using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using Discord;
using Discord.WebSocket;
using RipBot.Types;
using System.IO;
using Discord.Commands;

namespace RipBot
{
	/// <summary>
	/// Entry point.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The name of the service.
		/// </summary>
		public const string Name = "RipBot";
		/// <summary>
		/// The description of the service.
		/// </summary>
		public const string Description = "Discord RipBot service";

		static void Main(string[] args)
		{
			BasicServiceStarter.Run<MyService>(Name, Description);
		}
	}


	/// <summary>
	/// The actual service to be run.
	/// </summary>
	class MyService : IService
	{
		private bool _stopped = true;

		public async void Start()
		{
			await StartBot();
		}

		public void Dispose()
		{
			_stopped = true;
			StopBot();
			//System.Environment.Exit(-1);
		}



		#region Bot code

		private DiscordSocketClient _client;
		private CommandHandler _commands;


		/// <summary>
		/// Starts the bot.
		/// </summary>
		/// <returns></returns>
		public async Task StartBot()
		{
			EnsureBotConfigExists();                            // Ensure the configuration file has been created.
			EnsureWoWConfigExists();
			EnsureMOTDConfigExists();


			// Create a new instance of DiscordSocketClient.
			_client = new DiscordSocketClient(new DiscordSocketConfig()
			{
				LogLevel = LogSeverity.Verbose                  // Specify console verbose information level.
			});

			_client.Log += (l)                               // Register the console log event.
				=> Task.Run(()
				=> Console.WriteLine($"[{l.Severity}] {l.Source}: {l.Exception?.ToString() ?? l.Message}"));

			_client.Log += Logger;

			await _client.LoginAsync(TokenType.Bot, BotConfiguration.Load().Token);
			await _client.StartAsync();

			_commands = new CommandHandler();               // Initialize the command handler service
			await _commands.Install(_client);

			_stopped = false;
			while (!_stopped)
			{ }
			//await Task.Delay(-1);                            // Prevent the console window from closing.
		}

		public void StopBot()
		{
			_stopped = true;
			//System.Environment.Exit(-1);
			//Application.Exit;
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




		/// <summary>
		/// Ensures the bots configuration file exists, prompting for the info if it doesn't.
		/// </summary>
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

				Console.Write("Log bot commands (true or false): ");
				string tmp = Console.ReadLine();
				config.LogCommands = bool.Parse(tmp);              // Use command logging?.

				config.Save();                                  // Save the new configuration object to file.
			}
			Console.WriteLine("Bot configuration Loaded...");
		}


		/// <summary>
		/// Ensures the WoW configuration file exists, prompting for the info if it doesn't.
		/// </summary>
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


		/// <summary>
		/// Ensures the MOTD configuration file exists, creating a default one if it doesn't.
		/// </summary>
		public static void EnsureMOTDConfigExists()
		{
			if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")))
				Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));

			string loc = Path.Combine(AppContext.BaseDirectory, "data/motdconfiguration.json");

			if (!File.Exists(loc))                              // Check if the configuration file exists.
			{
				var config = new MOTDConfiguration();               // Create a new configuration object.

				config.Message = "Default MOTD message.";   // Set the default message.
				config.IntervalInMinutes = "5";         // set the default timer to 5 minutes.

				config.Save();                                  // Save the new configuration object to file.
			}
			Console.WriteLine("MOTD configuration Loaded...");
		}


		#endregion


	}



	#region Service installer stuff

	[RunInstaller(true)]
	public class MyInstaller : Installer
	{
		public MyInstaller()
		{
			Installers.Add(new ServiceProcessInstaller
			{
				Account = ServiceAccount.LocalSystem
			});
			Installers.Add(new ServiceInstaller
			{
				ServiceName = Program.Name,
				DisplayName = Program.Name,
				Description = Program.Description
			});
		}
	}

	static class BasicServiceInstaller
	{
		public static void Install(string serviceName, string serviceDescription)
		{
			CreateInstaller(serviceName, serviceDescription).Install(new Hashtable());
		}

		public static void Uninstall(string serviceName, string serviceDescription)
		{
			CreateInstaller(serviceName, serviceDescription).Uninstall(null);
		}

		private static Installer CreateInstaller(string serviceName, string serviceDescription)
		{
			var installer = new TransactedInstaller();
			installer.Installers.Add(new ServiceInstaller
			{
				ServiceName = serviceName,
				DisplayName = serviceName,
				Description = serviceDescription,
				StartType = ServiceStartMode.Manual
			});
			installer.Installers.Add(new ServiceProcessInstaller
			{
				Account = ServiceAccount.LocalSystem
			});
			var installContext = new InstallContext(
			serviceName + ".install.log", null);
			installContext.Parameters["assemblypath"] = Assembly.GetEntryAssembly().Location;
			installer.Context = installContext;
			return installer;
		}
	}

	#endregion


	#region Service related stuff

	public interface IService : IDisposable
	{
		void Start();
	}

	public class BasicService : ServiceBase
	{
		private readonly IService _service;

		public BasicService(IService service, string name)
		{
			_service = service;
			ServiceName = name;
		}

		protected override void OnStart(string[] args)
		{
			_service.Start();
		}

		protected override void OnStop()
		{
			_service.Dispose();
		}
	}

	/// <summary>
	/// Handles how to run the program. As a service or a console app.
	/// </summary>
	public static class BasicServiceStarter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serviceName"></param>
		/// <param name="serviceDescription"></param>
		public static void Run<T>(string serviceName, string serviceDescription) where T : IService, new()
		{
			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				if (EventLog.SourceExists(serviceName))
				{
					EventLog.WriteEntry(serviceName,
						"Fatal Exception : " + Environment.NewLine +
						e.ExceptionObject, EventLogEntryType.Error);
				}
			};

			if (Environment.UserInteractive)
			{
				var cmd =
			(Environment.GetCommandLineArgs().Skip(1).FirstOrDefault() ?? "")
			.ToLower();
				switch (cmd)
				{
					case "i":
					case "install":
						Console.WriteLine("Installing {0}", serviceName);
						BasicServiceInstaller.Install(serviceName, serviceDescription);
						break;
					case "u":
					case "uninstall":
						Console.WriteLine("Uninstalling {0}", serviceName);
						BasicServiceInstaller.Uninstall(serviceName, serviceDescription);
						break;
					default:
						using (var service = new T())
						{
							service.Start();
							Console.WriteLine("Running {0}, press any key to stop", serviceName);
							Console.ReadKey();
						}
						break;
				}
			}
			else
			{
				ServiceBase.Run(new BasicService<T> { ServiceName = serviceName });
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BasicService<T> : ServiceBase where T : IService, new()
	{
		private IService _service;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			try
			{
				_service = new T();
				_service.Start();
			}
			catch
			{
				ExitCode = 1064;
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnStop()
		{
			_service.Dispose();
		}
	}

	#endregion
}
