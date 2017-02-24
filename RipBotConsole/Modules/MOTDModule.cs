using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using RipBot.Attributes;
using RipBot.Enums;
using RipBot.Services;
using RipBot.Types;

namespace RipBot.Modules
{
	/// <summary>
	/// Handles all MOTD commands.
	/// </summary>
	[Name("MOTD")]
	[RequireContext(ContextType.Guild)]
	public class MOTDModule : ModuleBase<SocketCommandContext>
	{
		private readonly MOTDTimerService _service;

		/// <summary>
		/// CStor.
		/// </summary>
		/// <param name="service"></param>
		public MOTDModule(MOTDTimerService service)
		{
			_service = service;

			// if the timer is not already running add an event handler
			if (!Globals.TIMERRUNNING)
			{
				_service.MOTDTimerFired += _service_MOTDTimerFired;
			}
		}


		/// <summary>
		/// Handler for the timers elapsed event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void _service_MOTDTimerFired(object sender, EventArgs e)
		{
			//Console.WriteLine("Timer fired from MOTDModule at " + DateTime.Now.ToString());

			if (Globals.GUILDCHANNELSBYNAME["general"] == null)
			{
				await ReplyAsync("Could not get the general text channel at " + DateTime.Now.ToString());
				return;
			}

			ulong ul = (ulong)Globals.GUILDCHANNELSBYNAME["general"];
			SocketChannel channel = Context.Client.GetChannel(ul);
			SocketTextChannel chnGeneral = channel as SocketTextChannel;
			// send the motd to the general guild channel
			await chnGeneral?.SendMessageAsync("\n**MOTD:**\n\n" + Globals.CURRENTMOTDMESSAGE + "\n");
			//              ^ This question mark is used to indicate that 'channel' may sometimes be null, and in cases that it is null, we will do nothing here.

			await ReplyAsync("Timer fired from MOTDModule at " + DateTime.Now.ToString() + "\n\n");

			//	//// this is here because we've been firing off the bot command from a different channel than general
			//	//// and will also message it
			//	//await ReplyAsync("Timer fired at " + DateTime.Now.ToString() + "\n");
			//	//await ReplyAsync("MOTD:\n" + Globals.CURRENTMOTDMESSAGE + "\n");
		}



		/// <summary>
		/// Stops the MOTD timer.
		/// </summary>
		/// <returns></returns>
		[Command("stopmotdtimer")]
		[Remarks("Stops the MOTD timer.\n")]
		[Summary("EX: ripbot stopmotdtimer\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task StopCmd()
		{
			// only kill it if it is running
			if (Globals.TIMERRUNNING)
			{
				_service.Stop();
				Globals.TIMERRUNNING = false;
				// remove the event handler
				_service.MOTDTimerFired -= _service_MOTDTimerFired;

				await ReplyAsync("Timer stopped.\n");
			}
			else
			{
				await ReplyAsync("Timer already stopped.\n");
			}
		}


		/// <summary>
		/// Starts the MOTD timer.
		/// </summary>
		/// <param name="intervalinminutes">Optional. Sets the interval when starting the timer.</param>
		/// <returns></returns>
		[Command("startmotdtimer")]
		[Remarks("Starts the MOTD timer.\n")]
		[Summary("EX: ripbot startmotdtimer\nEX: ripbot startmotdtimer 5\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task RestartCmd([Remainder]string intervalinminutes = null)
		{
			string ourintervalinminutes = intervalinminutes ?? Globals.CURRENTMOTDINTERVAL;

			// if the passed interval is different than the global, update the global
			if (ourintervalinminutes != Globals.CURRENTMOTDINTERVAL)
			{
				await this.SetMOTDIntervalCmd(ourintervalinminutes);
			}

			Globals.TIMERRUNNING = false;

			// cache our current guild channels
			bool ret = Utility.CacheOurChannels(Context.Guild);
			if (!ret)
			{
				await ReplyAsync("Could not cache the guild channel list.\nMOTD timer not started.\n");
				//return Task.CompletedTask;
				return;
			}

			// make sure it's a valid number
			double chk = 0;
			if (!double.TryParse(ourintervalinminutes, out chk))
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

			//double intervalinseconds = (chk * 60) * 1000;
			//300000 = 5 minutes
			// only create it if it isn't running
			if (!Globals.TIMERRUNNING)
			{
				_service.SetInterval(double.Parse(Globals.CURRENTMOTDINTERVAL));
				_service.Restart();
				Globals.TIMERRUNNING = true;

				await ReplyAsync("Timer (re)started.\n");
			}
			else
			{
				// change the interval
				_service.SetInterval(double.Parse(Globals.CURRENTMOTDINTERVAL));
				await ReplyAsync("Timer already started.\n");
			}
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
			// Set the default message.
			config.Message = message;
			// use the global interval
			config.IntervalInMinutes = Globals.CURRENTMOTDINTERVAL;
			// save the config
			config.Save();
			// reload the global motd message setting
			Globals.CURRENTMOTDMESSAGE = config.Message;

			await ReplyAsync("MOTD set to:\n\n");
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
			double chk = 0;
			if (!double.TryParse(intervalinminutes, out chk))
			{
				await ReplyAsync("Invalid interval. Please enter a number.\n");
				return;
			}

			// make sure it's high enough so we dont get rate limit banned (which is actually like 10-15 seconds or so)
			if (chk < 1)
			{
				await ReplyAsync("Please enter a number that is 1+ to prevent getting rate limit banned by Discord.\n");
				return;
			}

			// recreate the motd config
			var config = new MOTDConfiguration();
			// use the existing motd message
			config.Message = Globals.CURRENTMOTDMESSAGE;
			// update the interval with our new one
			config.IntervalInMinutes = intervalinminutes;
			// save it
			config.Save();
			// reload the global interval setting
			Globals.CURRENTMOTDINTERVAL = config.IntervalInMinutes;

			bool ret = false;
			if (Globals.TIMERRUNNING)
			{
				// the timer is running so try to change it's interval
				ret = _service.SetInterval(double.Parse(Globals.CURRENTMOTDINTERVAL));

				if (ret)
				{
					await ReplyAsync("MOTD interval in minutes changed to: " + config.IntervalInMinutes + " minutes.\n");
				}
				else
				{
					await ReplyAsync("MOTD interval in minutes **NOT CHANGED** to: " + config.IntervalInMinutes + " minutes.\n");
				}
			}
			else
			{
				await ReplyAsync("MOTD timer isn't running but interval in minutes changed to: " + config.IntervalInMinutes + " minutes in the config.\n");
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





		//private async Task SendMessageToChannel(ulong ChannelId)
		//{
		//	// TODO: maybe use this for MOTD sending
		//	var channel = Context.Client.GetChannel(ChannelId) as ISocketMessageChannel;
		//	await channel?.SendMessageAsync("\n**MOTD:**\n\n" + Globals.CURRENTMOTDMESSAGE + "\n");
		//	//           ^ This question mark is used to indicate that 'channel' may sometimes be null, and in cases that it is null, we will do nothing here.
		//}
	}
}
