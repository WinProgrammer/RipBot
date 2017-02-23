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
using Discord;

namespace RipBot.Modules
{
	/// <summary>
	/// Handles all the WoW related commands.
	/// </summary>
	[Name("WoW")]
	[RequireContext(ContextType.Guild)]
	public class WoWModule : ModuleBase<SocketCommandContext>
	{
		//[Command("adminsay"), Alias("as")]
		//[Remarks("Make the bot echo something by a server admin.\n")]
		//[Summary("EX: ripbot adminsay Hello\n")]
		//[MinPermissions(AccessLevel.ServerAdmin)]
		//public async Task AdminSayCmd([Remainder]string text)
		//{
		//	await ReplyAsync(text);
		//}

		//[Command("modsay"), Alias("ms")]
		//[Remarks("Make the bot echo something by a server mod.\n")]
		//[Summary("EX: ripbot modsay Hello\n")]
		//[MinPermissions(AccessLevel.ServerMod)]
		//public async Task ModSayCmd([Remainder]string text)
		//{
		//	await ReplyAsync(text);
		//}





		/// <summary>
		/// Display all Hordecorp High Council members.\nThis command ONLY works for Hordecorp.
		/// </summary>
		/// <returns></returns>
		[Command("whoishighcouncil"), Alias("wihc")]
		[Remarks("Display all Hordecorp High Council members.\nThis command ONLY works for Hordecorp.\n")]
		[Summary("EX: ripbot whoishighcouncil\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task WhoIsHighCouncilCmd()
		{
			StringBuilder sb = new StringBuilder();
			DataAccess da = new DataAccess();
			List<string> hc = da.GetHordecorpHighCouncil();
			da.Dispose();
			da = null;

			hc.Sort();

			foreach (string player in hc)
			{
				sb.AppendLine(player + " is High Council");
			}


			await ReplyAsync(sb.ToString());
		}


		/// <summary>
		/// Display all Hordecorp officers.\nThis command ONLY works for Hordecorp.
		/// </summary>
		/// <returns></returns>
		[Command("whoareofficers"), Alias("wao")]
		[Remarks("Display all Hordecorp Officers.\nThis command ONLY works for Hordecorp.\n")]
		[Summary("EX: ripbot whoareofficers\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task WhoAreOfficersCmd()
		{
			StringBuilder sb = new StringBuilder();
			DataAccess da = new DataAccess();
			string working = "";


			EmbedBuilder embedofficers = new EmbedBuilder()
				.WithAuthor(new EmbedAuthorBuilder()
				.WithIconUrl(Context.Guild.IconUrl)
				.WithName("WhoAreOfficers"))
				.WithColor(new Color(0, 191, 255))
				//.WithThumbnailUrl(Context.Guild.IconUrl)
				.WithTitle("Gets a list of Hordecorp officers. (GM - Corporate)")
				//.WithDescription("```\nripbot comparegear " + playersdescrip + "```")
				.WithDescription("```\n" + "Players in **Bold** haven't been seen in the last 90 days." + "```")
				;


			List<string> gm = da.GetHordecorpGM();
			gm.Sort();
			working = BuildOfficerLine(gm);
			embedofficers.AddField(x =>
			{
				x.IsInline = true;
				x.Name = "__**GM**__";
				x.Value = working;
			});


			List<string> vp = da.GetHordecorpVPs();
			vp.Sort();
			working = BuildOfficerLine(vp);
			embedofficers.AddField(x =>
			{
				x.IsInline = true;
				x.Name = "__**VP**__";
				x.Value = working;
			});


			List<string> hc = da.GetHordecorpHighCouncil();
			hc.Sort();
			working = BuildOfficerLine(hc);
			embedofficers.AddField(x =>
			{
				x.IsInline = true;
				x.Name = "__**High Council**__";
				x.Value = working;
			});


			List<string> bw = da.GetHordecorpBigWigs();
			bw.Sort();
			working = BuildOfficerLine(bw);
			embedofficers.AddField(x =>
			{
				x.IsInline = true;
				x.Name = "__**Big Wig**__";
				x.Value = working;
			});


			List<string> cp = da.GetHordecorpCorporate();
			cp.Sort();
			working = BuildOfficerLine(cp);
			embedofficers.AddField(x =>
			{
				x.IsInline = true;
				x.Name = "__**Corporate**__";
				x.Value = working;
			});



			embedofficers.Build();

			da.Dispose();
			da = null;

			await ReplyAsync("", embed: embedofficers);
		}

		private string BuildOfficerLine(List<string> members)
		{
			string working = "";
			foreach (string player in members)
			{
				working += player + " -- ";
			}

			// remove the trailing " -- "
			working = working.Substring(0, working.Length - 4);

			return working;
		}




		/// <summary>
		/// Gets the gear for a player. (Updates the guild cache and the player)
		/// </summary>
		/// <param name="playername">The player to get the gear for.</param>
		/// <param name="optionalguildname">The optional guild name.</param>
		/// <returns></returns>
		[Command("getgear"), Alias("gg")]
		[Remarks("Gets the gear for a player.\n")]
		[Summary("EX: ripbot getgear Ripgut\nEX: ripbot getgear Ripgut Hordecorp\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task GetGearCmd(string playername, [Remainder] string optionalguildname = null)
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
			catch (Exception ex)
			{
				if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
				{
					//sb.AppendLine("Blizzard API service is down.");
					sb.AppendLine(ex.Message + "\n");
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
				catch (Exception eex)
				{
					sb.AppendLine(eex.Message);

					if (eex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						//sb.AppendLine("Blizzard API service is down.");
						sb.AppendLine(eex.Message + "\n");
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
		/// <param name="theguild">The guild to update.</param>
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

			da.Dispose();
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




		/// <summary>
		/// Compare the gear of multiple players from any guild. (Updates the players also)
		/// </summary>
		/// <param name="playernames">A space seperated list of players to compare.</param>
		/// <returns></returns>
		[Command("comparegear")]
		[Remarks("Compare the gear of multiple players from any guild.\n")]
		[Summary("EX: ripbot comparegear Ripgut Weedinator Sunscreen\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task CompareGearCmd(params string[] playernames)
		{
			StringBuilder sb = new StringBuilder();
			WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);


			Hashtable[] players = new Hashtable[playernames.Length];
			string ilvlsforembed = "";
			DataAccess da = new DataAccess();
			bool ret = false;


			for (int i = 0; i < playernames.Length; i++)
			{
				Character currentplayer = null;

				try
				{
					currentplayer = explorer.GetCharacter(Globals.DEFAULTREALM, playernames[i], CharacterOptions.GetEverything);
					ilvlsforembed += playernames[i] + " " + currentplayer.Items.AverageItemLevel + "/" + currentplayer.Items.AverageItemLevelEquipped + " -- ";
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					sb.AppendLine(ex.Message);

					if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						sb.AppendLine(ex.Message + "\n");
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
							sb.AppendLine(ex.Message + "\n");
						}

						if (ex.HResult == -2146233076)  // seems to happen on Wrobbinhuud
						{
							sb.AppendLine("Player " + playernames[i] + " Error deserializing object.");
						}

						sb.AppendLine("Player " + playernames[i] + " not found or could not be updated.");
						await ReplyAsync(sb.ToString());
						continue;
					}
				}
			}

			ilvlsforembed = ilvlsforembed.Substring(0, ilvlsforembed.Length - 4);
			sb.AppendLine();



			#region build each string

			// get the playernames to use in our embed Description
			string playersdescrip = "";
			for (int i = 0; i < playernames.Length; i++)
			{
				playersdescrip += playernames[i] + " ";
			}

			EmbedBuilder embedgear = new EmbedBuilder()
				.WithAuthor(new EmbedAuthorBuilder()
				.WithIconUrl(Context.Guild.IconUrl)
				.WithName("CompareGear"))
				.WithColor(new Color(0, 191, 255))
				//.WithThumbnailUrl(Context.Guild.IconUrl)
				.WithTitle("Compare the gear of multiple players.")
				//.WithDescription("```\nripbot comparegear " + playersdescrip + "```")
				.WithDescription("```\n" + ilvlsforembed + "```")
				;


			bool buildembedok = false;


			buildembedok = BuildGearEmbed("HEAD", players, ref embedgear);
			buildembedok = BuildGearEmbed("HANDS", players, ref embedgear);
			buildembedok = BuildGearEmbed("Neck", players, ref embedgear);
			buildembedok = BuildGearEmbed("Waist", players, ref embedgear);
			buildembedok = BuildGearEmbed("Shoulder", players, ref embedgear);
			buildembedok = BuildGearEmbed("Legs", players, ref embedgear);
			buildembedok = BuildGearEmbed("Back", players, ref embedgear);
			buildembedok = BuildGearEmbed("Feet", players, ref embedgear);
			buildembedok = BuildGearEmbed("Chest", players, ref embedgear);
			buildembedok = BuildGearEmbed("Finger1", players, ref embedgear);
			buildembedok = BuildGearEmbed("RANGED", players, ref embedgear);
			//buildembedok = BuildGearEmbed("SHIRT", players, ref embedgear);
			buildembedok = BuildGearEmbed("Finger2", players, ref embedgear);
			buildembedok = BuildGearEmbed("TABARD", players, ref embedgear);
			buildembedok = BuildGearEmbed("TRINKET1", players, ref embedgear);
			buildembedok = BuildGearEmbed("Wrist", players, ref embedgear);
			buildembedok = BuildGearEmbed("Trinket2", players, ref embedgear);
			buildembedok = BuildGearEmbed("MAINHAND", players, ref embedgear);
			buildembedok = BuildGearEmbed("OFFHAND", players, ref embedgear);


			embedgear.Build();

			#endregion


			explorer = null;

			sb.AppendLine();
			sb.AppendLine();

			await ReplyAsync("", embed: embedgear);
		}

		private string BuildLine(string itemname, Hashtable[] players)
		{
			itemname = itemname.ToUpper();

			string tmp = "";
			tmp = itemname.PadRight(12);
			try
			{
				foreach (Hashtable h in players)
				{
					tmp += h[itemname].ToString() + "\t";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return tmp;
		}


		private bool BuildGearEmbed(string itemname, Hashtable[] players, ref EmbedBuilder emb)
		{
			itemname = itemname.ToUpper();

			string embvalue = "";

			string tmp = "";

			try
			{
				foreach (Hashtable h in players)
				{
					try
					{
						tmp += h[itemname].ToString() + " -- ";
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}

			// remove the trailing " -- "
			embvalue = tmp.Substring(0, tmp.Length - 4);

			try
			{
				emb.AddField(x =>
				{
					x.IsInline = true;
					x.Name = itemname;
					x.Value = embvalue;
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}

			return true;
		}



		/// <summary>
		/// Gets the profession totals for a guild from the cache.
		/// </summary>
		/// <param name="optionalguildname">The optional guild name to get the class totals for.</param>
		/// <returns></returns>
		[Command("getprofessiontotals"), Alias("gct")]
		//[Remarks("Gets the profession totals for a guild.")]
		[Remarks("Gets the professiontotals totals for a guild.\n")]
		[Summary("EX: ripbot getprofessiontotals\nEX: ripbot getprofessiontotals Hordecorp\n")]
		[MinPermissions(AccessLevel.User)]
		public async Task ClassProfessionTotalsCmd([Remainder]string optionalguildname = null)
		{
			string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
			//string realm = optionalrealmname ?? Utility.DEFAULTREALM;



			StringBuilder sb = new StringBuilder();
			DataAccess da = new DataAccess();

			if (!da.DoesGuildExist(guildname))
			{
				await ReplyAsync("Guild not found in database.");
				da.Dispose();
				da = null;
				return;
			}


			int totalmembers = 0;
			Hashtable TOTALS = da.GetProfessionTotals(guildname, out totalmembers);

			EmbedBuilder embedclasses = new EmbedBuilder()
			.WithAuthor(new EmbedAuthorBuilder()
			.WithIconUrl(Context.Guild.IconUrl)
			.WithName("get ProfessionTotals"))
			.WithColor(new Color(0, 191, 255))
			//.WithThumbnailUrl(Context.Guild.IconUrl)
			.WithTitle("Gets the profession totals.")
			.WithDescription("```\n" + String.Format("Profession totals for {0} who has {1} members.", guildname, totalmembers.ToString()) +
				"\nThe UNKNOWN profession means that a player hasn't picked either a primary, secondary or both main professions" + "```")
			;


			var allkeys = TOTALS.Keys;
			foreach (string currentprofession in allkeys)
			{
				embedclasses.AddField(x =>
				{
					x.IsInline = true;
					x.Name = "__**" + currentprofession + "**__";
					x.Value = TOTALS[currentprofession].ToString();
				});
			}

			embedclasses.Build();

			da.Dispose();
			da = null;

			await ReplyAsync("", embed: embedclasses);
		}




		/// <summary>
		/// 
		/// </summary>
		[Group("get"), Name("WoW")]
		public class Get : ModuleBase<SocketCommandContext>
		{
			/// <summary>
			/// Gets basic information about a guild. (Updates guild cache also)
			/// </summary>
			/// <param name="optionalguildname">The optional guild name to get the info on.</param>
			/// <returns></returns>
			[Command("guildinfo"), Alias("ggi")]
			[Remarks("Gets basic information about a guild.\n")]
			[Summary("EX: ripbot get guildinfo\nEX: ripbot get guildinfo Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task GuildInfoCmd([Remainder]string optionalguildname = null)
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
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						//await ReplyAsync("Blizzard API service is down.\n");
						await ReplyAsync(ex.Message + "\n");
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
				//da.Dispose();
				//da = null;

				theguild = null;
				explorer = null;

				//txtResults.Text += sb.ToString() + "\r\n\r\n";

				await ReplyAsync(sb.ToString());
			}




			/// <summary>
			/// Get all the wow stats for specified user. (Updates guild cache and always refreshes the player in the database.)
			/// </summary>
			/// <param name="playername">The name of the player to get the stats for.</param>
			/// <returns></returns>
			[Command("allstats"), Alias("gas")]
			[Remarks("Get all the wow stats for specified user.\n(Always refreshes the player in the database.)\n")]
			[Summary("EX: ripbot get allstats Ripgut\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task AllStatsCmd(string playername)
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
				catch (Exception ex)
				{
					sb.AppendLine(ex.Message);

					if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
					{
						//sb.AppendLine("Blizzard API service is down.");
						sb.AppendLine(ex.Message + "\n");
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
					catch (Exception ex)
					{
						sb.AppendLine(ex.Message);

						if (ex.HResult == -2146233079)  // "The remote server returned an error: (503) Server Unavailable."
						{
							//sb.AppendLine("Blizzard API service is down.");
							sb.AppendLine(ex.Message + "\n");
						}

						//txtResults.Text = "Guild" + txtGuild.Text + " not found";
						//return;
					}

					theguild = null;
				}

				// upsert the player
				ret = da.UpdatePlayer(player);
				DateTime cachedate = da.GetPlayerCachedDateUnixReadableDate(player.Name);
				da.Dispose();
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



			/// <summary>
			/// Gets the total number of level 110's for a guild from the cache.
			/// </summary>
			/// <param name="optionalguildname"></param>
			/// <returns></returns>
			[Command("total110count"), Alias("g110t")]
			[Remarks("Gets the total number of level 110's for a guild.\n")]
			[Summary("EX: ripbot get total110count\nEX: ripbot get total110count Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task Total110CountCmd([Remainder]string optionalguildname = null)
			{
				//string realm = optionalrealmname != "" ? optionalrealmname : Utility.DEFAULTREALM;
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Globals.DEFAULTREALM;


				//WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, Globals.MASHERYAPIKEY);
				StringBuilder sb = new StringBuilder();

				DataAccess da = new DataAccess();
				int totalmembers = 0;
				int count = da.Get110Count(guildname, out totalmembers);
				da.Dispose();
				da = null;

				sb.AppendLine(String.Format("There are {0} level 110 members out of {1} members in {2}.", count.ToString(), totalmembers.ToString(), guildname));

				await ReplyAsync(sb.ToString());
			}




			/// <summary>
			/// Gets the class totals for a guild from the cache.
			/// </summary>
			/// <param name="optionalguildname">The optional guild name to get the class totals for.</param>
			/// <returns></returns>
			[Command("classtotals"), Alias("gct")]
			//[Remarks("Gets the class totals for a guild.")]
			[Remarks("Gets the class totals for a guild.\n")]
			[Summary("EX: ripbot get classtotals\nEX: ripbot get classtotals Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task ClassTotalsCmd([Remainder]string optionalguildname = null)
			{
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Utility.DEFAULTREALM;



				StringBuilder sb = new StringBuilder();
				DataAccess da = new DataAccess();

				if (!da.DoesGuildExist(guildname))
				{
					await ReplyAsync("Guild not found in database.");
					da.Dispose();
					da = null;
					return;
				}


				int totalmembers = 0;
				Hashtable TOTALS = da.GetClassTotals(guildname, out totalmembers);

				EmbedBuilder embedclasses = new EmbedBuilder()
				.WithAuthor(new EmbedAuthorBuilder()
				.WithIconUrl(Context.Guild.IconUrl)
				.WithName("get ClassTotals"))
				.WithColor(new Color(0, 191, 255))
				//.WithThumbnailUrl(Context.Guild.IconUrl)
				.WithTitle("Gets the class totals.")
				.WithDescription("```\n" + String.Format("Class totals for {0} who has {1} members.", guildname, totalmembers.ToString()) + "```")
				;


				var allkeys = TOTALS.Keys;
				foreach (string currentclass in allkeys)
				{
					embedclasses.AddField(x =>
					{
						x.IsInline = true;
						x.Name = "__**" + currentclass + "**__";
						x.Value = TOTALS[currentclass].ToString();
					});
				}

				embedclasses.Build();

				da.Dispose();
				da = null;

				await ReplyAsync("", embed: embedclasses);
			}





			/// <summary>
			/// Gets a list of players in a guild who meet the specified iLvl from the cache.
			/// </summary>
			/// <param name="minimumaverageitemlevel">The minimum iLvl the player must have to be included.</param>
			/// <param name="optionalrole">Optionally narrow the list down by roles (DPS, HEALING, TANK).</param>
			/// <returns></returns>
			[Command("raidready")]
			[Remarks("Gets a list of players in a guild who meet the specified iLvl.\n")]
			[Summary("EX: ripbot get raidready 840\nEX: ripbot get raidready 840 dps\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task RaidReadyCmd(string minimumaverageitemlevel, string optionalrole = null)
			{
				//string realm = optionalrealmname != "" ? optionalrealmname : Globals.DEFAULTREALM;
				//string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				string guildname = Globals.DEFAULTGUILDNAME;
				string role = optionalrole ?? "*";  // the asterik will mean all roles
													//string realm = optionalrealmname ?? Globals.DEFAULTREALM;

				//string showcacheddatetmp = optionalshowcacheddate ?? "false";
				//bool showcacheddate = bool.Parse(showcacheddatetmp);
				bool showcacheddate = true;

				StringBuilder sb = new StringBuilder();

				DataAccess da = new DataAccess();
				DataTable players = da.GetRaidReadyPlayers(guildname, minimumaverageitemlevel, role);
				da.Dispose();
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




			/// <summary>
			/// Gets a list of level 110 players in a guild who are less than the specified iLvl from the cache.
			/// </summary>
			/// <param name="averageitemlevel">The minimum iLvl the player must have to be included.</param>
			/// <param name="optionalguildname">The optional guild name to get the class totals for.</param>
			/// <returns></returns>
			[Command("undergeared110s")]
			[Remarks("Gets a list of level 110 players in a guild who are less than the specified iLvl.\n")]
			[Summary("EX: ripbot get undergeared110s 780\nEX: ripbot get undergeared110s 780 Hordecorp\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task UnderGeared110sCmd(string averageitemlevel, [Remainder] string optionalguildname = null)
			{
				//string realm = optionalrealmname != "" ? optionalrealmname : Globals.DEFAULTREALM;
				string guildname = optionalguildname ?? Globals.DEFAULTGUILDNAME;
				//string realm = optionalrealmname ?? Globals.DEFAULTREALM;
				//string realm = Globals.DEFAULTREALM;


				StringBuilder sb = new StringBuilder();

				DataAccess da = new DataAccess();
				DataTable players = da.GetUndergeared110Players(guildname, averageitemlevel);
				da.Dispose();
				da = null;

				DateTime now = DateTime.Now;
				// get todays unix time
				long today = Utility.ConvertLocalTimeToUnix(now);
				// subtract 90 days from now
				TimeSpan ts = new TimeSpan(90, 0, 0, 0);
				DateTime prev = now.Subtract(ts);
				// convert that date to unix time
				long span = Utility.ConvertLocalTimeToUnix(prev);


				sb.AppendLine("Players who are level 110 but below " + averageitemlevel + " average iLvl.");
				sb.AppendLine("Players in **Bold** haven't been seen in the last 90 days.\n");
				sb.AppendLine();

				string playername = "";
				long lastmodified = 0;
				foreach (DataRow dr in players.Rows)
				{
					// 0 means we haven't cached that particular player yet, so move to next record
					if (dr["AverageItemLevel"].ToString() == "0") continue;

					lastmodified = long.Parse(dr["LastModified"].ToString());
					if (lastmodified <= span & lastmodified != 0)
					{
						// inactive
						playername = "**" + dr["PlayerName"].ToString() + "**";
					}
					else
					{
						playername = dr["PlayerName"].ToString();
					}

					sb.AppendLine(String.Format("{0} has {1}\t\tLast seen {2}",
						playername,
						dr["AverageItemLevel"].ToString(),
						dr["LastModifiedReadable"].ToString()));

					// can't send over 2k in a message
					if (sb.Length > 1900)
					{
						// send what we have
						await ReplyAsync(sb.ToString());
						sb.Clear();
					}
				}

				await ReplyAsync(sb.ToString());
			}




			/// <summary>
			/// Gets the players who haven't been seen in x days from the cache.
			/// </summary>
			/// <param name="days">The number of days to go back.</param>
			/// <returns></returns>
			[Command("inactiveplayers"), Alias("gip")]
			[Remarks("Gets the players who haven't been seen in x days.\n")]
			[Summary("EX: ripbot get inactiveplayers 90\n")]
			[MinPermissions(AccessLevel.User)]
			public async Task InactivePlayersCmd(int days)
			{
				StringBuilder sb = new StringBuilder();
				DataAccess da = new DataAccess();
				DataTable dt = da.GetInactivePlayers(days);
				da.Dispose();
				da = null;

				int intrank = 0;
				Globals.GUILDRANK grr = Globals.GUILDRANK.Applicant;
				string playersrank = "";

				sb.AppendLine("There are " + dt.Rows.Count.ToString() + " players that haven't been seen in the last " + days + " days.");
				sb.AppendLine();
				foreach (DataRow dr in dt.Rows)
				{
					intrank = int.Parse(dr["GuildRank"].ToString());
					grr = (Globals.GUILDRANK)intrank;
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
