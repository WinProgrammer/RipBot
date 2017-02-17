﻿using Discord.Commands;
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
using Discord;

namespace RipBot.Modules
{
	[Name("WoW")]
	[RequireContext(ContextType.Guild)]
	public class WoWModule : ModuleBase<SocketCommandContext>
	{
		[Command("adminsay"), Alias("as")]
		[Remarks("Make the bot echo something by a server admin.\n")]
		[Summary("EX: ripbot adminsay Hello\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task AdminSay([Remainder]string text)
		{
			await ReplyAsync(text);
		}

		[Command("modsay"), Alias("ms")]
		[Remarks("Make the bot echo something by a server mod.\n")]
		[Summary("EX: ripbot modsay Hello\n")]
		[MinPermissions(AccessLevel.ServerMod)]
		public async Task ModSay([Remainder]string text)
		{
			await ReplyAsync(text);
		}





		[Command("whoishighcouncil"), Alias("wihc")]
		[Remarks("Display all Hordecorp High Council members.\nThis command ONLY works for Hordecorp.\n")]
		[Summary("EX: ripbot whoishighcouncil\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task WhoIsHighCouncil()
		{
			StringBuilder sb = new StringBuilder();
			DataAccess da = new DataAccess();
			List<string> hc = da.GetHighCouncilMembers();
			da = null;

			hc.Sort();

			foreach (string player in hc)
			{
				sb.AppendLine(player + " is High Council");
			}


			await ReplyAsync(sb.ToString());
		}





		[Command("getgear"), Alias("gg")]
		[Remarks("Gets the gear for a player.\n")]
		[Summary("EX: ripbot getgear Ripgut\nEX: ripbot getgear Ripgut Hordecorp\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task GetGear(string playername, [Remainder] string optionalguildname = null)
		{
			string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;


			StringBuilder sb = new StringBuilder();
			bool ret = false;
			WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
			Character player = null;

			try
			{
				player = explorer.GetCharacter(Globals.DEFAULTREALM, playername, CharacterOptions.GetEverything);
			}
			catch(Exception ex)
			{
				if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
				{
					sb.AppendLine("Blizzard API service is down.");
				}
				if (ex.HResult == -2146233076)  // seems to happen on Wrobbinhuud
				{
					sb.AppendLine("Player " + playername + " Error deserializing object.");
				}

				sb.AppendLine("Player " + playername + " not found.");
				await ReplyAsync(sb.ToString());
				return;
			}

			DataAccess da = new DataAccess();
			// check if guild exists
			ret = da.DoesGuildExist(player.Guild.Name);
			// if it doesn't, get it
			if (!ret)
			{
				Guild theguild = null;

				try
				{
					theguild = explorer.GetGuild(Globals.DEFAULTREALM, player.Guild.Name, GuildOptions.GetEverything);

					UpdateGuildInfo(theguild);
				}
				catch(Exception eex)
				{
					sb.AppendLine(eex.Message);

					if (eex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						sb.AppendLine("Blizzard API service is down.");
					}
					//txtResults.Text = "Guild" + txtGuild.Text + " not found";
					await ReplyAsync(sb.ToString());

					return;
				}

				theguild = null;
			}

			// now upsert the player
			ret = da.UpdatePlayer(player);

			// get the spec and role for use in our output
			string specname = da.GetFieldValue("SELECT SpecName FROM PLAYERS WHERE PlayerName = '" + player.Name + "'");
			string specrole = da.GetFieldValue("SELECT SpecRole FROM PLAYERS WHERE PlayerName = '" + player.Name + "'");


			sb.AppendLine(playername + " is a level " + player.Level + " " + Utility.ToTitleCase(player.Race.ToString()) + " " + specname + " " + Utility.ToTitleCase(player.Class.ToString()) + "  (" + specrole + ")");

			#region build output

			//sb.AppendLine(playername);
			sb.AppendLine("Avg iLvl: " + player.Items.AverageItemLevel);
			sb.AppendLine("Avg Equipped iLvl: " + player.Items.AverageItemLevelEquipped);
			sb.AppendLine();

			if (player.Items.Back != null)
			{ sb.AppendLine(player.Items.Back.ItemLevel + " BACK\t" + player.Items.Back.Name); }
			else { sb.AppendLine("000 BACK\t"); }

			if (player.Items.Chest != null)
			{ sb.AppendLine(player.Items.Chest.ItemLevel + " CHEST\t" + player.Items.Chest.Name); }
			else { sb.AppendLine("000 CHEST\t"); }

			if (player.Items.Feet != null)
			{ sb.AppendLine(player.Items.Feet.ItemLevel + " FEET\t" + player.Items.Feet.Name); }
			else { sb.AppendLine("000 FEET\t"); }

			if (player.Items.Finger1 != null)
			{ sb.AppendLine(player.Items.Finger1.ItemLevel + " FINGER 1\t" + player.Items.Finger1.Name); }
			else { sb.AppendLine("000 FINGER 1\t"); }

			if (player.Items.Finger2 != null)
			{ sb.AppendLine(player.Items.Finger2.ItemLevel + " FINGER 2\t" + player.Items.Finger2.Name); }
			else { sb.AppendLine("000 FINGER 2\t"); }

			if (player.Items.Hands != null)
			{ sb.AppendLine(player.Items.Hands.ItemLevel + " HANDS\t" + player.Items.Hands.Name); }
			else { sb.AppendLine("000 HANDS\t"); }

			if (player.Items.Head != null)
			{ sb.AppendLine(player.Items.Head.ItemLevel + " HEAD\t" + player.Items.Head.Name); }
			else { sb.AppendLine("000 HEAD\t"); }

			if (player.Items.Legs != null)
			{ sb.AppendLine(player.Items.Legs.ItemLevel + " LEGS\t" + player.Items.Legs.Name); }
			else { sb.AppendLine("000 LEGS\t"); }

			if (player.Items.MainHand != null)
			{ sb.AppendLine(player.Items.MainHand.ItemLevel + " MAIN HAND\t" + player.Items.MainHand.Name); }
			else { sb.AppendLine("000 MAIN HAND\t"); }

			if (player.Items.Neck != null)
			{ sb.AppendLine(player.Items.Neck.ItemLevel + " NECK\t" + player.Items.Neck.Name); }
			else { sb.AppendLine("000 NECK\t"); }

			if (player.Items.OffHand != null)
			{ sb.AppendLine(player.Items.OffHand.ItemLevel + " OFF HAND\t" + player.Items.OffHand.Name); }
			else { sb.AppendLine("000 OFF HAND\t"); }

			if (player.Items.Ranged != null)
			{ sb.AppendLine(player.Items.Ranged.ItemLevel + " RANGED\t" + player.Items.Ranged.Name); }
			else { sb.AppendLine("000 RANGED\t"); }

			if (player.Items.Shirt != null)
			{ sb.AppendLine(player.Items.Shirt.ItemLevel + " SHIRT\t\t" + player.Items.Shirt.Name); }
			else { sb.AppendLine("000 SHIRT\t"); }

			if (player.Items.Shoulder != null)
			{ sb.AppendLine(player.Items.Shoulder.ItemLevel + " SHOULDER\t" + player.Items.Shoulder.Name); }
			else { sb.AppendLine("000 SHOULDER\t"); }

			if (player.Items.Tabard != null)
			{ sb.AppendLine(player.Items.Tabard.ItemLevel + " TABARD\t" + player.Items.Tabard.Name); }
			else { sb.AppendLine("000 TABARD\t"); }

			if (player.Items.Trinket1 != null)
			{ sb.AppendLine(player.Items.Trinket1.ItemLevel + " TRINKET 1\t" + player.Items.Trinket1.Name); }
			else { sb.AppendLine("000 TRINKET 1\t"); }

			if (player.Items.Trinket2 != null)
			{ sb.AppendLine(player.Items.Trinket2.ItemLevel + " TRINKET 2\t" + player.Items.Trinket2.Name); }
			else { sb.AppendLine("000 TRINKET 2\t"); }

			if (player.Items.Waist != null)
			{ sb.AppendLine(player.Items.Waist.ItemLevel + " WAIST\t" + player.Items.Waist.Name); }
			else { sb.AppendLine("000 WAIST\t"); }

			if (player.Items.Wrist != null)
			{ sb.AppendLine(player.Items.Wrist.ItemLevel + " WRIST\t" + player.Items.Wrist.Name); }
			else { sb.AppendLine("000 WRIST\t"); }

			////CharacterItem ci = player.Items.Back;
			////ci.Armor;

			#endregion



			player = null;
			explorer = null;


			await ReplyAsync(sb.ToString());
		}


		/// <summary>
		/// Updates the guild cache and guildmembers (PLAYERS table) only if the lastmodified dates are different.
		/// </summary>
		/// <param name="theguild"></param>
		public static void UpdateGuildInfo(Guild theguild)
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




		/// <summary>
		/// Builds an Item = Level Hashtable.
		/// </summary>
		/// <param name="player">The player to parse items for.</param>
		/// <returns></returns>
		private Hashtable ParsePlayerGearForCompare(Character player)
		{
			Hashtable w = new Hashtable();
			//w.Add("key", "value");

			if (player.Items.Back != null)
			{ w.Add("BACK", player.Items.Back.ItemLevel); }
			else { w.Add("BACK", "000"); }

			if (player.Items.Chest != null)
			{ w.Add("CHEST", player.Items.Chest.ItemLevel); }
			else { w.Add("CHEST", "000"); }

			if (player.Items.Feet != null)
			{ w.Add("FEET", player.Items.Feet.ItemLevel); }
			else { w.Add("FEET", "000"); }

			if (player.Items.Finger1 != null)
			{ w.Add("FINGER1", player.Items.Finger1.ItemLevel); }
			else { w.Add("FINGER1", "000"); }

			if (player.Items.Finger2 != null)
			{ w.Add("FINGER2", player.Items.Finger2.ItemLevel); }
			else { w.Add("FINGER2", "000"); }

			if (player.Items.Hands != null)
			{ w.Add("HANDS", player.Items.Hands.ItemLevel); }
			else { w.Add("HANDS", "000"); }

			if (player.Items.Head != null)
			{ w.Add("HEAD", player.Items.Head.ItemLevel); }
			else { w.Add("HEAD", "000"); }

			if (player.Items.Legs != null)
			{ w.Add("LEGS", player.Items.Legs.ItemLevel); }
			else { w.Add("LEGS", "000"); }

			if (player.Items.MainHand != null)
			{ w.Add("MAINHAND", player.Items.MainHand.ItemLevel); }
			else { w.Add("MAINHAND", "000"); }

			if (player.Items.Neck != null)
			{ w.Add("NECK", player.Items.Neck.ItemLevel); }
			else { w.Add("NECK", "000"); }

			if (player.Items.OffHand != null)
			{ w.Add("OFFHAND", player.Items.OffHand.ItemLevel); }
			else { w.Add("OFFHAND", "000"); }

			if (player.Items.Ranged != null)
			{ w.Add("RANGED", player.Items.Ranged.ItemLevel); }
			else { w.Add("RANGED", "000"); }

			if (player.Items.Shirt != null)
			{ w.Add("SHIRT", player.Items.Shirt.ItemLevel); }
			else { w.Add("SHIRT", "000"); }

			if (player.Items.Shoulder != null)
			{ w.Add("SHOULDER", player.Items.Shoulder.ItemLevel); }
			else { w.Add("SHOULDER", "000"); }

			if (player.Items.Tabard != null)
			{ w.Add("TABARD", player.Items.Tabard.ItemLevel); }
			else { w.Add("TABARD", "000"); }

			if (player.Items.Trinket1 != null)
			{ w.Add("TRINKET1", player.Items.Trinket1.ItemLevel); }
			else { w.Add("TRINKET1", "000"); }

			if (player.Items.Trinket2 != null)
			{ w.Add("TRINKET2", player.Items.Trinket2.ItemLevel); }
			else { w.Add("TRINKET2", "000"); }

			if (player.Items.Waist != null)
			{ w.Add("WAIST", player.Items.Waist.ItemLevel); }
			else { w.Add("WAIST", "000"); }

			if (player.Items.Wrist != null)
			{ w.Add("WRIST", player.Items.Wrist.ItemLevel); }
			else { w.Add("WRIST", "000"); }




			return w;
		}





		[Command("comparegear")]
		[Remarks("Compare the gear of multiple players from any guild.\n")]
		[Summary("EX: ripbot comparegear Ripgut Weedinator Sunscreen\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task CompareGear(params string[] playernames)
		{
			StringBuilder sb = new StringBuilder();
			string currentline = "";
			WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);


			Hashtable[] players = new Hashtable[playernames.Length];
			currentline = "Gear\t";
			DataAccess da = new DataAccess();
			bool ret = false;


			for (int i = 0; i < playernames.Length; i++)
			{
				Character currentplayer = null;

				try
				{
					currentplayer = explorer.GetCharacter(Globals.DEFAULTREALM, playernames[i], CharacterOptions.GetItems);
					currentline += playernames[i] + " " + currentplayer.Items.AverageItemLevel + "/" + currentplayer.Items.AverageItemLevelEquipped + "\t" + "|\t";
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					sb.AppendLine(ex.Message);

					if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						sb.AppendLine("Blizzard API service is down.");
					}

					if (ex.HResult == -2146233076)  // seems to happen on Wrobbinhuud
					{
						sb.AppendLine("Player " + playernames[i] + " Error deserializing object.");
					}

					sb.AppendLine("Player " + playernames[i] + " not found.");
					await ReplyAsync(sb.ToString());
					return;
				}

				players[i] = ParsePlayerGearForCompare(currentplayer);



				// see if the player exists
				ret = da.DoesPlayersExist(playernames[i]);
				if (!ret)
				{
					// if they dont, then get them and update cache
					try
					{
						da.UpdatePlayer(currentplayer);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
						if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
						{
							sb.AppendLine("Blizzard API service is down.");
						}

						if (ex.HResult == -2146233076)  // seems to happen on Wrobbinhuud
						{
							sb.AppendLine("Player " + playernames[i] + " Error deserializing object.");
						}

						sb.AppendLine("Player " + playernames[i] + " not found or could not be updated.");
						await ReplyAsync(sb.ToString());
						continue;
						//return;
					}
				}

				//// if they exist then use cache
				//DataRow dr = da.GetPlayer(playernames[i]);
				//currentline += dr["PlayerName"].ToString() + " " + dr["AverageItemLevel"].ToString() + "/" + dr["AverageItemLevelEquipped"] + "\t" + "|\t";
			}


			sb.AppendLine(currentline);
			sb.AppendLine();



			#region build each string

			currentline = BuildLine("BACK", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("CHEST", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("FEET", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("FINGER1", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("FINGER2", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("HANDS", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("HEAD", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("LEGS", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("MAINHAND", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("NECK", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("OFFHAND", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("RANGED", players);
			sb.AppendLine(currentline);

			//currentline = BuildLine("SHIRT", players);
			//sb.AppendLine(currentline);

			currentline = BuildLine("SHOULDER", players);
			sb.AppendLine(currentline);

			//currentline = BuildLine("TABARD", players);
			//sb.AppendLine(currentline);

			currentline = BuildLine("TRINKET1", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("TRINKET2", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("WAIST", players);
			sb.AppendLine(currentline);

			currentline = BuildLine("WRIST", players);
			sb.AppendLine(currentline);

			#endregion


			explorer = null;

			await ReplyAsync(sb.ToString());
		}

		private string BuildLine(string itemname, Hashtable[] players)
		{
			string tmp = "";
			//tmp = itemname.PadRight(8) + "\t";
			//tmp = itemname + "\t\t";
			tmp = itemname.PadRight(12);
			foreach (Hashtable h in players)
			{
				tmp += h[itemname].ToString() + "\t";
				//tmp += h[itemname].ToString() + "  ";
			}
			return tmp;
		}








		[Group("get"), Name("WoW")]
		public class Get : ModuleBase<SocketCommandContext>
		{
			[Command("guildinfo"), Alias("ggi")]
			[Remarks("Gets basic information about a guild.\n")]
			[Summary("EX: ripbot get guildinfo\nEX: ripbot get guildinfo Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task GuildInfo([Remainder]string optionalguildname = null)
			{
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Utility.DEFAULTREALM;

				//string realm = optionalrealmname != "" ? optionalrealmname : Utility.DEFAULTREALM;

				WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
				StringBuilder sb = new StringBuilder();

				Guild theguild = null;
				try
				{
					theguild = explorer.GetGuild(Globals.DEFAULTREALM, guildname, GuildOptions.GetEverything);
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
					if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						await ReplyAsync("Blizzard API service is down.\n");
					}
					await ReplyAsync(guildname + " guild not found.");
					return;
				}


				sb.AppendLine(String.Format("{0} is a level {1} {5} guild who plays on {4} with {2} members and {3} achievement points.",
					theguild.Name,
					theguild.Level.ToString(),
					theguild.Members.Count().ToString(),
					theguild.AchievementPoints.ToString(),
					theguild.Realm,
					theguild.Side.ToString()
					));

				//DataAccess da = new DataAccess();
				//da.UpdateGuildInfo(theguild);
				//int inserts = 0;
				//int updates = 0;
				//da.UpdateGuildMembers(theguild, out inserts, out updates);
				UpdateGuildInfo(theguild);
				//da = null;

				theguild = null;
				explorer = null;

				//txtResults.Text += sb.ToString() + "\r\n\r\n";

				await ReplyAsync(sb.ToString());
			}





			[Command("allstats"), Alias("gas")]
			[Remarks("Get all the wow stats for specified user.\n(Always refreshes the player in the database.)\n")]
			[Summary("EX: ripbot get allstats Ripgut\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task AllStats(string playername)
			{
				//string realm = optionalrealmname != null ? optionalrealmname : Utility.DEFAULTREALM;

				WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
				StringBuilder sb = new StringBuilder();

				Character player = null;
				try
				{
					//player = explorer.GetCharacter(realm, playername, CharacterOptions.GetStats | CharacterOptions.GetAchievements);
					player = explorer.GetCharacter(Globals.DEFAULTREALM, playername, CharacterOptions.GetEverything);
				}
				catch(Exception ex)
				{
					sb.AppendLine(ex.Message);

					if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						sb.AppendLine("Blizzard API service is down.");
					}

					if (ex.HResult == -2146233076)  // seems to happen on Wrobbinhuud
					{
						sb.AppendLine("Player " + playername + " Error deserializing object.");
					}

					sb.AppendLine("Player " + playername + " not found.");
					explorer = null;
					await ReplyAsync(sb.ToString());
					return;
				}





				DataAccess da = new DataAccess();
				// check if guild exists
				bool ret = false;
				ret = da.DoesGuildExist(player.Guild.Name);

				// if it doesn't and the player is in a guild, get it
				if (!ret && player.Guild != null)
				{
					Guild theguild = null;

					try
					{
						theguild = explorer.GetGuild(Globals.DEFAULTREALM, player.Guild.Name, GuildOptions.GetEverything);

						UpdateGuildInfo(theguild);
					}
					catch(Exception ex)
					{
						sb.AppendLine(ex.Message);

						if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
						{
							sb.AppendLine("Blizzard API service is down.");
						}

						//txtResults.Text = "Guild" + txtGuild.Text + " not found";
						//return;
					}

					theguild = null;
				}

				// upsert the player
				ret = da.UpdatePlayer(player);
				DateTime cachedate = da.GetPlayerCachedDateUnixReadableDate(player.Name);
				da = null;





				sb.AppendLine(String.Format("{0} is a level {1} {2} who has {3} achievement points having completed {4} achievements as of {5}",
					player.Name,
					player.Level,
					Utility.ToTitleCase(player.Class.ToString()),
					player.AchievementPoints,
					player.Achievements.AchievementsCompleted.Count(),
					cachedate.ToString()
					));

				sb.AppendLine();
				sb.AppendLine();

				foreach (KeyValuePair<string, object> stat in player.Stats)
				{
					sb.AppendLine(stat.Key + " : " + stat.Value);
				}

				explorer = null;
				player = null;


				await ReplyAsync(sb.ToString());
			}




			[Command("total110count"), Alias("g110t")]
			[Remarks("Gets the total number of level 110's for a guild.\n")]
			[Summary("EX: ripbot get total110count\nEX: ripbot get total110count Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task Total110Count([Remainder]string optionalguildname = null)
			{
				//string realm = optionalrealmname != "" ? optionalrealmname : Utility.DEFAULTREALM;
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Globals.DEFAULTREALM;


				//WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
				StringBuilder sb = new StringBuilder();

				DataAccess da = new DataAccess();
				int totalmembers = 0;
				int count = da.Get110Count(guildname, out totalmembers);
				da = null;

				sb.AppendLine(String.Format("There are {0} level 110 members out of {1} members in {2}.", count.ToString(), totalmembers.ToString(), guildname));

				await ReplyAsync(sb.ToString());
			}




			[Command("classtotals"), Alias("gct")]
			//[Remarks("Gets the class totals for a guild. \n__*Any guild name with spaces must be enclosed in quotes*__")]
			[Remarks("Gets the class totals for a guild.\n")]
			[Summary("EX: ripbot get classtotals\nEX: ripbot get classtotals Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task ClassTotals([Remainder]string optionalguildname = null)
			{
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Utility.DEFAULTREALM;


				StringBuilder sb = new StringBuilder();
				DataAccess da = new DataAccess();

				if (!da.DoesGuildExist(guildname))
				{
					await ReplyAsync("Guild not found in database.");
					da = null;
					return;
				}




				int totalmembers = 0;
				Hashtable TOTALS = da.GetClassTotals(guildname, out totalmembers);


				sb.AppendLine(String.Format("Class totals for {0} who has {1} members.", guildname, totalmembers.ToString()));
				sb.AppendLine();


				var allkeys = TOTALS.Keys;
				foreach (string currentclass in allkeys)
				{
					sb.AppendLine(String.Format("{0} = {1}",
						currentclass,
						TOTALS[currentclass]));
				}


				await ReplyAsync(sb.ToString());
			}




			[Command("raidready")]
			[Remarks("Gets a list of players in a guild who meet the specified iLvl.\n")]
			[Summary("EX: ripbot get raidready 840\nEX: ripbot get raidready 840 dps\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task RaidReady(string minimumaverageitemlevel, string optionalrole = null)
			{
				//string realm = optionalrealmname != "" ? optionalrealmname : Globals.DEFAULTREALM;
				//string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				string guildname = Globals.DEFAULTGUILDNAME;
				string role = optionalrole ?? "*";	// the asterik will mean all roles
				//string realm = optionalrealmname ?? Globals.DEFAULTREALM;

				//string showcacheddatetmp = optionalshowcacheddate ?? "false";
				//bool showcacheddate = bool.Parse(showcacheddatetmp);
				bool showcacheddate = true;

				StringBuilder sb = new StringBuilder();

				DataAccess da = new DataAccess();
				DataTable players = da.GetRaidReadyPlayers(guildname, minimumaverageitemlevel, role);
				da = null;



				//bool test = chkShowCachedDate.Checked;
				if (players != null)
				{
					string tmp = "";
					foreach (DataRow dr in players.Rows)
					{
						tmp = "";
						if (showcacheddate)
							tmp = "\tas of " + (Utility.ConvertUnixToLocalTime(long.Parse(dr["CachedDateUnix"].ToString()))).ToString();

						sb.AppendLine(String.Format("{0} / {1}  **{2}**   ({3}) {4}",
							dr["AverageItemLevel"].ToString(),
							dr["AverageItemLevelEquipped"].ToString(),
							dr["PlayerName"].ToString(),
							dr["SpecRole"].ToString(),
							tmp
							));

						// can't send over 2k in a message
						if (sb.Length > 1950)
						{
							// send what we have and clear the stringbuilder
							await ReplyAsync(sb.ToString());
							sb.Clear();
						}
					}
				}
				else
				{ sb.AppendLine("Guild not found."); }


				await ReplyAsync(sb.ToString());
			}




			[Command("undergeared110s")]
			[Remarks("Gets a list of level 110 players in a guild who are less than the specified iLvl.\n")]
			[Summary("EX: ripbot get undergeared110s 780\nEX: ripbot get undergeared110s 780 Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task UnderGeared110s(string averageitemlevel, [Remainder] string optionalguildname = null)
			{
				//string realm = optionalrealmname != "" ? optionalrealmname : Globals.DEFAULTREALM;
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Globals.DEFAULTREALM;
				//string realm = Globals.DEFAULTREALM;

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Players who are level 110 but below " + averageitemlevel + " average iLvl.\n");
				//sb.AppendLine();


				DataAccess da = new DataAccess();
				Hashtable players = da.GetUndergeared110Players(guildname, averageitemlevel);
				da = null;

				var allkeys = players.Keys;
				foreach (string currentilvl in allkeys)
				{
					sb.AppendLine(String.Format("{0} is {1}",
						currentilvl,
						players[currentilvl]));
					// can't send over 2k in a message
					if (sb.Length > 1980)
					{
						// send what we have
						await ReplyAsync(sb.ToString());
						sb.Clear();
					}
				}

				await ReplyAsync(sb.ToString());
			}




			[Command("inactiveplayers"), Alias("gip")]
			[Remarks("Gets the players who haven't been seen in x days.\n")]
			[Summary("EX: ripbot get inactiveplayers 90\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task InactivePlayers(int days)
			{
				StringBuilder sb = new StringBuilder();
				DataAccess da = new DataAccess();
				DataTable dt = da.GetInactivePlayers(days);
				da = null;

				int intrank = 0;
				Globals.GUILDRANK grr = Globals.GUILDRANK.Applicant;
				string playersrank = "";

				sb.AppendLine("There are " + dt.Rows.Count.ToString() + " players that haven't been seen in the last " + days + " days.");
				sb.AppendLine();
				foreach (DataRow dr in dt.Rows)
				{
					intrank = int.Parse(dr["GuildRank"].ToString());
					grr = (Globals.GUILDRANK) intrank;
					playersrank = grr.ToString();

					sb.AppendLine("**" + dr["PlayerName"].ToString() + "**\t(" + playersrank + ") last seen " + dr["LastModifiedReadable"].ToString());

					// can't send over 2k in a message
					if (sb.Length > 1950)
					{
						// send what we have and clear the stringbuilder
						await ReplyAsync(sb.ToString());
						sb.Clear();
					}
				}

				await ReplyAsync(sb.ToString());
			}


		}   // end public class Get








		///// <summary>
		///// Holds some info on a guild member.
		///// </summary>
		//[Serializable]
		//public struct PLAYERGUILDINFO
		//{
		//	/// <summary>
		//	/// Guild members name.
		//	/// </summary>
		//	public string NAME;
		//	/// <summary>
		//	/// Guild members spec.
		//	/// </summary>
		//	public string SPEC;
		//	/// <summary>
		//	/// Guild members class.
		//	/// </summary>
		//	public string CLASS;
		//	/// <summary>
		//	/// Guild members role (DPS, TANK, HEALER)
		//	/// </summary>
		//	public string ROLE;
		//	/// <summary>
		//	/// Guild members current level.
		//	/// </summary>
		//	public int LEVEL;
		//}


		///// <summary>
		///// Holds a PLAYERGUILDINFO struct for each guild member.
		///// </summary>
		//public static Hashtable PLAYERCACHE = new Hashtable();



		///// <summary>
		///// Caches some info about all guild members.
		///// </summary>
		///// <param name="guildname">The name of the guild to query.</param>
		///// <returns>True if successful, otherwise false.</returns>
		//private bool CacheGuildMembers(string guildname)
		//{
		//	WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Utility.MASHERYAPIKEY);
		//	Guild theguild = null;
		//	try
		//	{
		//		theguild = explorer.GetGuild("aerie peak", guildname, GuildOptions.GetMembers);
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.ToString());

		//		//txtResults.Text = "Guild not found";
		//		return false;
		//	}


		//	PLAYERCACHE.Clear();

		//	foreach (GuildMember guildmember in theguild.Members)
		//	{
		//		PLAYERGUILDINFO p = new PLAYERGUILDINFO();
		//		try
		//		{
		//			p.NAME = guildmember.Character.Name;
		//			p.SPEC = guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Name : "UNKNOWN";
		//			p.CLASS = guildmember.Character.Class.ToString();
		//			p.ROLE = guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Role : "UNKNOWN";
		//			p.LEVEL = guildmember.Character.Level;
		//		}
		//		catch (Exception eex)
		//		{
		//			Console.WriteLine(eex.ToString());
		//			continue;
		//		}

		//		PLAYERCACHE.Add(p.NAME.ToLower(), p);
		//	}

		//	theguild = null;

		//	return true;
		//}








		//[Command("tabletest"), Alias("tt")]
		//[Remarks("test")]
		//[Summary("ripbot tabletest")]
		//[MinPermissions(AccessLevel.User)]
		//public async Task TableTest()
		//{
		//	StringBuilder sb = new StringBuilder();

		//	sb.AppendLine("**First Header** | Second Header");
		//	sb.AppendLine("----- | -----");
		//	sb.AppendLine("Cell1 | Cell2");

		//	await ReplyAsync(sb.ToString());
		//}



	}   // end public class WoWModule
}