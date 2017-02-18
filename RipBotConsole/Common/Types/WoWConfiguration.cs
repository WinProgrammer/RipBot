using Newtonsoft.Json;
using System;
using System.IO;

namespace RipBot.Types
{
	/// <summary> 
	/// A file that contains information you either don't want public
	/// or will want to change without having to compile another bot.
	/// </summary>
	public class WoWConfiguration
	{
		/// <summary> The location of your bot's dll, ignored by the json parser. </summary>
		[JsonIgnore]
		public static readonly string appdir = AppContext.BaseDirectory;

		///// <summary> Ids of users who will have owner access to the bot. </summary>
		//public ulong[] Owners { get; set; }
		
		
		/// <summary> Your WoW Mashery key </summary>
		public string MasheryKey { get; set; }
		/// <summary> Your WoW default guild name </summary>
		public string DefaultGuildName { get; set; }
		/// <summary> Your WoW default realm </summary>
		public string DefaultRealm { get; set; }


		/// <summary>
		/// CStor.
		/// </summary>
		public WoWConfiguration()
		{
			// set some defaults
			//Owners = new ulong[] { 0 };
			MasheryKey = "";
			DefaultGuildName = "Hordecorp";
			DefaultRealm = "Aerie Peak";
		}

		/// <summary> Save the configuration to the specified file location. </summary>
		public void Save(string dir = "data/wowconfiguration.json")
		{
			string file = Path.Combine(appdir, dir);
			File.WriteAllText(file, ToJson());
		}

		/// <summary> Load the configuration from the specified file location. </summary>
		public static WoWConfiguration Load(string dir = "data/wowconfiguration.json")
		{
			string file = Path.Combine(appdir, dir);
			return JsonConvert.DeserializeObject<WoWConfiguration>(File.ReadAllText(file));
		}

		/// <summary> Convert the configuration to a json string. </summary>
		public string ToJson()
			=> JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
