using System;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestThreadUser : RestEntity<ulong>, IThreadUser, IMentionable
{
	public IThreadChannel Thread { get; }

	public DateTimeOffset ThreadJoinedAt { get; private set; }

	public IGuild Guild { get; }

	public string Mention => MentionUtils.MentionUser(base.Id);

	internal RestThreadUser(BaseDiscordClient discord, IGuild guild, IThreadChannel channel, ulong id)
		: base(discord, id)
	{
		Guild = guild;
		Thread = channel;
	}

	internal static RestThreadUser Create(BaseDiscordClient client, IGuild guild, ThreadMember model, IThreadChannel channel)
	{
		RestThreadUser restThreadUser = new RestThreadUser(client, guild, channel, model.UserId.Value);
		restThreadUser.Update(model);
		return restThreadUser;
	}

	internal void Update(ThreadMember model)
	{
		ThreadJoinedAt = model.JoinTimestamp;
	}

	public Task<IGuildUser> GetGuildUser()
	{
		return Guild.GetUserAsync(base.Id);
	}
}
