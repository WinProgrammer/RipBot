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
	/// <summary>
	/// Handles all general commands.
	/// </summary>
	[Name("General")]
	public class GeneralModule : ModuleBase<SocketCommandContext>
	{
		/// <summary>
		/// Display some bot info.
		/// </summary>
		/// <returns></returns>
		[Command("info")]
		[Remarks("Display some bot info.")]
		[Summary("EX: ripbot info")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task InfoCmd()
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



		/// <summary>
		/// Displays some info about the server.
		/// </summary>
		/// <returns></returns>
		[Command("serverinfo")]
		[Remarks("Display some server info.")]
		[Summary("EX: ripbot serverinfo")]
		[RequireContext(ContextType.Guild)]
		public async Task ServerInfoCmd()
		{
			var channel = (ITextChannel)Context.Channel;
			IGuild guild = channel.Guild;
			if (guild == null)
				return;
			var ownername = await guild.GetUserAsync(guild.OwnerId);
			var textchn = (await guild.GetTextChannelsAsync()).Count();
			var voicechn = (await guild.GetVoiceChannelsAsync()).Count();

			var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(guild.Id >> 22);
			var users = await guild.GetUsersAsync().ConfigureAwait(false);
			var features = string.Join("\n", guild.Features);
			if (string.IsNullOrWhiteSpace(features))
				features = "-";
			var embed = new EmbedBuilder()
					.WithAuthor(eab => eab.WithName("Server Info"))
					.WithTitle(guild.Name)
					.AddField(fb => fb.WithName("**ID**").WithValue(guild.Id.ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Owner**").WithValue(ownername.ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Members**").WithValue(users.Count.ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Text Channels**").WithValue(textchn.ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Voice Channels**").WithValue(voicechn.ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Created At**").WithValue($"{createdAt:MM.dd.yyyy HH:mm}").WithIsInline(true))
					.AddField(fb => fb.WithName("**Region**").WithValue(guild.VoiceRegionId.ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Roles**").WithValue((guild.Roles.Count - 1).ToString()).WithIsInline(true))
					.AddField(fb => fb.WithName("**Features**").WithValue(features).WithIsInline(true))
					.WithImageUrl(guild.IconUrl)
				.WithColor(new Color(0, 191, 255))
				;
			embed.Build();

			await ReplyAsync("", embed: embed);
		}



		/// <summary>
		/// Returns info about the current user (which would be the bot), or the user parameter, if one passed.
		/// </summary>
		/// <param name="user">The IUser to get info on.</param>
		/// <returns></returns>
		[Command("userinfo"), Alias("user", "whois")]
		[Remarks("Returns info about the current user (which would be the bot), or the user parameter, if one passed.")]
		[Summary("EX: ripbot userinfo\nEX: ripbot userinfo Ripgut\nEX: ripbot userinfo 96642168176807936\n")]
		[MinPermissions(AccessLevel.ServerAdmin)]
		public async Task UserInfoCmd([Summary("The (optional) user to get info for")] IUser user = null)
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



			EmbedBuilder emb = new EmbedBuilder()
			//emb.WithDescription("Description");

			    .WithAuthor(new EmbedAuthorBuilder()
				.WithIconUrl("http://vignette4.wikia.nocookie.net/mspaintadventures/images/7/77/Omgitsskaia.png/revision/latest?cb=20111231073525")
				.WithName("Profile"))
				.WithColor(new Color(0, 191, 255))
				//.WithThumbnailUrl("http://vignette4.wikia.nocookie.net/mspaintadventures/images/7/77/Omgitsskaia.png/revision/latest?cb=20111231073525")
				.WithTitle("Title")
				.WithDescription("Description")
				//.WithTimestamp(DateTime.Now.ToLocalTime())
				.WithCurrentTimestamp()
				;



			//// Author
			//EmbedAuthorBuilder embauth = new EmbedAuthorBuilder();
			//embauth.Name = "RipBot";
			////embauth.IconUrl = message.Author.AvatarUrl;
			//emb.WithAuthor(embauth);

			bool inline = true;

			// Fields
			emb.AddField(efb => efb.WithName(Format.Bold("TestName1")).WithValue("TestValue1").WithIsInline(false));
			emb.AddField(x =>
			{
				x.IsInline = inline;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name1";
				x.Value = "[TestUrl](https://wow.zamimg.com/images/wow/icons/large/inv_6_2raid_ring_2a.jpg)";
			});
			emb.AddField(x =>
			{
				x.IsInline = inline;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name2";
				x.Value = "https://wow.zamimg.com/images/wow/icons/large/inv_6_2raid_ring_2a.jpg";
			});
			emb.AddField(x =>
			{
				x.IsInline = inline;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name3";
				x.Value = "(https://wow.zamimg.com/images/wow/icons/large/inv_6_2raid_ring_2a.jpg)";
			});
			emb.AddField(x =>
			{
				x.IsInline = inline;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name4";
				x.Value = "[https://wow.zamimg.com/images/wow/icons/large/inv_6_2raid_ring_2a.jpg] Test";
			});
			emb.AddField(x =>
			{
				x.IsInline = inline;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name5";
				x.Value = "Value5";
			});
			emb.AddField(x =>
			{
				x.IsInline = inline;
				//x.Name = message.CreatedAt.ToUniversalTime().ToString() + "UTC";
				//x.Value = message.Content;
				x.Name = "Name6";
				x.Value = "Value6";
			}
			);

			// Footer
			EmbedFooterBuilder embfoot = new EmbedFooterBuilder();
			embfoot.Text = "Footer";
			emb.WithFooter(embfoot);

			emb.Build();


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

			//await ReplyAsync(sb.ToString());
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
