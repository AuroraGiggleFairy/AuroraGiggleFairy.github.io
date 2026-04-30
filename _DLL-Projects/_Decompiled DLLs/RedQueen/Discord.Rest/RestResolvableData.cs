using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestResolvableData<T> where T : IResolvable
{
	internal readonly Dictionary<ulong, RestGuildUser> GuildMembers = new Dictionary<ulong, RestGuildUser>();

	internal readonly Dictionary<ulong, RestUser> Users = new Dictionary<ulong, RestUser>();

	internal readonly Dictionary<ulong, RestChannel> Channels = new Dictionary<ulong, RestChannel>();

	internal readonly Dictionary<ulong, RestRole> Roles = new Dictionary<ulong, RestRole>();

	internal readonly Dictionary<ulong, RestMessage> Messages = new Dictionary<ulong, RestMessage>();

	internal readonly Dictionary<ulong, Attachment> Attachments = new Dictionary<ulong, Attachment>();

	internal async Task PopulateAsync(DiscordRestClient discord, RestGuild guild, IRestMessageChannel channel, T model, bool doApiCall)
	{
		ApplicationCommandInteractionDataResolved resolved = model.Resolved.Value;
		if (resolved.Users.IsSpecified)
		{
			foreach (KeyValuePair<string, User> item in resolved.Users.Value)
			{
				RestUser value = RestUser.Create(discord, item.Value);
				Users.Add(ulong.Parse(item.Key), value);
			}
		}
		if (resolved.Channels.IsSpecified)
		{
			IReadOnlyCollection<RestGuildChannel> readOnlyCollection = ((!doApiCall) ? null : (await guild.GetChannelsAsync().ConfigureAwait(continueOnCapturedContext: false)));
			IReadOnlyCollection<RestGuildChannel> readOnlyCollection2 = readOnlyCollection;
			foreach (KeyValuePair<string, Channel> channelModel in resolved.Channels.Value)
			{
				if (readOnlyCollection2 != null)
				{
					RestGuildChannel restGuildChannel = readOnlyCollection2.FirstOrDefault((RestGuildChannel x) => x.Id == channelModel.Value.Id);
					restGuildChannel.Update(channelModel.Value);
					Channels.Add(ulong.Parse(channelModel.Key), restGuildChannel);
				}
				else
				{
					RestChannel restChannel = RestChannel.Create(discord, channelModel.Value);
					restChannel.Update(channelModel.Value);
					Channels.Add(ulong.Parse(channelModel.Key), restChannel);
				}
			}
		}
		if (resolved.Members.IsSpecified)
		{
			foreach (KeyValuePair<string, GuildMember> member in resolved.Members.Value)
			{
				member.Value.User = resolved.Users.Value.FirstOrDefault((KeyValuePair<string, User> x) => x.Key == member.Key).Value;
				RestGuildUser value2 = RestGuildUser.Create(discord, guild, member.Value);
				GuildMembers.Add(ulong.Parse(member.Key), value2);
			}
		}
		if (resolved.Roles.IsSpecified)
		{
			foreach (KeyValuePair<string, Role> item2 in resolved.Roles.Value)
			{
				RestRole value3 = RestRole.Create(discord, guild, item2.Value);
				Roles.Add(ulong.Parse(item2.Key), value3);
			}
		}
		if (resolved.Messages.IsSpecified)
		{
			foreach (KeyValuePair<string, Message> msg in resolved.Messages.Value)
			{
				if (channel == null)
				{
					RestChannel restChannel2 = Channels.FirstOrDefault((KeyValuePair<ulong, RestChannel> x) => x.Key == msg.Value.ChannelId).Value;
					if (restChannel2 == null)
					{
						RestChannel restChannel3 = ((!doApiCall) ? null : (await discord.GetChannelAsync(msg.Value.ChannelId).ConfigureAwait(continueOnCapturedContext: false)));
						restChannel2 = restChannel3;
					}
					channel = (IRestMessageChannel)restChannel2;
				}
				RestUser author = ((!msg.Value.Author.IsSpecified) ? RestGuildUser.Create(discord, guild, msg.Value.Member.Value) : RestUser.Create(discord, msg.Value.Author.Value));
				RestMessage restMessage = RestMessage.Create(discord, channel, author, msg.Value);
				Messages.Add(restMessage.Id, restMessage);
			}
		}
		if (!resolved.Attachments.IsSpecified)
		{
			return;
		}
		foreach (KeyValuePair<string, Discord.API.Attachment> item3 in resolved.Attachments.Value)
		{
			Attachment value4 = Attachment.Create(item3.Value);
			Attachments.Add(ulong.Parse(item3.Key), value4);
		}
	}
}
