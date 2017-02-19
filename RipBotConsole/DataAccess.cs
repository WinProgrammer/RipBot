using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using System.IO;


using WowDotNetAPI;
using WowDotNetAPI.Models;
using System.Collections;

namespace RipBot
{
	/// <summary>
	/// Handles all database access.
	/// </summary>
	internal class DataAccess : IDisposable
	{
		private string _instanceCnnstring = string.Empty;
		private SQLiteConnection _cnn = null;
		private bool _isdbopen = false;

		public event EventHandler GuildInfoUpdated;


		/// <summary>
		/// CStor
		/// </summary>
		public DataAccess()
		{
			// create the connection string
			_instanceCnnstring = @"Data Source=" + Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "\\WoW.db3;Version=3;";
		}



		/// <summary>
		/// Opens the connection to the database if not already open.
		/// </summary>
		/// <returns>True if successful, otherwise False.</returns>
		public bool Open()
		{
			if (_isdbopen)
				return true;

			_cnn = new SQLiteConnection(_instanceCnnstring);
			try
			{
				_cnn.Open();
				_isdbopen = true;
			}
			catch
			{
				_isdbopen = false;
			}

			return _isdbopen;
		}
		/// <summary>
		/// Closes and NULLs the databse connection.
		/// </summary>
		/// <returns>True if successful, otherwise False.</returns>
		public bool Close()
		{
			if (_cnn != null)
			{
				try
				{
					_cnn.Close();
					_cnn.Dispose();
					_cnn = null;
				}
				catch
				{
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Takes care of any apostrophes in a field.
		/// </summary>
		/// <param name="field"></param>
		/// <returns>A SQL friendly field.</returns>
		private string FixField(string field)
		{
			if (field != null)
				field = field.Replace("'", "''");
			else
				field = string.Empty;

			return field;
		}



		/// <summary>
		/// Executes an ExecuteScalar() from a sql query.
		/// </summary>
		/// <param name="sql">The sql script to execute. IE: SELECT Count('PlayerName') FROM PLAYERS</param>
		/// <returns>A single value as a String.</returns>
		public string GetFieldValue(string sql)
		{
			if (!_isdbopen)
				this.Open();

			object ret = null;

			try
			{
				SQLiteCommand cmd = new SQLiteCommand(sql, _cnn);
				ret = cmd.ExecuteScalar();
			}
			catch (SQLiteException se)
			{
				Console.WriteLine(se.Message);
				//Debug.Fail(se.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				//Debug.Fail(ex.Message);
			}

			if (null == ret)
				ret = "";

			return ret.ToString();
		}



		/// <summary>
		/// Handles the actual retrieval of a table from the database.
		/// </summary>
		/// <param name="sql">The query to issue.</param>
		/// <param name="tablename">The name of the returning table.</param>
		/// <returns></returns>
		public DataTable GetTable(string sql, string tablename)
		{
			if (!_isdbopen)
				this.Open();

			DataTable dt = new DataTable(tablename);

			SQLiteDataAdapter da = new SQLiteDataAdapter(sql, _cnn);
			try
			{
				da.Fill(dt);
			}
			catch (SQLiteException se)
			{
				Console.WriteLine(se.Message);
				//Debug.Fail(se.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				//Debug.Fail(ex.Message);
			}

			return dt;
		}


		/// <summary>
		/// Gets a player from the cache.
		/// </summary>
		/// <param name="playername">The player to get.</param>
		/// <returns>A DataRow containing the players info.</returns>
		public DataRow GetPlayer(string playername)
		{
			DataTable dt = new DataTable();

			string qry = "SELECT * FROM PLAYERS WHERE Playername = '" + playername + "'";
			dt = GetTable(qry, "Players");

			DataRow dr = null;

			if (dt != null & dt.Rows.Count > 0)
			{
				dr = dt.Rows[0];
			}

			return dr;
		}



		/// <summary>
		/// Gets a list of players for a specific guild.
		/// </summary>
		/// <param name="guildname">The guild to check.</param>
		/// <returns>A list of the guilds players.</returns>
		public List<string> GetPlayers(string guildname)
		{
			List<string> players = new List<string>();


			DataTable dt = new DataTable();

			string qry = "SELECT Playername FROM PLAYERS WHERE GuildName = '" + guildname + "'";
			dt = GetTable(qry, "Players");

			foreach (DataRow dr in dt.Rows)
			{
				players.Add(dr["PlayerName"].ToString());
			}

			return players;
		}



		/// <summary>
		/// Removes a List of players from the PLAYERS table.
		/// </summary>
		/// <param name="playerstoremove">The list of players to remove.</param>
		/// <returns>True if successful, otherwise False.</returns>
		public int PurgePlayers(List<string> playerstoremove)
		{
			int ret = 0;

			if (!_isdbopen)
				this.Open();

			object qryret = null;
			SQLiteCommand cmd = new SQLiteCommand();

			foreach (string playername in playerstoremove)
			{
				try
				{
					cmd = new SQLiteCommand("DELETE FROM PLAYERS WHERE PlayerName = '" + playername + "'", _cnn);
					qryret = cmd.ExecuteScalar();
					ret++;
				}
				catch (SQLiteException se)
				{
					Console.WriteLine(se.Message);
					//Debug.Fail(se.Message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					//Debug.Fail(ex.Message);
				}

				//if (null == qryret)
				//	qryret = "";
			}

			return ret;
		}


		/// <summary>
		/// Gets a list of inactive players.
		/// </summary>
		/// <param name="days">The number of days back to check for.</param>
		/// <returns>A DataTable with the inactive players and the last seen date.</returns>
		public DataTable GetInactivePlayers(int days)
		{
			DataTable dt = new DataTable();
			DateTime now = DateTime.Now;

			// get todays unix time
			long today = Utility.ConvertLocalTimeToUnix(now);

			// subtract the number of days from now
			TimeSpan ts = new TimeSpan(days, 0, 0, 0);
			DateTime prev = now.Subtract(ts);

			// convert that date to unix time
			long span = Utility.ConvertLocalTimeToUnix(prev);

			// get the data
			dt = GetTable("SELECT PlayerName, LastModifiedReadable, GuildRank FROM PLAYERS WHERE LastModified <= " + span.ToString() + " AND LastModified != 0 AND GuildName = '" + Globals.DEFAULTGUILDNAME + "' ORDER BY LastModified", "INACTIVEPLAYERS");


			return dt;
		}




		/// <summary>
		/// Gets a readable date from the wow Guild LastModified date, which in unix time.
		/// </summary>
		/// <param name="guildname">The name of the guild to get the date for.</param>
		/// <returns>A readable/usable date, or 1-1-1970 if there was an error.</returns>
		public DateTime GetGuildLastModifiedReadableDate(string guildname)
		{
			DateTime dt = DateTime.Now;

			string dts = GetFieldValue("SELECT LastModifiedReadable FROM GUILDS WHERE GuildName = '" + guildname + "'");
			try
			{
				dt = DateTime.Parse(dts);
			}
			catch
			{
				dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			}

			return dt;
		}

		/// <summary>
		/// Gets the date the record was cached in the GUILDS table.
		/// </summary>
		/// <param name="guildname">The player to look up.</param>
		/// <returns>The date the record was cached, or 1-1-1970 if there was an error.</returns>
		public DateTime GetGuildCachedDateUnixReadable(string guildname)
		{
			DateTime dt = DateTime.Now;

			string dts = GetFieldValue("SELECT CachedDateUnix FROM GUILDS WHERE GuildName = '" + guildname + "'");
			try
			{
				dt = Utility.ConvertUnixToLocalTime(long.Parse(dts));
			}
			catch
			{
				dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			}

			return dt;
		}


		/// <summary>
		/// Gets a readable date from the wow PLAYER LastModified date, which in unix time.
		/// </summary>
		/// <param name="playername">The name of the guild to get the date for.</param>
		/// <returns>A readable/usable date, or 1-1-1970 if there was an error.</returns>
		public DateTime GetPlayerLastModifiedReadableDate(string playername)
		{
			DateTime dt = DateTime.Now;

			string dts = GetFieldValue("SELECT LastModifiedReadable FROM PLAYERS WHERE PlayerName = '" + playername + "'");
			try
			{
				dt = DateTime.Parse(dts);
			}
			catch
			{
				dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			}

			return dt;
		}

		/// <summary>
		/// Gets the date the record was cached in the PLAYERS table.
		/// </summary>
		/// <param name="playername">The player to look up.</param>
		/// <returns>The date the record was cached, or 1-1-1970 if there was an error.</returns>
		public DateTime GetPlayerCachedDateUnixReadableDate(string playername)
		{
			DateTime dt = DateTime.Now;

			string dts = GetFieldValue("SELECT CachedDateUnix FROM PLAYERS WHERE PlayerName = '" + playername + "'");
			try
			{
				dt = Utility.ConvertUnixToLocalTime(long.Parse(dts));
			}
			catch
			{
				dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			}

			return dt;
		}

		/// <summary>
		/// Check the GUILDS table to see if a Guild exists.
		/// </summary>
		/// <param name="guildname">Name of the guild to check for.</param>
		/// <returns>True if it exists, otherwise False.</returns>
		public bool DoesGuildExist(string guildname)
		{
			string dts = GetFieldValue("SELECT GuildName FROM GUILDS WHERE GuildName = '" + guildname + "'");

			return !String.IsNullOrEmpty(dts);
		}


		/// <summary>
		/// Check the PLAYERS table to see if a player exists.
		/// </summary>
		/// <param name="playername">Name of the player to check for.</param>
		/// <returns>True if it exists, otherwise False.</returns>
		public bool DoesPlayersExist(string playername)
		{
			string dts = GetFieldValue("SELECT PlayerName FROM PLAYERS WHERE PlayerName = '" + playername + "'");

			return !String.IsNullOrEmpty(dts);
		}


		/// <summary>
		/// (HORDECORP SPECIFIC) Gets a list of all the high council members from the database.
		/// </summary>
		/// <returns>A list of Hordecorp high council members.</returns>
		public List<string> GetHighCouncilMembers()
		{
			List<string> hc = new List<string>();

			DataTable dt = GetTable("SELECT PlayerName FROM PLAYERS WHERE GuildRank = '2' AND GuildName = 'Hordecorp'", "HighCouncil");

			foreach (DataRow dr in dt.Rows)
			{
				hc.Add(dr["PlayerName"].ToString());
			}

			return hc;
		}


		/// <summary>
		/// Gets the total number of level 110 players in a guild.
		/// </summary>
		/// <param name="guildname">The guild to check</param>
		/// <param name="totalmembers">(out) the total number of players in the guild.</param>
		/// <returns>The total number of level 110's in a guild.</returns>
		public int Get110Count(string guildname, out int totalmembers)
		{
			string tm = GetFieldValue("SELECT COUNT(PlayerName) FROM PLAYERS WHERE GuildName = '" + guildname + "'");
			totalmembers = int.Parse(tm);

			string cnt = GetFieldValue("SELECT COUNT(PlayerName) FROM PLAYERS WHERE Level = '110' AND GuildName = '" + guildname + "'");

			return int.Parse(cnt);
		}


		/// <summary>
		/// Gets the total for each class in a guild.
		/// </summary>
		/// <param name="guildname">The guild to check.</param>
		/// <param name="totalmembers">(out) the total number of players in the guild.</param>
		/// <returns></returns>
		public Hashtable GetClassTotals(string guildname, out int totalmembers)
		{
			string tm = GetFieldValue("SELECT COUNT(PlayerName) FROM PLAYERS WHERE GuildName = '" + guildname + "'");
			totalmembers = int.Parse(tm);

			Hashtable TOTALS = new Hashtable();

			string baseqry = "SELECT COUNT(Class) FROM PLAYERS WHERE Guildname = '" + guildname + "' AND Class = '{0}'";
			string qry = "";

			qry = string.Format(baseqry, "Death_Knight");
			TOTALS.Add("DEATH_KNIGHT", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Demonhunter");
			TOTALS.Add("DEMONHUNTER", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Druid");
			TOTALS.Add("DRUID", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Hunter");
			TOTALS.Add("HUNTER", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Mage");
			TOTALS.Add("MAGE", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Monk");
			TOTALS.Add("MONK", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Paladin");
			TOTALS.Add("PALADIN", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Priest");
			TOTALS.Add("PRIEST", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Rogue");
			TOTALS.Add("ROGUE", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Shaman");
			TOTALS.Add("SHAMAN", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Warlock");
			TOTALS.Add("WARLOCK", int.Parse(GetFieldValue(qry)));
			qry = string.Format(baseqry, "Warrior");
			TOTALS.Add("WARRIOR", int.Parse(GetFieldValue(qry)));

			return TOTALS;
		}


		/// <summary>
		/// Gets all the level 110+ players who have equal or greater ilvl's than the argument.
		/// </summary>
		/// <param name="guildname">The guild to check.</param>
		/// <param name="ilvl">The minimum average ilvl to search for.</param>
		/// <returns>A HAshtable containing the players and thier average ilvl.</returns>
		public DataTable GetRaidReadyPlayers(string guildname, string ilvl, string role)
		{
			role = role.ToUpper();

			DataTable players = null;

			// make sure the guild exists
			if (!DoesGuildExist(guildname)) return players;

			string qry = "SELECT PlayerName, AverageItemLevel, AverageItemLevelEquipped, SpecRole, CachedDateUnix FROM PLAYERS WHERE Guildname = '" + guildname + "' AND AverageItemLevel >= " + ilvl;
			if (role != "*")
			{
				// see if its HEALING
				if (role.Contains("HEAL"))
					role = "HEALING";

				qry += " AND SpecRole = '" + role.ToUpper() + "'";
			}
			qry += " ORDER BY AverageItemLevel DESC, PlayerName";
			players = GetTable(qry, "RaidReadyPlayers");


			return players;
		}


		/// <summary>
		/// Gets all the level 110+ players who have equal or less than ilvl's than the argument.
		/// </summary>
		/// <param name="guildname">The guild to check.</param>
		/// <param name="ilvl">The minimum average ilvl to search for.</param>
		/// <returns>A DataTable containing the players, thier average ilvl and Last seen date.</returns>
		public DataTable GetUndergeared110Players(string guildname, string ilvl)
		{
			string qry = "SELECT PlayerName, AverageItemLevel, LastModifiedReadable FROM PLAYERS WHERE Guildname = '" + guildname + "' AND AverageItemLevel <= " + ilvl + " AND Level >= 110 ORDER BY AverageItemLevel DESC";

			DataTable dt = GetTable(qry, "UndergearedPlayers");

			return dt;
		}



		#region Update Guild

		/// <summary>
		/// Upserts the guilds info into the GUILDS table.
		/// </summary>
		/// <param name="theguild">A WoWDotNetAPI.Modules.Guild object.</param>
		/// <returns>The number of rows affected, or -1 on failure.</returns>
		public int UpdateGuildInfo(Guild theguild)
		{
			if (!_isdbopen)
				this.Open();

			SQLiteCommand cmd = null;
			int numrows = 0;
			bool ret = false;
			string sql = "";

			ret = DoesGuildExist(theguild.Name);
			if (ret)
			{
				// guild exists so build update query
				sql = BuildGuildInfoUpdateSQL(theguild);
			}
			else
			{
				// guild does not exists so build insert query
				sql = BuildGuildInfoInsertSQL(theguild);
			}

			cmd = new SQLiteCommand(sql, _cnn);
			try
			{
				numrows += cmd.ExecuteNonQuery();
				//Debug.Assert(numrows > 0);

				if (GuildInfoUpdated != null)
				{
					// someone is subscribed, throw event
					GuildInfoUpdated(this, new EventArgs());
				}
			}
			catch (SQLiteException se)
			{
				Console.WriteLine(se.Message);
				return -1;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return -1;
			}

			return numrows;
		}

		/// <summary>
		/// Build an INSERT query for the GUILDS table.
		/// </summary>
		/// <param name="theguild">A WoWDotNetAPI.Modules.Guild object.</param>
		/// <returns>An INSERT query.</returns>
		private string BuildGuildInfoInsertSQL(Guild theguild)
		{
			//UPDATE t SET a = 'pdf' WHERE id = 2;
			//INSERT INTO t(id, a) SELECT 2, 'pdf' WHERE changes() = 0;


			StringBuilder sb = new StringBuilder();

			sb.Append("INSERT INTO GUILDS (GuildID, GuildName, Level, MemberCount, Realm, AchievementPoints, LastModified, LastModifiedReadable, Side, CachedDateUnix) VALUES (");
			sb.Append(String.Format("{0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}'",
				"null",
				theguild.Name,
				theguild.Level.ToString(),
				theguild.Members.Count().ToString(),
				theguild.Realm,
				theguild.AchievementPoints.ToString(),
				theguild.LastModified.ToString(),
				Utility.ConvertUnixToLocalTime(theguild.LastModified),
				theguild.Side.ToString(),
				Utility.ConvertLocalTimeToUnix(DateTime.Now)
				));
			sb.Append(")");


			string sql = sb.ToString();
			return sql;
		}

		/// <summary>
		/// Build an UPDATE query for the GUILDS table.
		/// </summary>
		/// <param name="theguild">A WoWDotNetAPI.Modules.Guild object.</param>
		/// <returns>An UPDATE query.</returns>
		private string BuildGuildInfoUpdateSQL(Guild theguild)
		{
			//UPDATE t SET a = 'pdf' WHERE id = 2;
			//INSERT INTO t(id, a) SELECT 2, 'pdf' WHERE changes() = 0;


			StringBuilder sb = new StringBuilder();

			sb.Append(String.Format("UPDATE GUILDS SET GuildName = '{0}', Level = '{1}', MemberCount = '{2}', Realm = '{3}', AchievementPoints = '{4}', LastModified = '{5}', LastModifiedReadable = '{6}', Side = '{7}', CachedDateUnix = '{8}' WHERE GuildName = '{9}'",
				theguild.Name,
				theguild.Level.ToString(),
				theguild.Members.Count().ToString(),
				theguild.Realm,
				theguild.AchievementPoints.ToString(),
				theguild.LastModified.ToString(),
				Utility.ConvertUnixToLocalTime(theguild.LastModified),
				theguild.Side.ToString(),
				Utility.ConvertLocalTimeToUnix(DateTime.Now),
				theguild.Name
				));

			string sql = sb.ToString();
			return sql;
		}

		#endregion




		#region Update GuildMembers

		/// <summary>
		/// Upserts the PLAYERS table with the guilds Member list.
		/// </summary>
		/// <param name="theguild">A WoWDotNetAPI.Modules.Guild object.</param>
		/// <param name="inserts">Holds the number of inserts.</param>
		/// <param name="updates">Holds the number of updates.</param>
		/// <returns>The number of rows affected, or -1 on failure.</returns>
		public int UpdateGuildMembers(Guild theguild, out int inserts, out int updates)
		{
			if (!_isdbopen)
				this.Open();

			SQLiteCommand cmd = null;
			StringBuilder sb = new StringBuilder();
			int numrows = 0;
			inserts = 0;
			updates = 0;
			string sql = "";
			bool ret = false;

			foreach (GuildMember guildmember in theguild.Members)
			{
				// check if player exists
				ret = DoesPlayersExist(guildmember.Character.Name);

				if (ret)
				{
					// player exists so build update query
					sql = BuildUpdateGuildMemberSQL(theguild.Name, guildmember);
				}
				else
				{
					// player does not exists so build insert query
					sql = BuildInsertGuildMemberSQL(theguild.Name, guildmember);
				}

				cmd = new SQLiteCommand(sql, _cnn);

				try
				{
					numrows += cmd.ExecuteNonQuery();
					inserts++;
					//Debug.Assert(numrows > 0);

					//if (GuildInfoUpdated != null)
					//{
					//	// someone is subscribed, throw event
					//	GuildInfoUpdated(this, new EventArgs());
					//}
				}
				catch (SQLiteException se)
				{
					Console.WriteLine(se.Message);
					// skip to next one
					continue;
				}
				catch (Exception ex)
				{
					//Debug.Fail(ex.Message);
					Console.WriteLine(ex.Message);
					// skip to next one
					continue;
					//return -1;
				}
			}

			return numrows;
		}

		private string BuildInsertGuildMemberSQL(string theguildname, GuildMember guildmember)
		{
			StringBuilder sb = new StringBuilder();
			sb.Clear();
			sb.Append("INSERT INTO PLAYERS (PlayerID, PlayerName, GuildName, Class, Gender, Realm, LastModifiedFromGuild, LastModifiedReadableFromGuild, Level, Race, GuildRank, SpecName, SpecRole, CachedDateUnix) VALUES (");
			string sql = String.Format("{0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}'",
				"null",
				guildmember.Character.Name,
				theguildname,
				Utility.ToTitleCase(guildmember.Character.Class.ToString()),
				Utility.ToTitleCase(guildmember.Character.Gender.ToString()),
				guildmember.Character.GuildRealm,
				guildmember.Character.LastModified,
				Utility.ConvertUnixToLocalTime(guildmember.Character.LastModified),
				guildmember.Character.Level.ToString(),
				Utility.ToTitleCase(guildmember.Character.Race.ToString()),
				guildmember.Rank.ToString(),
				guildmember.Character.Specialization != null ? Utility.ToTitleCase(guildmember.Character.Specialization.Name) : "UNKNOWN",
				guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Role : "UNKNOWN",
				Utility.ConvertLocalTimeToUnix(DateTime.Now)
				);
			sb.Append(sql);

			sb.Append(")");

			return sb.ToString();
		}

		private string BuildUpdateGuildMemberSQL(string theguildname, GuildMember guildmember)
		{
			StringBuilder sb = new StringBuilder();
			sb.Clear();
			sb.Append(String.Format("UPDATE PLAYERS SET PlayerName = '{0}', GuildName = '{1}', Class = '{2}', Gender = '{3}', Realm = '{4}', LastModifiedFromGuild = '{5}', LastModifiedReadableFromGuild = '{6}', Level = '{7}', Race = '{8}', GuildRank = '{9}', SpecName = '{10}', SpecRole = '{11}', CachedDateUnix = '{12}' WHERE PlayerName = '{13}'",
				guildmember.Character.Name,
				theguildname,
				Utility.ToTitleCase(guildmember.Character.Class.ToString()),
				Utility.ToTitleCase(guildmember.Character.Gender.ToString()),
				guildmember.Character.GuildRealm,
				guildmember.Character.LastModified,
				Utility.ConvertUnixToLocalTime(long.Parse(guildmember.Character.LastModified)),
				guildmember.Character.Level.ToString(),
				Utility.ToTitleCase(guildmember.Character.Race.ToString()),
				guildmember.Rank.ToString(),
				guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Name : "UNKNOWN",
				guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Role : "UNKNOWN",
				Utility.ConvertLocalTimeToUnix(DateTime.Now),
				guildmember.Character.Name
				));

			return sb.ToString();
		}

		#endregion




		#region Update Player

		/// <summary>
		/// Upsert a player into the PLAYERS table.
		/// </summary>
		/// <param name="player">A WoWDotNetAPI.Modules.Character object.</param>
		/// <returns>True uf successful, otherwise false.</returns>
		public bool UpdatePlayer(Character player)
		{
			if (!_isdbopen)
				this.Open();

			SQLiteCommand cmd = null;
			StringBuilder sb = new StringBuilder();
			int numrows = 0;
			string sql = "";
			bool ret = false;

			// check if player exists
			ret = DoesPlayersExist(player.Name);

			if (ret)
			{
				// player exists so build update query
				sql = BuildUpdatePlayerSQL(player);
			}
			else
			{
				// player does not exists so build insert query
				sql = BuildInsertPlayerSQL(player);
			}

			cmd = new SQLiteCommand(sql, _cnn);
			try
			{
				numrows += cmd.ExecuteNonQuery();

				//	//if (GuildInfoUpdated != null)
				//	//{
				//	//	// someone is subscribed, throw event
				//	//	GuildInfoUpdated(this, new EventArgs());
				//	//}
			}
			catch (SQLiteException se)
			{
				Console.WriteLine(se.Message);
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}

			return true;
		}

		private string BuildInsertPlayerSQL(Character player)
		{
			//p.SPEC = guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Name : "UNKNOWN";
			//p.ROLE = guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Role : "UNKNOWN";

			//guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Name : "UNKNOWN",
			//guildmember.Character.Specialization != null ? guildmember.Character.Specialization.Role : "UNKNOWN",



			CharacterProfession[] profs = (CharacterProfession[])player.Professions.Primary;
			int numofprofs = 0;
			if (profs != null) numofprofs = profs.Length;


			StringBuilder sb = new StringBuilder();
			sb.Clear();
			sb.Append("INSERT INTO PLAYERS (PlayerID, PlayerName, GuildName, Realm, Level, Class, Race, AchievementPoints, AchievementsCompleted, Gender,");
			sb.Append("AverageItemLevel, AverageItemLevelEquipped, Profession1, Profession1Rank, Profession2, Profession2Rank, QuestsCompleted, TotalHonorableKills, LastModified, LastModifiedReadable, CachedDateUnix) VALUES (");
			sb.Append(String.Format("{0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}'",
				"null",                                             // PlayerID
				player.Name,                                        // PlayerName
				player.Guild != null ? player.Guild.Name : "Not in a Guild",    // GuildName
				player.Realm,                                       // Realm
				player.Level.ToString(),                            // Level
				Utility.ToTitleCase(player.Class.ToString()),       // Class
				Utility.ToTitleCase(player.Race.ToString()),        // Race
				player.AchievementPoints.ToString(),                // AchievementPoints
				player.Achievements.AchievementsCompleted.Count().ToString(),    // AchievementsCompleted
				Utility.ToTitleCase(player.Gender.ToString()),      // Gender
				player.Items.AverageItemLevel.ToString(),           // AverageItemLevel
				player.Items.AverageItemLevelEquipped.ToString(),   // AverageItemLevelEquipped


				numofprofs >= 1 ? profs[0].Name : "UNKNOWN",        // Profession1
				numofprofs >= 1 ? profs[0].Rank.ToString() : "0",   // Profession1Rank
				numofprofs > 1 ? profs[1].Name : "UNKNOWN",         // Profession2
				numofprofs > 1 ? profs[1].Rank.ToString() : "0",    // Profession2Rank


				//(Quest)player.Quests),
				"0",                                                // QuestsCompleted

				player.TotalHonorableKills.ToString(),              // TotalHonorableKills
				player.LastModified,                                // LastModified
				Utility.ConvertUnixToLocalTime(player.LastModified),// LastModifiedReadable
				Utility.ConvertLocalTimeToUnix(DateTime.Now)        // CachedDateUnix
				));
			sb.Append(")");

			return sb.ToString();
		}

		private string BuildUpdatePlayerSQL(Character player)
		{
			StringBuilder sb = new StringBuilder();
			sb.Clear();

			string p1 = "UPDATE PLAYERS SET GuildName = '{0}', Realm = '{1}', Level = '{2}', Class = '{3}', Race = '{4}', AchievementPoints = '{5}', AchievementsCompleted = '{6}', Gender = '{7}',";
			string p2 = "AverageItemLevel = '{8}', AverageItemLevelEquipped = '{9}', Profession1 = '{10}', Profession1Rank = '{11}', Profession2 = '{12}', Profession2Rank = '{13}', QuestsCompleted = '{14}', TotalHonorableKills = '{15}', LastModified = '{16}', LastModifiedReadable = '{17}', CachedDateUnix = '{18}'  WHERE PlayerName = '{19}'";

			string p3 = p1 + p2;


			CharacterProfession[] profs = (CharacterProfession[])player.Professions.Primary;
			int numofprofs = 0;
			if (profs != null) numofprofs = profs.Length;

			try
			{
				sb.Append(String.Format(p3,
					player.Guild != null ? player.Guild.Name : "Not in a Guild",    // GuildName
					player.Realm,                                       // Realm
					player.Level.ToString(),                            // Level
					Utility.ToTitleCase(player.Class.ToString()),       // Class
					Utility.ToTitleCase(player.Race.ToString()),        // Race
					player.AchievementPoints.ToString(),                // AchievementPoints
					player.Achievements.AchievementsCompleted.Count().ToString(), // AchievementsCompleted
					Utility.ToTitleCase(player.Gender.ToString()),      // Gender
					player.Items.AverageItemLevel.ToString(),           // AverageItemLevel
					player.Items.AverageItemLevelEquipped.ToString(),   // AverageItemLevelEquipped

					numofprofs >= 1 ? profs[0].Name : "UNKNOWN",        // Profession1
					numofprofs >= 1 ? profs[0].Rank.ToString() : "0",   // Profession1Rank
					numofprofs > 1 ? profs[1].Name : "UNKNOWN",         // Profession2
					numofprofs > 1 ? profs[1].Rank.ToString() : "0",    // Profession2Rank


					//(Quest)player.Quests),
					"0",                                                // QuestsCompleted

					player.TotalHonorableKills.ToString(),              // TotalHonorableKills
					player.LastModified,                                // LastModified
					Utility.ConvertUnixToLocalTime(player.LastModified),// LastModifiedReadable
					Utility.ConvertLocalTimeToUnix(DateTime.Now),       // CachedDateUnix
					player.Name
					));

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

			}

			return sb.ToString();
		}

		#endregion










		public bool Test()
		{
			if (!_isdbopen)
				this.Open();

			SQLiteCommand cmd = null;
			StringBuilder sb = new StringBuilder();
			int numrows = 0;
			string sql = "UPDATE PLAYERS SET Level = '210', Class = 'Robot' WHERE Playername = 'Ripgut'";


			// attempt to INSERT the database
			cmd = new SQLiteCommand(sql, _cnn);
			try
			{
				numrows += cmd.ExecuteNonQuery();
				Console.WriteLine("");
			}
			catch (SQLiteException se)
			{
				if (se.ResultCode.ToString() == "Mismatch")
				{
					// data type mismatch
					Console.WriteLine("");
				}
				if (se.ResultCode.ToString() == "Constraint")
				{
					Console.WriteLine("");
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return false;
			}


			return true;
		}








		#region IDisposable Implementation

		/// <summary>
		/// Can be called by the client.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Does the actuall cleaning up depending on if client or GC called.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// TODO: clean up any other resources
			}
			Close();
		}

		/// <summary>
		/// DStor
		/// </summary>
		/// <remarks>Called by the GC</remarks>
		~DataAccess()
		{
			Dispose(false);
		}

		#endregion
	}

}
