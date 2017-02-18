using System;
//using System.Threading; // 1) Add this namespace
using System.Threading.Tasks;
using Discord.Commands;
using RipBot.Attributes;
using RipBot.Enums;

using System.Timers;
using RipBot.Types;
using Discord.WebSocket;

namespace RipBot.Modules
{
	/// <summary>
	/// Handles all MOTD commands.
	/// </summary>
	[Name("MOTD")]
	[RequireContext(ContextType.Guild)]
	public class MOTDModule : ModuleBase<SocketCommandContext>
	{
		/// <summary>
		/// CStor.
		/// </summary>
		public MOTDModule()
		{

		}


		//[Group("get"), Name("MOTD")]
		//public class Get : ModuleBase<SocketCommandContext>
		//{
		private async void MOTDTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			ulong ul = (ulong)Globals.GUILDCHANNELSBYNAME["general"];
			SocketChannel channel = Context.Client.GetChannel(ul);
			SocketTextChannel sokText = channel as SocketTextChannel;
			await sokText.SendMessageAsync("\n**MOTD:**\n\n" + Globals.CURRENTMOTDMESSAGE + "\n");

			//// this is here because we've been firing off the bot command from a different channel than general
			//// and will also message it
			//await ReplyAsync("Timer fired at " + DateTime.Now.ToString() + "\n");
			//await ReplyAsync("MOTD:\n" + Globals.CURRENTMOTDMESSAGE + "\n");
		}




