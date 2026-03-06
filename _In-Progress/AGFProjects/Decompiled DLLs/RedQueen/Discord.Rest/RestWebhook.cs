using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestWebhook : RestEntity<ulong>, IWebhook, IDeletable, ISnowflakeEntity, IEntity<ulong>, IUpdateable
{
	internal IGuild Guild { get; private set; }

	internal ITextChannel Channel { get; private set; }

	public string Token { get; }

	public ulong ChannelId { get; private set; }

	public string Name { get; private set; }

	public string AvatarId { get; private set; }

	public ulong? GuildId { get; private set; }

	public IUser Creator { get; private set; }

	public ulong? ApplicationId { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	private string DebuggerDisplay => $"Webhook: {Name} ({base.Id})";

	IGuild IWebhook.Guild => Guild ?? throw new InvalidOperationException("Unable to return this entity's parent unless it was fetched through that object.");

	ITextChannel IWebhook.Channel => Channel ?? throw new InvalidOperationException("Unable to return this entity's parent unless it was fetched through that object.");

	internal RestWebhook(BaseDiscordClient discord, IGuild guild, ulong id, string token, ulong channelId)
		: base(discord, id)
	{
		Guild = guild;
		Token = token;
		ChannelId = channelId;
	}

	internal RestWebhook(BaseDiscordClient discord, ITextChannel channel, ulong id, string token, ulong channelId)
		: this(discord, channel.Guild, id, token, channelId)
	{
		Channel = channel;
	}

	internal static RestWebhook Create(BaseDiscordClient discord, IGuild guild, Webhook model)
	{
		RestWebhook restWebhook = new RestWebhook(discord, guild, model.Id, model.Token, model.ChannelId);
		restWebhook.Update(model);
		return restWebhook;
	}

	internal static RestWebhook Create(BaseDiscordClient discord, ITextChannel channel, Webhook model)
	{
		RestWebhook restWebhook = new RestWebhook(discord, channel, model.Id, model.Token, model.ChannelId);
		restWebhook.Update(model);
		return restWebhook;
	}

	internal void Update(Webhook model)
	{
		if (ChannelId != model.ChannelId)
		{
			ChannelId = model.ChannelId;
		}
		if (model.Avatar.IsSpecified)
		{
			AvatarId = model.Avatar.Value;
		}
		if (model.Creator.IsSpecified)
		{
			Creator = RestUser.Create(base.Discord, model.Creator.Value);
		}
		if (model.GuildId.IsSpecified)
		{
			GuildId = model.GuildId.Value;
		}
		if (model.Name.IsSpecified)
		{
			Name = model.Name.Value;
		}
		ApplicationId = model.ApplicationId;
	}

	public async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetWebhookAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetUserAvatarUrl(base.Id, AvatarId, size, format);
	}

	public async Task ModifyAsync(Action<WebhookProperties> func, RequestOptions options = null)
	{
		Update(await WebhookHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return WebhookHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return $"Webhook: {Name}:{base.Id}";
	}

	Task IWebhook.ModifyAsync(Action<WebhookProperties> func, RequestOptions options)
	{
		return ModifyAsync(func, options);
	}
}
