using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Threading;
using RipBot.Types;
using System.Collections;

namespace RipBot
{
	/// <summary>
	/// Various utility functions.
	/// </summary>
	public static class Utility
	{
		private static CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
		private static TextInfo txtInfo = cultureInfo.TextInfo;
		private static DateTime unixstartdate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


		//static Utility()
		//{

		//}



		public static ulong GetChannelID(Discord.WebSocket.SocketGuild guild, string channelname)
		{
			ulong ret = 0;

			foreach (Discord.WebSocket.SocketGuildChannel sgc in guild.Channels)
			{
				//sb.AppendLine(string.Format("Guild: {0}\tChannel ID: {1}\tChannel name: {2}", sgc.Guild.Name, sgc.Id.ToString(), sgc.Name));
				if (sgc.Name == channelname)
				{
					ret = sgc.Id;
					break;
				}
			}
			
			return ret;
		}

		public static string GetChannelName(Discord.WebSocket.SocketGuild guild, ulong channelid)
		{
			string ret = "";

			foreach (Discord.WebSocket.SocketGuildChannel sgc in guild.Channels)
			{
				//sb.AppendLine(string.Format("Guild: {0}\tChannel ID: {1}\tChannel name: {2}", sgc.Guild.Name, sgc.Id.ToString(), sgc.Name));
				if (sgc.Id == channelid) ret = sgc.Name;
			}

			return ret;
		}


		public static bool CacheOurChannels(Discord.WebSocket.SocketGuild guild)
		{
			Globals.OURCHANNELSIDKEY = new System.Collections.Hashtable();
			Globals.OURCHANNELSNAMEKEY = new System.Collections.Hashtable();

			try
			{
				foreach (Discord.WebSocket.SocketGuildChannel sgc in guild.Channels)
				{
					//sb.AppendLine(string.Format("Guild: {0}\tChannel ID: {1}\tChannel name: {2}", sgc.Guild.Name, sgc.Id.ToString(), sgc.Name));
					Globals.OURCHANNELSIDKEY.Add(sgc.Id, sgc.Name);
					Globals.OURCHANNELSNAMEKEY.Add(sgc.Name, sgc.Id);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}

			return true;
		}




		/// <summary>
		/// Convert unix time to Local time.
		/// </summary>
		/// <param name="unixdatetime"></param>
		/// <returns></returns>
		public static DateTime ConvertUnixToLocalTime(long unixdatetime)
		{
			// this was added in .NET 4.6
			//DateTime localDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixdatetime).DateTime.ToLocalTime();
			DateTime localDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixdatetime).DateTime.ToLocalTime();

			// pre .NET 4.6 way of doing it
			//DateTime localDateTimeOffset = unixstartdate.AddMilliseconds(unixdatetime).ToLocalTime();

			return localDateTimeOffset;
		}

		/// <summary>
		/// Convert unix time to Local time.
		/// </summary>
		/// <param name="unixdatetime"></param>
		/// <returns></returns>
		public static DateTime ConvertUnixToLocalTime(string unixdatetime)
		{
			// this was added in .NET 4.6
			//DateTime localDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(unixdatetime)).DateTime.ToLocalTime();
			DateTime localDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(unixdatetime)).DateTime.ToLocalTime();

			// pre .NET 4.6 way of doing it
			//DateTime localDateTimeOffset = unixstartdate.AddMilliseconds(long.Parse(unixdatetime)).ToLocalTime();

			return localDateTimeOffset;
		}


		public static long ConvertLocalTimeToUnix(DateTime localdatetime)
		{
			// these were added in .NET 4.6

			//DateTime dateTime = new DateTime(2017, 02, 11, 10, 2, 0, DateTimeKind.Local);
			//DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
			//long unixDateTime = dateTimeOffset.ToUnixTimeSeconds();

			DateTimeOffset dateTimeOffset = new DateTimeOffset(localdatetime);
			long unixDateTime = dateTimeOffset.ToUnixTimeMilliseconds();

			return unixDateTime;
		}





		/// <summary>
		/// Converts a string to proper case.
		/// </summary>
		/// <param name="txt">The string to convert to proper case.</param>
		/// <returns>The passed string converted to proper case.</returns>
		public static string ToTitleCase(string txt)
		{
			return txtInfo.ToTitleCase(txt.ToLower()); ;
		}



		public static string EncloseInQuotes(string texttoenclose)
		{
			string quote = "\"";

			return quote + texttoenclose + quote;
		}

	}
}
