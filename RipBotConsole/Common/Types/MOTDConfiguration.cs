using Newtonsoft.Json;
using System;
using System.IO;

namespace RipBot.Types
{
	/// <summary> 
	/// A file that contains information you either don't want public
	/// or will want to change without having to compile another bot.
	/// </summary>
	public class MOTDConfiguration
	{
		/// <summary> The location of your bot's dll, ignored by the json parser. </summary>
		[JsonIgnore]
		public static readonly string appdir = AppContext.BaseDirectory;


		/// <summary> The MOTD message </summary>
		public string Message { get; set; }
		/// <summary> The interval in minutes to use for the MOTD timer </summary>
		public string IntervalInMinutes { get; set; }



		/// <summary>
		/// CStor.
		/// </summary>
		public MOTDConfiguration()
		{
			//Message = "Default MOTD message";
			//IntervalInMinutes = "5";
		}

		/// <summary> Save the configuration to the specified file location. </summary>
		public void Save(string dir = "data/motdconfiguration.json")
		{
			string file = Path.Combine(appdir, dir);
			File.WriteAllText(file, ToJson());
		}

		/// <summary> Load the configuration from the specified file location. </summary>
		public static MOTDConfiguration Load(string dir = "data/motdconfiguration.json")
		{
			string file = Path.Combine(appdir, dir);
			return JsonConvert.DeserializeObject<MOTDConfiguration>(File.ReadAllText(file));
		}

		/// <summary> Convert the configuration to a json string. </summary>
		public string ToJson()
			=> JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
