using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RipBot.Types;

namespace RipBot
{
	public static class Globals
	{
		public static string MASHERYAPIKEY = WoWConfiguration.Load().MasheryKey;
		public static string DEFAULTGUILDNAME = WoWConfiguration.Load().DefaultGuildName;
		public static string DEFAULTREALM = WoWConfiguration.Load().DefaultRealm;


		// TODO: while making the motd timer static does work, lets see if we can find a cleaner way of implementing it.
		public static System.Timers.Timer MOTDTIMER = null;
		public static string CURRENTMOTDMESSAGE = MOTDConfiguration.Load().Message;
		public static string CURRENTMOTDINTERVAL = MOTDConfiguration.Load().IntervalInMinutes;
		public static bool TIMERRUNNING = false;


		public static Hashtable OURCHANNELSIDKEY = new Hashtable();
		public static Hashtable OURCHANNELSNAMEKEY = new Hashtable();




		/// <summary>
		/// Hordecorps guild ranks.
		/// </summary>
		public enum GUILDRANK
		{
			GM = 0,
			VP = 1,
			HighCouncil = 2,
			BigWig = 3,
			Corporate = 4,
			Executive = 5,
			Manager = 6,
			Apprentice = 7,
			Intern = 8,
			Applicant = 9
		}
	}
}
