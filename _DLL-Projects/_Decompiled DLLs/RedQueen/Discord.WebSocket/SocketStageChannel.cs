using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

namespace Discord.WebSocket;

internal class SocketStageChannel : SocketVoiceChannel, IStageChannel, IVoiceChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, INestedChannel, IGuildChannel, IDeletable, IAudioChannel, IMentionable
{
	public override bool IsTextInVoice => false;

	public StagePrivacyLevel? PrivacyLevel { get; private set; }

	public bool? IsDiscoverableDisabled { get; private set; }

	public bool IsLive { get; private set; }

	public bool IsSpeaker => !base.Guild.CurrentUser.IsSuppressed;

	public IReadOnlyCollection<SocketGuildUser> Speakers => Users.Where((SocketGuildUser x) => !x.IsSuppressed).ToImmutableArray();

	internal new SocketStageChannel Clone()
	{
		return MemberwiseClone() as SocketStageChannel;
	}

	internal SocketStageChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketStageChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketStageChannel socketStageChannel = new SocketStageChannel(guild?.Discord, model.Id, guild);
		socketStageChannel.Update(state, model);
		return socketStageChannel;
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

	public async Task StartStageAsync(string topic, StagePrivacyLevel privacyLevel = StagePrivacyLevel.GuildOnly, RequestOptions options = null)
	{
		CreateStageInstanceParams args = new CreateStageInstanceParams
		{
			ChannelId = base.Id,
			Topic = topic,
			PrivacyLevel = privacyLevel
		};
		Update(await base.Discord.ApiClient.CreateStageInstanceAsync(args, options).ConfigureAwait(continueOnCapturedContext: false), isLive: true);
	}

	public async Task ModifyInstanceAsync(Action<StageInstanceProperties> func, RequestOptions options = null)
	{
		Update(await ChannelHelper.ModifyAsync(this, base.Discord, func, options), isLive: true);
	}

	public async Task StopStageAsync(RequestOptions options = null)
	{
		await base.Discord.ApiClient.DeleteStageInstanceAsync(base.Id, options);
		Update(null);
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
