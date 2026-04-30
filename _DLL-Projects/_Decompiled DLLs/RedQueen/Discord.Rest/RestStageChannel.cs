using System;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal class RestStageChannel : RestVoiceChannel, IStageChannel, IVoiceChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, INestedChannel, IGuildChannel, IDeletable, IAudioChannel, IMentionable
{
	public override bool IsTextInVoice => false;

	public StagePrivacyLevel? PrivacyLevel { get; private set; }

	public bool? IsDiscoverableDisabled { get; private set; }

	public bool IsLive { get; private set; }

	internal RestStageChannel(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, guild, id)
	{
	}

	internal new static RestStageChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestStageChannel restStageChannel = new RestStageChannel(discord, guild, model.Id);
		restStageChannel.Update(model);
		return restStageChannel;
	}

	internal void Update(StageInstance model, bool isLive = false)
	{
		IsLive = isLive;
		if (isLive)
		{
			PrivacyLevel = model.PrivacyLevel;
			IsDiscoverableDisabled = model.DiscoverableDisabled;
		}
		else
		{
			PrivacyLevel = null;
			IsDiscoverableDisabled = null;
		}
	}

	public async Task ModifyInstanceAsync(Action<StageInstanceProperties> func, RequestOptions options = null)
	{
		Update(await ChannelHelper.ModifyAsync(this, base.Discord, func, options), isLive: true);
	}

	public async Task StartStageAsync(string topic, StagePrivacyLevel privacyLevel = StagePrivacyLevel.GuildOnly, RequestOptions options = null)
	{
		CreateStageInstanceParams args = new CreateStageInstanceParams
		{
			ChannelId = base.Id,
			PrivacyLevel = privacyLevel,
			Topic = topic
		};
		Update(await base.Discord.ApiClient.CreateStageInstanceAsync(args, options), isLive: true);
	}

	public async Task StopStageAsync(RequestOptions options = null)
	{
		await base.Discord.ApiClient.DeleteStageInstanceAsync(base.Id, options);
		Update(null);
	}

	public override async Task UpdateAsync(RequestOptions options = null)
	{
		await base.UpdateAsync(options);
		StageInstance stageInstance = await base.Discord.ApiClient.GetStageInstanceAsync(base.Id, options);
		Update(stageInstance, stageInstance != null);
	}

	public Task RequestToSpeakAsync(RequestOptions options = null)
	{
		ModifyVoiceStateParams args = new ModifyVoiceStateParams
		{
			ChannelId = base.Id,
			RequestToSpeakTimestamp = DateTimeOffset.UtcNow
		};
		return base.Discord.ApiClient.ModifyMyVoiceState(base.Guild.Id, args, options);
	}

	public Task BecomeSpeakerAsync(RequestOptions options = null)
	{
		ModifyVoiceStateParams args = new ModifyVoiceStateParams
		{
			ChannelId = base.Id,
			Suppressed = false
		};
		return base.Discord.ApiClient.ModifyMyVoiceState(base.Guild.Id, args, options);
	}

	public Task StopSpeakingAsync(RequestOptions options = null)
	{
		ModifyVoiceStateParams args = new ModifyVoiceStateParams
		{
			ChannelId = base.Id,
			Suppressed = true
		};
		return base.Discord.ApiClient.ModifyMyVoiceState(base.Guild.Id, args, options);
	}

	public Task MoveToSpeakerAsync(IGuildUser user, RequestOptions options = null)
	{
		ModifyVoiceStateParams args = new ModifyVoiceStateParams
		{
			ChannelId = base.Id,
			Suppressed = false
		};
		return base.Discord.ApiClient.ModifyUserVoiceState(base.Guild.Id, user.Id, args);
	}

	public Task RemoveFromSpeakerAsync(IGuildUser user, RequestOptions options = null)
	{
		ModifyVoiceStateParams args = new ModifyVoiceStateParams
		{
			ChannelId = base.Id,
			Suppressed = true
		};
		return base.Discord.ApiClient.ModifyUserVoiceState(base.Guild.Id, user.Id, args);
	}
}
