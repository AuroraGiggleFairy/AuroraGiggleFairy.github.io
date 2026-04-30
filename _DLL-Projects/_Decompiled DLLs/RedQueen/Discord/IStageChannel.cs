using System;
using System.Threading.Tasks;

namespace Discord;

internal interface IStageChannel : IVoiceChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, INestedChannel, IGuildChannel, IDeletable, IAudioChannel, IMentionable
{
	string Topic { get; }

	StagePrivacyLevel? PrivacyLevel { get; }

	bool? IsDiscoverableDisabled { get; }

	bool IsLive { get; }

	Task StartStageAsync(string topic, StagePrivacyLevel privacyLevel = StagePrivacyLevel.GuildOnly, RequestOptions options = null);

	Task ModifyInstanceAsync(Action<StageInstanceProperties> func, RequestOptions options = null);

	Task StopStageAsync(RequestOptions options = null);

	Task RequestToSpeakAsync(RequestOptions options = null);

	Task BecomeSpeakerAsync(RequestOptions options = null);

	Task StopSpeakingAsync(RequestOptions options = null);

	Task MoveToSpeakerAsync(IGuildUser user, RequestOptions options = null);

	Task RemoveFromSpeakerAsync(IGuildUser user, RequestOptions options = null);
}
