using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RipBot.Attributes;
using RipBot.Enums;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using WowDotNetAPI;
using WowDotNetAPI.Models;

namespace RipBot.Modules
{
	[Name("General")]
	public class GeneralModule : ModuleBase<SocketCommandContext>
	{
		[Command("info")]
		[Remarks("Display some bot info.")]
		[Summary("EX: ripbot info")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task Info()
		{
			var application = await Context.Client.GetApplicationInfoAsync();
			await ReplyAsync(
				$"{Format.Bold("Info")}\n" +
				//$"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
				$"- Author: {application.Owner.Username} (ID {application.Owner.Username + "#" + application.Owner.Discriminator})\n" +
				//$"- Author: {application.Owner.Username} (ID {application.Owner.Discriminator})\n" +
				$"- Library: Discord.Net ({DiscordConfig.Version})\n" +
				$"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
				$"- Uptime: {GetUptime()}\n\n" +

				$"{Format.Bold("Stats")}\n" +
				$"- Heap Size: {GetHeapSize()} MB\n" +
				$"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
				$"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
				$"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
			);
		}

		private static string GetUptime()
			=> (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();





		[Command("userinfo"), Alias("user", "whois")]
		[Remarks("Returns info about the current user (which would be the bot), or the user parameter, if one passed.")]
		[Summary("EX: ripbot userinfo\nEX: ripbot userinfo Ripgut\nEX: ripbot userinfo 96642168176807936\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}




		/// <summary>
		/// Used for running various short tests.
		/// </summary>
		/// <returns></returns>
		[Command("test")]
		[Remarks("test")]
		[Summary("ripbot test")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task TestCmd()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Test:\n\n");



			EmbedBuilder emb = new EmbedBuilder();
			EmbedAuthorBuilder embauth = new EmbedAuthorBuilder();
			embauth.Name = "Auth name";
			//embauth.IconUrl = message.Author.AvatarUrl;
			emb.WithAuthor(embauth);
			emb.AddField(x =>
			{
				x.IsInline = true;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name1";
				x.Value = "Value1";
			});
			emb.AddField(x =>
			{
				x.IsInline = true;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name2";
				x.Value = "Value2";
			});
			emb.AddField(x =>
			{
				x.IsInline = true;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name3";
				x.Value = "Value3";
			}
			);
			await ReplyAsync("", embed: emb);



			//bool ret = Utility.CacheOurChannels(Context.Guild);
			//Console.WriteLine(ret.ToString());

			//sb.AppendLine("nk = " + Globals.OURCHANNELSNAMEKEY["general"].ToString());

			//ulong ul = (ulong)Globals.OURCHANNELSNAMEKEY["general"];
			//SocketChannel channel = Context.Client.GetChannel(ul);
			//SocketTextChannel sokText = channel as SocketTextChannel;
			//await sokText.SendMessageAsync("test"); //you can understand i think




			////sb.AppendLine("**First Header** | Second Header");
			////sb.AppendLine("----- | -----");
			////sb.AppendLine("Cell1 | Cell2");

			await ReplyAsync(sb.ToString());
		}





		//[Command("adminsay"), Alias("as")]
		//[Remarks("Make the bot say something by a server admin")]
		//[Summary("ripbot adminsay Hello")]
		//[MinPermissions(AccessLevel.ServerAdmin)]
		//public async Task AdminSay([Remainder]string text)
		//{
		//	await ReplyAsync(text);
		//}




		//[Command("say"), Alias("say")]
		//[Remarks("Echo a users input")]
		//[Summary("ripbot say Hello")]
		////[Description("Make the bot say something by a user")]
		//[MinPermissions(AccessLevel.ServerAdmin)]
		//public async Task Say([Remainder]string whattosay)
		//{
		//	await ReplyAsync(whattosay);
		//}


		//[Command("whereiszang"), Alias("wiz")]
		//[Remarks("Locates Zang.")]
		//[Summary("ripbot whereiszang")]
		//[MinPermissions(AccessLevel.User)]
		//public async Task WhereIsZang()
		//{
		//	await ReplyAsync("Zang is AFK");
		//}



		//[Group("set"), Name("General")]
		//public class Set : ModuleBase
		//{
		//	[Command("nick")]
		//	[Remarks("Change a users nick")]
		//	[MinPermissions(AccessLevel.User)]
		//	public async Task Nick([Remainder]string name)
		//	{
		//		var user = Context.User as SocketGuildUser;
		//		await user.ModifyAsync(x => x.Nickname = name);

		//		await ReplyAsync($"{user.Mention} I changed your name to **{name}**");
		//	}

		//	[Command("botnick")]
		//	[Remarks("Make the bot say something")]
		//	[MinPermissions(AccessLevel.ServerOwner)]
		//	public async Task BotNick([Remainder]string name)
		//	{
		//		var self = await Context.Guild.GetCurrentUserAsync();
		//		await self.ModifyAsync(x => x.Nickname = name);

		//		await ReplyAsync($"I changed my name to **{name}**");
		//	}

		//	[Command("say"), Alias("say")]
		//	[Remarks("Echo a users input")]
		//	[Summary("ripbot say Hello")]
		//	//[Description("Make the bot say something by a user")]
		//	[MinPermissions(AccessLevel.User)]
		//	public async Task Say([Remainder]string whattosay)
		//	{
		//		await ReplyAsync(whattosay);
		//	}


		//	[Command("whereiszang"), Alias("wiz")]
		//	[Remarks("Locates Zang.")]
		//	[Summary("ripbot whereiszang")]
		//	[MinPermissions(AccessLevel.User)]
		//	public async Task WhereIsZang()
		//	{
		//		await ReplyAsync("Zang is AFK");
		//	}

		//}
	}

}
