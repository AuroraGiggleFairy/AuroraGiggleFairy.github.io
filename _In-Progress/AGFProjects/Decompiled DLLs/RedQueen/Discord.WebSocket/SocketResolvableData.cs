using System.Collections.Generic;
using Discord.API;
using Discord.Net;

namespace Discord.WebSocket;

internal class SocketResolvableData<T> where T : IResolvable
{
	internal readonly Dictionary<ulong, SocketGuildUser> GuildMembers = new Dictionary<ulong, SocketGuildUser>();

	internal readonly Dictionary<ulong, SocketGlobalUser> Users = new Dictionary<ulong, SocketGlobalUser>();

	internal readonly Dictionary<ulong, SocketChannel> Channels = new Dictionary<ulong, SocketChannel>();

	internal readonly Dictionary<ulong, SocketRole> Roles = new Dictionary<ulong, SocketRole>();

	internal readonly Dictionary<ulong, SocketMessage> Messages = new Dictionary<ulong, SocketMessage>();

	internal readonly Dictionary<ulong, Attachment> Attachments = new Dictionary<ulong, Attachment>();

	internal SocketResolvableData(DiscordSocketClient discord, ulong? guildId, T model)
	{
		SocketGuild socketGuild = (guildId.HasValue ? discord.GetGuild(guildId.Value) : null);
		ApplicationCommandInteractionDataResolved value = model.Resolved.Value;
		if (value.Users.IsSpecified)
		{
			foreach (KeyValuePair<string, User> item in value.Users.Value)
			{
				SocketGlobalUser orCreateUser = discord.GetOrCreateUser(discord.State, item.Value);
				Users.Add(ulong.Parse(item.Key), orCreateUser);
			}
		}
		if (value.Channels.IsSpecified)
		{
			foreach (KeyValuePair<string, Channel> item2 in value.Channels.Value)
			{
				SocketChannel socketChannel = ((socketGuild != null) ? socketGuild.GetChannel(item2.Value.Id) : discord.GetChannel(item2.Value.Id));
				if (socketChannel == null)
				{
					try
					{
						Channel model2 = ((socketGuild != null) ? discord.Rest.ApiClient.GetChannelAsync(socketGuild.Id, item2.Value.Id).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
							.GetResult() : discord.Rest.ApiClient.GetChannelAsync(item2.Value.Id).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
							.GetResult());
						socketChannel = ((socketGuild != null) ? SocketGuildChannel.Create(socketGuild, discord.State, model2) : ((SocketChannel)SocketChannel.CreatePrivate(discord, discord.State, model2)));
					}
					catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions)
					{
						socketChannel = (guildId.HasValue ? SocketGuildChannel.Create(socketGuild, discord.State, item2.Value) : ((SocketChannel)SocketChannel.CreatePrivate(discord, discord.State, item2.Value)));
					}
				}
				discord.State.AddChannel(socketChannel);
				Channels.Add(ulong.Parse(item2.Key), socketChannel);
			}
		}
		if (value.Members.IsSpecified && socketGuild != null)
		{
			foreach (KeyValuePair<string, GuildMember> item3 in value.Members.Value)
			{
				item3.Value.User = value.Users.Value[item3.Key];
				SocketGuildUser value2 = socketGuild.AddOrUpdateUser(item3.Value);
				GuildMembers.Add(ulong.Parse(item3.Key), value2);
			}
		}
		if (value.Roles.IsSpecified && socketGuild != null)
		{
			foreach (KeyValuePair<string, Role> item4 in value.Roles.Value)
			{
				SocketRole value3 = ((socketGuild == null) ? SocketRole.Create(null, discord.State, item4.Value) : socketGuild.AddOrUpdateRole(item4.Value));
				Roles.Add(ulong.Parse(item4.Key), value3);
			}
		}
		if (value.Messages.IsSpecified)
		{
			foreach (KeyValuePair<string, Message> msg in value.Messages.Value)
			{
				ISocketMessageChannel socketMessageChannel = discord.GetChannel(msg.Value.ChannelId) as ISocketMessageChannel;
				SocketUser socketUser = ((socketGuild == null) ? (socketMessageChannel as SocketChannel)?.GetUser(msg.Value.Author.Value.Id) : ((!msg.Value.WebhookId.IsSpecified) ? ((SocketUser)socketGuild.GetUser(msg.Value.Author.Value.Id)) : ((SocketUser)SocketWebhookUser.Create(socketGuild, discord.State, msg.Value.Author.Value, msg.Value.WebhookId.Value))));
				if (socketMessageChannel == null && !guildId.HasValue)
				{
					socketMessageChannel = discord.CreateDMChannel(msg.Value.ChannelId, msg.Value.Author.Value, discord.State);
					socketUser = ((SocketDMChannel)socketMessageChannel).Recipient;
				}
				if (socketUser == null)
				{
					socketUser = discord.State.GetOrAddUser(msg.Value.Author.Value.Id, (ulong _) => SocketGlobalUser.Create(discord, discord.State, msg.Value.Author.Value));
				}
				SocketMessage socketMessage = SocketMessage.Create(discord, discord.State, socketUser, socketMessageChannel, msg.Value);
				Messages.Add(socketMessage.Id, socketMessage);
			}
		}
		if (!value.Attachments.IsSpecified)
		{
			return;
		}
		foreach (KeyValuePair<string, Discord.API.Attachment> item5 in value.Attachments.Value)
		{
			Attachment value4 = Attachment.Create(item5.Value);
			Attachments.Add(ulong.Parse(item5.Key), value4);
		}
	}
}
