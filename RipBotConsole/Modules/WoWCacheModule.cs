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
	[Name("WoWCache")]

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


		[Command("updateguildcache"), Alias("ugc")]
		[Remarks("Updates the GUILD table.\n")]
		[Summary("EX: ripbot updateguildcache 110\nEX: ripbot updateguildcache 110 Hordecorp")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task UpdateGuildCache(string level, [Remainder]string optionalguildname = null)
		{
			string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;

			WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
			Character player = null;
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Attempting to update " + guildname);
			await ReplyAsync(sb.ToString());

			DataAccess da = new DataAccess();

			//// check if guild exists
			bool ret = false;
			//ret = da.DoesGuildExist(guildname);

			//// if it doesn't, get it
			//if (!ret)
			//{
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
			//}


			int numofplayers =0;
			int numofupdatedplayers = 0;


			// Get the list of playernames in a guild who have a matching ilvl
			DataTable matchingplayers = da.GetTable("SELECT PlayerName FROM PLAYERS WHERE Level = " + level + " AND GuildName = '" + guildname + "'", "Matches");
			if (matchingplayers != null & matchingplayers.Rows.Count > 0)
			{


				numofplayers = matchingplayers.Rows.Count;
				numofupdatedplayers = 0;


				// loop through them
				foreach (DataRow dr in matchingplayers.Rows)
				{
					//Character player = explorer.GetCharacter("aerie peak", txtPlayerName.Text, CharacterOptions.GetEverything);
					try
					{
						//player = explorer.GetCharacter("aerie peak", txtPlayerName.Text, CharacterOptions.GetItems | CharacterOptions.GetStats | CharacterOptions.GetAchievements);
						//player = explorer.GetCharacter("aerie peak", txtPlayerName.Text, CharacterOptions.GetEverything);
						//player = explorer.GetCharacter(Utility.EncloseInQuotes(txtRealm.Text.ToLower()), txtPlayerName.Text, CharacterOptions.GetEverything);
						player = explorer.GetCharacter(Globals.DEFAULTREALM, dr["PlayerName"].ToString(), CharacterOptions.GetEverything);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						sb.AppendLine("Player " + dr["PlayerName"].ToString() + " not found.");
						//await ReplyAsync("Player " + dr["PlayerName"].ToString() + " not found.\r\n");
						//Application.DoEvents();
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
						//txtResults.Text += "Player " + dr["PlayerName"].ToString() + " updated.\r\n";
					}
					else
					{
						sb.AppendLine("Player " + dr["PlayerName"].ToString() + " not updated.");
						//await ReplyAsync("Player " + dr["PlayerName"].ToString() + " not updated.\r\n");
						//txtResults.Text += "Player " + dr["PlayerName"].ToString() + " not updated.\r\n";
						continue;
					}

					//Application.DoEvents();
					//Thread.Sleep(2000);
				}

			}
			else
			{
				sb.AppendLine("There aren't any level " + level.ToString() + " players in the guild.");
				//await ReplyAsync("There aren't any level " + level.ToString() + " players in the guild.\r\n");
			}




			matchingplayers = null;
			da = null;
			player = null;

			await ReplyAsync(sb.ToString() + "\n");
			await ReplyAsync("FINISHED adding/updating " + numofupdatedplayers.ToString() + " level " + level + " players out of " + numofplayers.ToString() + "\r\n");
		}


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

			da = null;
		}



	}
}
