﻿using Newtonsoft.Json;
using System;
using System.IO;

namespace RipBot.Types
{
	/// <summary> 
	/// A file that contains information you either don't want public
	/// or will want to change without having to compile another bot.
	/// </summary>
	public class BotConfiguration
	{
		/// <summary> The location of your bot's dll, ignored by the json parser. </summary>
		[JsonIgnore]
		public static readonly string appdir = AppContext.BaseDirectory;

		/// <summary> Your bot's command prefix. Please don't pick `!`. </summary>
		public string Prefix { get; set; }
		/// <summary> Ids of users who will have owner access to the bot. </summary>
		public ulong[] Owners { get; set; }
		/// <summary> Your bot's login token. </summary>
		public string Token { get; set; }
		/// <summary> Log commands and responses. </summary>
		public bool LogCommands { get; set; }


		/// <summary>
		/// CStor.
		/// </summary>
		public BotConfiguration()
		{
			Prefix = "ripbot ";
			Owners = new ulong[] { 0 };
			Token = "";
			LogCommands = true;
		}

		/// <summary> Save the configuration to the specified file location. </summary>
		public void Save(string dir = "data/botconfiguration.json")
		{
			string file = Path.Combine(appdir, dir);
			File.WriteAllText(file, ToJson());
		}

		/// <summary> Load the configuration from the specified file location. </summary>
		public static BotConfiguration Load(string dir = "data/botconfiguration.json")
		{
			string file = Path.Combine(appdir, dir);
			return JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText(file));
		}

		/// <summary> Convert the configuration to a json string. </summary>
		public string ToJson()
			=> JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
