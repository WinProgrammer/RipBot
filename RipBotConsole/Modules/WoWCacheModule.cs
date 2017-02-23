using Discord.Commands;
using Discord.WebSocket;
using RipBot.Attributes;
using RipBot.Enums;
using RipBot.Types;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WowDotNetAPI;
using WowDotNetAPI.Models;
using System.Collections;
using System.Data;
using System.Threading;

namespace RipBot.Modules
{
	/// <summary>
	/// Handles all the database caching.
	/// </summary>
	[Name("WoWCache")]
	[RequireContext(ContextType.Guild)]

	public class WoWCacheModule : ModuleBase<SocketCommandContext>
	{
		//[Command("botsay"), Alias("bsay")]
		//[Remarks("Echo a bot owners input.\n")]
		//[Summary("EX: ripbot botsay Hello\n")]
		//[MinPermissions(AccessLevel.ServerAdmin)]
		//public async Task BotSay([Remainder]string whattosay)
		//{
		//	await ReplyAsync(whattosay);
		//}


		/// <summary>
		/// Updates the GUILD table.
		/// </summary>
		/// <param name="level">The level(s) of the players to update. We do it this way instead of all at once to avoid being Rate Limited.</param>
		/// <param name="optionalguildname"></param>
		/// <returns></returns>
		[Command("updateguildcache"), Alias("ugc")]
		[Remarks("Updates the GUILD and PLAYERS tables in the cache.\n")]
		[Summary("EX: ripbot updateguildcache 110\nEX: ripbot updateguildcache 110 Hordecorp\nEX: ripbot updateguildcache 40-50")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task UpdateGuildCacheCmd(string level, [Remainder]string optionalguildname = null)
		{
			string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;

			StringBuilder sb = new StringBuilder();

			// check if the level passed is a range
			string lowerrange = "";
			string upperrange = "";
			if (level.Contains("-"))
			{
				// it does so parse the lower upper level range
				string[] ranges = level.Split('-');
				lowerrange = ranges[0].Trim();
				upperrange = ranges[1].Trim();

				// make sure the range isn't over level 100
				if (int.Parse(lowerrange) > 100 || int.Parse(upperrange) > 100)
				{
					sb.AppendLine("Level ranges cannot exceed level 100.\nLevels 101 thru 110 must be run individually.");
					await ReplyAsync(sb.ToString());
					return;
				}
				
				// make sure the range isn't over 100
				if (int.Parse(upperrange) - int.Parse(lowerrange) > 10)
				{
					sb.AppendLine("Level ranges cannot exceed 10 levels at once.\nEX: ripbot updateguildcache 50-60 is ok.\nEX: ripbot updateguildcache 50-61 is NOT ok.");
					await ReplyAsync(sb.ToString());
					return;
				}
			}


			WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
			Character player = null;

			sb.AppendLine("Attempting to update level " + level + " members of " + guildname + "  (" + DateTime.Now.ToString() + ")");
			await ReplyAsync(sb.ToString());

			DataAccess da = new DataAccess();

			// check if guild exists
			bool ret = false;
			ret = da.DoesGuildExist(guildname);

			// if it doesn't, get it
			if (!ret)
			{
				Guild theguild = null;

				try
				{
					theguild = explorer.GetGuild(Globals.DEFAULTREALM, guildname, GuildOptions.GetEverything);

					UpdateGuildInfo(theguild);
				}
				catch
				{
					sb.AppendLine("Guild" + guildname + " not found.");
					await ReplyAsync(sb.ToString());

					//txtResults.Text = "Guild" + txtGuild.Text + " not found";
					return;
				}

				theguild = null;
			}


			int numofplayers = 0;
			int numofupdatedplayers = 0;


			// Get the list of playernames in a guild from the cache who have a matching lvl
			string sql = "SELECT PlayerName FROM PLAYERS WHERE Level = " + level + " AND GuildName = '" + guildname + "'";
			if (!string.IsNullOrEmpty(lowerrange) && !string.IsNullOrEmpty(upperrange))
			{
				sql = "SELECT PlayerName FROM PLAYERS WHERE Level >= " + lowerrange + " AND Level <= " + upperrange + " AND GuildName = '" + guildname + "'";
			}

			DataTable matchingplayersfromcache = da.GetTable(sql, "Matches");
			if (matchingplayersfromcache != null & matchingplayersfromcache.Rows.Count > 0)
			{
				numofplayers = matchingplayersfromcache.Rows.Count;
				numofupdatedplayers = 0;


				// loop through them
				foreach (DataRow dr in matchingplayersfromcache.Rows)
				{
					try
					{
						player = explorer.GetCharacter(Globals.DEFAULTREALM, dr["PlayerName"].ToString(), CharacterOptions.GetEverything);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						sb.AppendLine("Player " + dr["PlayerName"].ToString() + " not found.\nREASON: " + ex.Message + "\n");
						continue;
					}


					// upsert the player
					ret = false;
					try
					{
						ret = da.UpdatePlayer(player);
					}
					catch (Exception exx)
					{
						Console.WriteLine(exx.Message);
					}
					if (ret)
					{
						numofupdatedplayers++;
					}
					else
					{
						Console.WriteLine("");
						sb.AppendLine("Player " + dr["PlayerName"].ToString() + " not updated.");
						continue;
					}

					//Thread.Sleep(2000);
				}
			}
			else
			{
				sb.AppendLine("There aren't any level " + level.ToString() + " players in the guild.  (" + DateTime.Now.ToString() + ")\n");
			}




			matchingplayersfromcache = null;
			da.Dispose();
			da = null;
			player = null;

			await ReplyAsync(sb.ToString() + "\n");
			await ReplyAsync("FINISHED adding/updating " + numofupdatedplayers.ToString() + " level " + level + " players out of " + numofplayers.ToString() + "  (" + DateTime.Now.ToString() + ")\n");
		}


		/// <summary>
		/// Does the actual updating of the guild and players.
		/// </summary>
		/// <param name="theguild">The Guild to update.</param>
		private void UpdateGuildInfo(Guild theguild)
		{
			int ret = -1;
			int updates, inserts = 0;

			DataAccess da = new DataAccess();

			// get the guilds lastmodified readable date from database
			DateTime dt = da.GetGuildLastModifiedReadableDate(theguild.Name);

			// compare to lastmodified date from wow api query
			DateTime dtfromwow = Utility.ConvertUnixToLocalTime(theguild.LastModified);

			// if they are different then update the database
			if (dtfromwow > dt)
			{
				// upsert the GUILDS table
				ret = da.UpdateGuildInfo(theguild);

				// if successful, then upsert the PLAYERS table with info from the guilds members
				if (ret != -1)
				{
					//txtResults.Text += "GUILDS table updated\r\n";
					ret = da.UpdateGuildMembers(theguild, out inserts, out updates);

					if (ret != -1)
					{
						//txtResults.Text += "GUILDMEMBERS table updated with " + ret.ToString() + " players.\r\n";
						//txtResults.Text += "GUILDMEMBERS table had " + inserts.ToString() + " inserts.\r\n";
						//txtResults.Text += "GUILDMEMBERS table had " + updates.ToString() + " updates.\r\n";
					}
				}
			}

			//txtResults.Text += "Finished updating guild info.\r\n";

			da.Dispose();
			da = null;
		}



		/// <summary>
		/// Purges a guilds players from the cache who are no longer are in the guild. (PLAYERS table)
		/// </summary>
		/// <param name="optionalguildname">The guild name to purge.</param>
		/// <returns></returns>
		[Command("purgeplayers"), Alias("pp")]
		[Remarks("Purges a guilds players from the cache who are no longer are in the guild. (PLAYERS table)\n")]
		[Summary("EX: ripbot purgeplayers\nEX: ripbot purgeplayers Hordecorp")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task PurgePlayersCmd([Remainder] string optionalguildname = null)
		{
			string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;

			await ReplyAsync("Attempting to purge players in " + guildname + "  (" + DateTime.Now.ToString() + ").\n");

			List<string> playerstoremove = new List<string>();


			#region pull guildmembers list from Blizz

			// pull guildmembers list from Blizz
			Hashtable guildmembers = new Hashtable();
			WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
			Guild theguild = null;
			try
			{
				theguild = explorer.GetGuild(Globals.DEFAULTREALM, guildname, GuildOptions.GetEverything);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
				{
					//await ReplyAsync("Blizzard API service is down.\n");
					//await ReplyAsync(ex.Message + "\n");
				}
				await ReplyAsync(ex.Message + "\n");
				await ReplyAsync(guildname + " guild not found.\n");
				return;
			}

			// Update the guild and guildmember(PLAYERS) tables
			UpdateGuildInfo(theguild);

			// loop through the guildmembers and add to HashTable using players name as key
			foreach (GuildMember guildmember in theguild.Members)
			{
				guildmembers.Add(guildmember.Character.Name, guildmember.Character.Name);
			}

			theguild = null;
			explorer = null;

			#endregion


			#region pull players list from cache

			// create players list from cache
			//List<string> players = new List<string>();
			DataAccess da = new DataAccess();
			List<string> players = da.GetPlayers(guildname);
			players.Sort();

			#endregion


			#region store the playernames to purge

			// store the playernames to purge
			foreach (string playername in players)
			{
				// check if player exist in guildmember list
				if (guildmembers.ContainsKey(playername))
					continue;

				// if it doesn't then add to removal list
				playerstoremove.Add(playername);

			}

			#endregion


			// remove the players from the cache who are in the removal list
			int playersdeletedfromguild = da.PurgePlayers(playerstoremove);
			da.Dispose();
			da = null;



			await ReplyAsync(playersdeletedfromguild + " players purged from " + guildname + " cache out of " + playerstoremove.Count.ToString() + " to be purged  (" + DateTime.Now.ToString() + ").\n");
			//await ReplyAsync(guildname + " had " + players.Count.ToString() + " members and now has " + DateTime.Now.ToString() + " members.\n");
		}

	}
}
