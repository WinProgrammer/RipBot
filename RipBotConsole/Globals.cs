using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RipBot.Types;

namespace RipBot
{
	/// <summary>
	/// A class used to store all our Static (global) stuff.
	/// </summary>
	public static class Globals
	{
		/// <summary>
		/// The WoW Mashery API key to use for any Blizzard api calls.
		/// </summary>
		public static string MASHERYAPIKEY = WoWConfiguration.Load().MasheryKey;
		/// <summary>
		/// The default guild name to use for Blizz queries.
		/// </summary>
		public static string DEFAULTGUILDNAME = WoWConfiguration.Load().DefaultGuildName;
		/// <summary>
		/// The default realm to use for Blizz api queries.
		/// </summary>
		public static string DEFAULTREALM = WoWConfiguration.Load().DefaultRealm;




		/// <summary>
		/// The MOTD message to use.
		/// </summary>
		public static string CURRENTMOTDMESSAGE = MOTDConfiguration.Load().Message;
		/// <summary>
		/// The interval in minutes that the MOTD should be displayed.
		/// </summary>
		public static string CURRENTMOTDINTERVAL = MOTDConfiguration.Load().IntervalInMinutes;
		/// <summary>
		/// Static flag that tells us if the MOTD timer is running or not.
		/// </summary>
		public static bool TIMERRUNNING = false;




		/// <summary>
		/// Static cache that holds all the Guilds channels using channel ID as key.
		/// </summary>
		public static Hashtable GUILDCHANNELSBYID = new Hashtable();
		/// <summary>
		/// Static cache that holds all the Guilds channels using channel Name as key.
		/// </summary>
		public static Hashtable GUILDCHANNELSBYNAME = new Hashtable();




		/// <summary>
		/// Hordecorps guild ranks.
		/// </summary>
		public enum GUILDRANK
		{
			/// <summary>
			/// 
			/// </summary>
			GM = 0,
			/// <summary>
			/// 
			/// </summary>
			VP = 1,
			/// <summary>
			/// 
			/// </summary>
			HighCouncil = 2,
			/// <summary>
			/// 
			/// </summary>
			BigWig = 3,
			/// <summary>
			/// 
			/// </summary>
			Corporate = 4,
			/// <summary>
			/// 
			/// </summary>
			Executive = 5,
			/// <summary>
			/// 
			/// </summary>
			Manager = 6,
			/// <summary>
			/// 
			/// </summary>
			Apprentice = 7,
			/// <summary>
			/// 
			/// </summary>
			Intern = 8,
			/// <summary>
			/// 
			/// </summary>
			Applicant = 9
		}
	}
}