		/// <summary>
		/// Sets the MOTD message.
		/// </summary>
		/// <param name="message">The message to set the MOTD to.</param>
		/// <returns></returns>
		[Command("setmotd")]
		[Remarks("Sets the MOTD message.\n")]
		[Summary("EX: ripbot setmotd Hey whats up? This is your message.\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task SetMOTDCmd([Remainder]string message)
		{
			var config = new MOTDConfiguration();               // Create a new configuration object.
			config.Message = message;   // Set the default message.
			config.IntervalInMinutes = Globals.CURRENTMOTDINTERVAL;
			config.Save();

			Globals.CURRENTMOTDMESSAGE = config.Message;

			await ReplyAsync("MOTD set to:\n");
			await ReplyAsync(config.Message + "\n");
		}


		/// <summary>
		/// Sets the MOTD message interval in minutes.
		/// </summary>
		/// <param name="intervalinminutes">The number of minutes to wait to show the MOTD.</param>
		/// <returns></returns>
		[Command("setmotdinterval")]
		[Remarks("Sets the MOTD message interval in minutes.\n")]
		[Summary("EX: ripbot setmotdinterval 5\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task SetMOTDIntervalCmd([Remainder]string intervalinminutes)
		{
			// public static string MASHERYAPIKEY = MOTDConfiguration.Load().Message;
			double chk = 0;
			if (!double.TryParse(intervalinminutes, out chk))
			{
				await ReplyAsync("Invalid interval. Please enter a number.\n");
				return;
			}

			// make sure it's high enough so we dont get rate limit banned (which is actually like 10-15 seconds or so)
			if (chk < 1)
			{
				await ReplyAsync("Please enter a number that is 1+ to prevent getting rate limit banned.\n");
				return;
			}

			var config = new MOTDConfiguration();               // Create a new configuration object.
			config.Message = Globals.CURRENTMOTDMESSAGE;
			config.IntervalInMinutes = intervalinminutes;   // Set the new interval in minutes.
			config.Save();

			Globals.CURRENTMOTDINTERVAL = config.IntervalInMinutes;

			// if the timer is running then update it's interval
			if (Globals.TIMERRUNNING)
			{
				Globals.MOTDTIMER.Interval = chk;
				await ReplyAsync("MOTD interval in minutes CHANGED to:\n");
				await ReplyAsync(config.IntervalInMinutes + "\n");
			}
			else
			{
				await ReplyAsync("MOTD interval in minutes set to:\n");
				await ReplyAsync(config.IntervalInMinutes + "\n");
			}

		}


		/// <summary>
		/// Gets the current MOTD message.
		/// </summary>
		/// <returns>The current MOTD message.</returns>
		[Command("getmotd")]
		[Remarks("Gets the current MOTD message.\n")]
		[Summary("EX: ripbot getmotdimessage\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task GetMOTDCmd()
		{
			await ReplyAsync("MOTD message is set to:\n" + MOTDConfiguration.Load().Message + "\n");
			//await ReplyAsync("MOTD message is set to:\n" + Globals.CURRENTMOTDMESSAGE + "\n");
		}


		/// <summary>
		/// Gets the current MOTD message interval in minutes.
		/// </summary>
		/// <returns>The current MOTD interval in minutes.</returns>
		[Command("getmotdinterval")]
		[Remarks("Gets the current MOTD message interval in minutes.\n")]
		[Summary("EX: ripbot getmotdinterval\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task GetMOTDIntervalCmd()
		{
			await ReplyAsync("MOTD interval in minutes set to " + MOTDConfiguration.Load().IntervalInMinutes + "\n");
			//await ReplyAsync("MOTD interval in minutes set to " + Globals.CURRENTMOTDINTERVAL + "\n");
		}



		/// <summary>
		/// Starts the MOTD timer.
		/// </summary>
		/// <returns></returns>
		[Command("startmotdtimer")]
		[Remarks("Starts the MOTD timer.\n")]
		[Summary("EX: ripbot startmotdtimer\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task StartMOTDTimerCmd()
		{
			bool ret = Utility.CacheOurChannels(Context.Guild);
			if (!ret)
			{
				await ReplyAsync("Could not cache channel list.\nMOTD timer not started.\n");
				//return Task.CompletedTask;
				return;
			}

			//string intervalinminutes = MOTDConfiguration.Load().IntervalInMinutes;
			string intervalinminutes = Globals.CURRENTMOTDINTERVAL;

			// make sure it's a valid number
			double chk = 0;
			if (!double.TryParse(intervalinminutes, out chk))
			{
				await ReplyAsync("Invalid interval. Please use 'ripbot setmotdinterval x' command to set the interval in minutes.\n");
				return;
			}
			
			// make sure it's high enough so we dont get rate limit banned (which is actually like 10-15 seconds or so)
			if (chk < 1)
			{
				await ReplyAsync("Please enter a number that is 1+ to prevent getting rate limit banned.\n");
				return;
			}

			double intervalinseconds = (chk * 60) * 1000;
			//300000 = 5 minutes
			// only create it if it isn't running
			if (!Globals.TIMERRUNNING)
			{
				// Create a timer with a 5 second interval
				//aTimer = new Timer(5000);
				Globals.MOTDTIMER = new Timer(intervalinseconds);
				// Hook up the Elapsed event for the timer
				Globals.MOTDTIMER.Elapsed += MOTDTimer_Elapsed;
				Globals.MOTDTIMER.AutoReset = true;
				Globals.MOTDTIMER.Enabled = true;
				Globals.MOTDTIMER.Start();

				Globals.TIMERRUNNING = true;

				await ReplyAsync("Timer started.\n");
			}
			else
			{
				await ReplyAsync("Timer already started.\n");
			}
		}



		/// <summary>
		/// Stops the MOTD timer.
		/// </summary>
		/// <returns></returns>
		[Command("stopmotdtimer")]
		[Remarks("Stops the MOTD timer.\n")]
		[Summary("EX: ripbot stopmotdtimer\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task StopMOTDTimerCmd()
		{
			// only kill it if it is running
			if (Globals.TIMERRUNNING)
			{
				Globals.MOTDTIMER.Enabled = false;
				Globals.MOTDTIMER.Stop();
				// kill the Elapsed event for the timer
				Globals.MOTDTIMER.Elapsed -= MOTDTimer_Elapsed;
				Globals.MOTDTIMER.Dispose();
				Globals.MOTDTIMER = null;

				Globals.TIMERRUNNING = false;

				await ReplyAsync("Timer stopped.\n");
			}
			else
			{
				await ReplyAsync("Timer already stopped.\n");
			}
		}

		//[Command("restarttimer")]
		//[MinPermissions(AccessLevel.ServerAdmin)]
		//public async Task RestartCmd()
		//{
		//	//_service.Restart();
		//	await ReplyAsync("Timer (re)started.");
		//}
	}
	//}

}
