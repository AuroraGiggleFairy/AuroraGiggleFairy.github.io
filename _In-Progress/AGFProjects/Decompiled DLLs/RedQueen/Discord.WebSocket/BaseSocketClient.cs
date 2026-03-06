using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

internal abstract class BaseSocketClient : BaseDiscordClient, IDiscordClient, IDisposable, IAsyncDisposable
{
	protected readonly DiscordSocketConfig BaseConfig;

	internal readonly AsyncEvent<Func<SocketChannel, Task>> _channelCreatedEvent = new AsyncEvent<Func<SocketChannel, Task>>();

	internal readonly AsyncEvent<Func<SocketChannel, Task>> _channelDestroyedEvent = new AsyncEvent<Func<SocketChannel, Task>>();

	internal readonly AsyncEvent<Func<SocketChannel, SocketChannel, Task>> _channelUpdatedEvent = new AsyncEvent<Func<SocketChannel, SocketChannel, Task>>();

	internal readonly AsyncEvent<Func<SocketMessage, Task>> _messageReceivedEvent = new AsyncEvent<Func<SocketMessage, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task>> _messageDeletedEvent = new AsyncEvent<Func<Cacheable<IMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task>>();

	internal readonly AsyncEvent<Func<IReadOnlyCollection<Cacheable<IMessage, ulong>>, Cacheable<IMessageChannel, ulong>, Task>> _messagesBulkDeletedEvent = new AsyncEvent<Func<IReadOnlyCollection<Cacheable<IMessage, ulong>>, Cacheable<IMessageChannel, ulong>, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task>> _messageUpdatedEvent = new AsyncEvent<Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task>> _reactionAddedEvent = new AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task>> _reactionRemovedEvent = new AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task>> _reactionsClearedEvent = new AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, IEmote, Task>> _reactionsRemovedForEmoteEvent = new AsyncEvent<Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, IEmote, Task>>();

	internal readonly AsyncEvent<Func<SocketRole, Task>> _roleCreatedEvent = new AsyncEvent<Func<SocketRole, Task>>();

	internal readonly AsyncEvent<Func<SocketRole, Task>> _roleDeletedEvent = new AsyncEvent<Func<SocketRole, Task>>();

	internal readonly AsyncEvent<Func<SocketRole, SocketRole, Task>> _roleUpdatedEvent = new AsyncEvent<Func<SocketRole, SocketRole, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, Task>> _joinedGuildEvent = new AsyncEvent<Func<SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, Task>> _leftGuildEvent = new AsyncEvent<Func<SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, Task>> _guildAvailableEvent = new AsyncEvent<Func<SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, Task>> _guildUnavailableEvent = new AsyncEvent<Func<SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, Task>> _guildMembersDownloadedEvent = new AsyncEvent<Func<SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, SocketGuild, Task>> _guildUpdatedEvent = new AsyncEvent<Func<SocketGuild, SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketGuildUser, ulong>, SocketGuild, Task>> _guildJoinRequestDeletedEvent = new AsyncEvent<Func<Cacheable<SocketGuildUser, ulong>, SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketGuildEvent, Task>> _guildScheduledEventCreated = new AsyncEvent<Func<SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketGuildEvent, ulong>, SocketGuildEvent, Task>> _guildScheduledEventUpdated = new AsyncEvent<Func<Cacheable<SocketGuildEvent, ulong>, SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<SocketGuildEvent, Task>> _guildScheduledEventCancelled = new AsyncEvent<Func<SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<SocketGuildEvent, Task>> _guildScheduledEventCompleted = new AsyncEvent<Func<SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<SocketGuildEvent, Task>> _guildScheduledEventStarted = new AsyncEvent<Func<SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketUser, RestUser, IUser, ulong>, SocketGuildEvent, Task>> _guildScheduledEventUserAdd = new AsyncEvent<Func<Cacheable<SocketUser, RestUser, IUser, ulong>, SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketUser, RestUser, IUser, ulong>, SocketGuildEvent, Task>> _guildScheduledEventUserRemove = new AsyncEvent<Func<Cacheable<SocketUser, RestUser, IUser, ulong>, SocketGuildEvent, Task>>();

	internal readonly AsyncEvent<Func<IIntegration, Task>> _integrationCreated = new AsyncEvent<Func<IIntegration, Task>>();

	internal readonly AsyncEvent<Func<IIntegration, Task>> _integrationUpdated = new AsyncEvent<Func<IIntegration, Task>>();

	internal readonly AsyncEvent<Func<IGuild, ulong, Optional<ulong>, Task>> _integrationDeleted = new AsyncEvent<Func<IGuild, ulong, Optional<ulong>, Task>>();

	internal readonly AsyncEvent<Func<SocketGuildUser, Task>> _userJoinedEvent = new AsyncEvent<Func<SocketGuildUser, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, SocketUser, Task>> _userLeftEvent = new AsyncEvent<Func<SocketGuild, SocketUser, Task>>();

	internal readonly AsyncEvent<Func<SocketUser, SocketGuild, Task>> _userBannedEvent = new AsyncEvent<Func<SocketUser, SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketUser, SocketGuild, Task>> _userUnbannedEvent = new AsyncEvent<Func<SocketUser, SocketGuild, Task>>();

	internal readonly AsyncEvent<Func<SocketUser, SocketUser, Task>> _userUpdatedEvent = new AsyncEvent<Func<SocketUser, SocketUser, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketGuildUser, ulong>, SocketGuildUser, Task>> _guildMemberUpdatedEvent = new AsyncEvent<Func<Cacheable<SocketGuildUser, ulong>, SocketGuildUser, Task>>();

	internal readonly AsyncEvent<Func<SocketUser, SocketVoiceState, SocketVoiceState, Task>> _userVoiceStateUpdatedEvent = new AsyncEvent<Func<SocketUser, SocketVoiceState, SocketVoiceState, Task>>();

	internal readonly AsyncEvent<Func<SocketVoiceServer, Task>> _voiceServerUpdatedEvent = new AsyncEvent<Func<SocketVoiceServer, Task>>();

	internal readonly AsyncEvent<Func<SocketSelfUser, SocketSelfUser, Task>> _selfUpdatedEvent = new AsyncEvent<Func<SocketSelfUser, SocketSelfUser, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<IUser, ulong>, Cacheable<IMessageChannel, ulong>, Task>> _userIsTypingEvent = new AsyncEvent<Func<Cacheable<IUser, ulong>, Cacheable<IMessageChannel, ulong>, Task>>();

	internal readonly AsyncEvent<Func<SocketGroupUser, Task>> _recipientAddedEvent = new AsyncEvent<Func<SocketGroupUser, Task>>();

	internal readonly AsyncEvent<Func<SocketGroupUser, Task>> _recipientRemovedEvent = new AsyncEvent<Func<SocketGroupUser, Task>>();

	internal readonly AsyncEvent<Func<SocketUser, SocketPresence, SocketPresence, Task>> _presenceUpdated = new AsyncEvent<Func<SocketUser, SocketPresence, SocketPresence, Task>>();

	internal readonly AsyncEvent<Func<SocketInvite, Task>> _inviteCreatedEvent = new AsyncEvent<Func<SocketInvite, Task>>();

	internal readonly AsyncEvent<Func<SocketGuildChannel, string, Task>> _inviteDeletedEvent = new AsyncEvent<Func<SocketGuildChannel, string, Task>>();

	internal readonly AsyncEvent<Func<SocketInteraction, Task>> _interactionCreatedEvent = new AsyncEvent<Func<SocketInteraction, Task>>();

	internal readonly AsyncEvent<Func<SocketMessageComponent, Task>> _buttonExecuted = new AsyncEvent<Func<SocketMessageComponent, Task>>();

	internal readonly AsyncEvent<Func<SocketMessageComponent, Task>> _selectMenuExecuted = new AsyncEvent<Func<SocketMessageComponent, Task>>();

	internal readonly AsyncEvent<Func<SocketSlashCommand, Task>> _slashCommandExecuted = new AsyncEvent<Func<SocketSlashCommand, Task>>();

	internal readonly AsyncEvent<Func<SocketUserCommand, Task>> _userCommandExecuted = new AsyncEvent<Func<SocketUserCommand, Task>>();

	internal readonly AsyncEvent<Func<SocketMessageCommand, Task>> _messageCommandExecuted = new AsyncEvent<Func<SocketMessageCommand, Task>>();

	internal readonly AsyncEvent<Func<SocketAutocompleteInteraction, Task>> _autocompleteExecuted = new AsyncEvent<Func<SocketAutocompleteInteraction, Task>>();

	internal readonly AsyncEvent<Func<SocketModal, Task>> _modalSubmitted = new AsyncEvent<Func<SocketModal, Task>>();

	internal readonly AsyncEvent<Func<SocketApplicationCommand, Task>> _applicationCommandCreated = new AsyncEvent<Func<SocketApplicationCommand, Task>>();

	internal readonly AsyncEvent<Func<SocketApplicationCommand, Task>> _applicationCommandUpdated = new AsyncEvent<Func<SocketApplicationCommand, Task>>();

	internal readonly AsyncEvent<Func<SocketApplicationCommand, Task>> _applicationCommandDeleted = new AsyncEvent<Func<SocketApplicationCommand, Task>>();

	internal readonly AsyncEvent<Func<SocketThreadChannel, Task>> _threadCreated = new AsyncEvent<Func<SocketThreadChannel, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketThreadChannel, ulong>, SocketThreadChannel, Task>> _threadUpdated = new AsyncEvent<Func<Cacheable<SocketThreadChannel, ulong>, SocketThreadChannel, Task>>();

	internal readonly AsyncEvent<Func<Cacheable<SocketThreadChannel, ulong>, Task>> _threadDeleted = new AsyncEvent<Func<Cacheable<SocketThreadChannel, ulong>, Task>>();

	internal readonly AsyncEvent<Func<SocketThreadUser, Task>> _threadMemberJoined = new AsyncEvent<Func<SocketThreadUser, Task>>();

	internal readonly AsyncEvent<Func<SocketThreadUser, Task>> _threadMemberLeft = new AsyncEvent<Func<SocketThreadUser, Task>>();

	internal readonly AsyncEvent<Func<SocketStageChannel, Task>> _stageStarted = new AsyncEvent<Func<SocketStageChannel, Task>>();

	internal readonly AsyncEvent<Func<SocketStageChannel, Task>> _stageEnded = new AsyncEvent<Func<SocketStageChannel, Task>>();

	internal readonly AsyncEvent<Func<SocketStageChannel, SocketStageChannel, Task>> _stageUpdated = new AsyncEvent<Func<SocketStageChannel, SocketStageChannel, Task>>();

	internal readonly AsyncEvent<Func<SocketStageChannel, SocketGuildUser, Task>> _requestToSpeak = new AsyncEvent<Func<SocketStageChannel, SocketGuildUser, Task>>();

	internal readonly AsyncEvent<Func<SocketStageChannel, SocketGuildUser, Task>> _speakerAdded = new AsyncEvent<Func<SocketStageChannel, SocketGuildUser, Task>>();

	internal readonly AsyncEvent<Func<SocketStageChannel, SocketGuildUser, Task>> _speakerRemoved = new AsyncEvent<Func<SocketStageChannel, SocketGuildUser, Task>>();

	internal readonly AsyncEvent<Func<SocketCustomSticker, Task>> _guildStickerCreated = new AsyncEvent<Func<SocketCustomSticker, Task>>();

	internal readonly AsyncEvent<Func<SocketCustomSticker, SocketCustomSticker, Task>> _guildStickerUpdated = new AsyncEvent<Func<SocketCustomSticker, SocketCustomSticker, Task>>();

	internal readonly AsyncEvent<Func<SocketCustomSticker, Task>> _guildStickerDeleted = new AsyncEvent<Func<SocketCustomSticker, Task>>();

	internal readonly AsyncEvent<Func<SocketGuild, SocketChannel, Task>> _webhooksUpdated = new AsyncEvent<Func<SocketGuild, SocketChannel, Task>>();

	public abstract int Latency { get; protected set; }

	public abstract UserStatus Status { get; protected set; }

	public abstract IActivity Activity { get; protected set; }

	public abstract DiscordSocketRestClient Rest { get; }

	internal new DiscordSocketApiClient ApiClient => base.ApiClient as DiscordSocketApiClient;

	public abstract IReadOnlyCollection<StickerPack<SocketSticker>> DefaultStickerPacks { get; }

	public new virtual SocketSelfUser CurrentUser
	{
		get
		{
			return base.CurrentUser as SocketSelfUser;
		}
		protected set
		{
			base.CurrentUser = value;
		}
	}

	public abstract IReadOnlyCollection<SocketGuild> Guilds { get; }

	public abstract IReadOnlyCollection<ISocketPrivateChannel> PrivateChannels { get; }

	public event Func<SocketChannel, Task> ChannelCreated
	{
		add
		{
			_channelCreatedEvent.Add(value);
		}
		remove
		{
			_channelCreatedEvent.Remove(value);
		}
	}

	public event Func<SocketChannel, Task> ChannelDestroyed
	{
		add
		{
			_channelDestroyedEvent.Add(value);
		}
		remove
		{
			_channelDestroyedEvent.Remove(value);
		}
	}

	public event Func<SocketChannel, SocketChannel, Task> ChannelUpdated
	{
		add
		{
			_channelUpdatedEvent.Add(value);
		}
		remove
		{
			_channelUpdatedEvent.Remove(value);
		}
	}

	public event Func<SocketMessage, Task> MessageReceived
	{
		add
		{
			_messageReceivedEvent.Add(value);
		}
		remove
		{
			_messageReceivedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task> MessageDeleted
	{
		add
		{
			_messageDeletedEvent.Add(value);
		}
		remove
		{
			_messageDeletedEvent.Remove(value);
		}
	}

	public event Func<IReadOnlyCollection<Cacheable<IMessage, ulong>>, Cacheable<IMessageChannel, ulong>, Task> MessagesBulkDeleted
	{
		add
		{
			_messagesBulkDeletedEvent.Add(value);
		}
		remove
		{
			_messagesBulkDeletedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task> MessageUpdated
	{
		add
		{
			_messageUpdatedEvent.Add(value);
		}
		remove
		{
			_messageUpdatedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> ReactionAdded
	{
		add
		{
			_reactionAddedEvent.Add(value);
		}
		remove
		{
			_reactionAddedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> ReactionRemoved
	{
		add
		{
			_reactionRemovedEvent.Add(value);
		}
		remove
		{
			_reactionRemovedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task> ReactionsCleared
	{
		add
		{
			_reactionsClearedEvent.Add(value);
		}
		remove
		{
			_reactionsClearedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, IEmote, Task> ReactionsRemovedForEmote
	{
		add
		{
			_reactionsRemovedForEmoteEvent.Add(value);
		}
		remove
		{
			_reactionsRemovedForEmoteEvent.Remove(value);
		}
	}

	public event Func<SocketRole, Task> RoleCreated
	{
		add
		{
			_roleCreatedEvent.Add(value);
		}
		remove
		{
			_roleCreatedEvent.Remove(value);
		}
	}

	public event Func<SocketRole, Task> RoleDeleted
	{
		add
		{
			_roleDeletedEvent.Add(value);
		}
		remove
		{
			_roleDeletedEvent.Remove(value);
		}
	}

	public event Func<SocketRole, SocketRole, Task> RoleUpdated
	{
		add
		{
			_roleUpdatedEvent.Add(value);
		}
		remove
		{
			_roleUpdatedEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, Task> JoinedGuild
	{
		add
		{
			_joinedGuildEvent.Add(value);
		}
		remove
		{
			_joinedGuildEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, Task> LeftGuild
	{
		add
		{
			_leftGuildEvent.Add(value);
		}
		remove
		{
			_leftGuildEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, Task> GuildAvailable
	{
		add
		{
			_guildAvailableEvent.Add(value);
		}
		remove
		{
			_guildAvailableEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, Task> GuildUnavailable
	{
		add
		{
			_guildUnavailableEvent.Add(value);
		}
		remove
		{
			_guildUnavailableEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, Task> GuildMembersDownloaded
	{
		add
		{
			_guildMembersDownloadedEvent.Add(value);
		}
		remove
		{
			_guildMembersDownloadedEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, SocketGuild, Task> GuildUpdated
	{
		add
		{
			_guildUpdatedEvent.Add(value);
		}
		remove
		{
			_guildUpdatedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<SocketGuildUser, ulong>, SocketGuild, Task> GuildJoinRequestDeleted
	{
		add
		{
			_guildJoinRequestDeletedEvent.Add(value);
		}
		remove
		{
			_guildJoinRequestDeletedEvent.Remove(value);
		}
	}

	public event Func<SocketGuildEvent, Task> GuildScheduledEventCreated
	{
		add
		{
			_guildScheduledEventCreated.Add(value);
		}
		remove
		{
			_guildScheduledEventCreated.Remove(value);
		}
	}

	public event Func<Cacheable<SocketGuildEvent, ulong>, SocketGuildEvent, Task> GuildScheduledEventUpdated
	{
		add
		{
			_guildScheduledEventUpdated.Add(value);
		}
		remove
		{
			_guildScheduledEventUpdated.Remove(value);
		}
	}

	public event Func<SocketGuildEvent, Task> GuildScheduledEventCancelled
	{
		add
		{
			_guildScheduledEventCancelled.Add(value);
		}
		remove
		{
			_guildScheduledEventCancelled.Remove(value);
		}
	}

	public event Func<SocketGuildEvent, Task> GuildScheduledEventCompleted
	{
		add
		{
			_guildScheduledEventCompleted.Add(value);
		}
		remove
		{
			_guildScheduledEventCompleted.Remove(value);
		}
	}

	public event Func<SocketGuildEvent, Task> GuildScheduledEventStarted
	{
		add
		{
			_guildScheduledEventStarted.Add(value);
		}
		remove
		{
			_guildScheduledEventStarted.Remove(value);
		}
	}

	public event Func<Cacheable<SocketUser, RestUser, IUser, ulong>, SocketGuildEvent, Task> GuildScheduledEventUserAdd
	{
		add
		{
			_guildScheduledEventUserAdd.Add(value);
		}
		remove
		{
			_guildScheduledEventUserAdd.Remove(value);
		}
	}

	public event Func<Cacheable<SocketUser, RestUser, IUser, ulong>, SocketGuildEvent, Task> GuildScheduledEventUserRemove
	{
		add
		{
			_guildScheduledEventUserRemove.Add(value);
		}
		remove
		{
			_guildScheduledEventUserRemove.Remove(value);
		}
	}

	public event Func<IIntegration, Task> IntegrationCreated
	{
		add
		{
			_integrationCreated.Add(value);
		}
		remove
		{
			_integrationCreated.Remove(value);
		}
	}

	public event Func<IIntegration, Task> IntegrationUpdated
	{
		add
		{
			_integrationUpdated.Add(value);
		}
		remove
		{
			_integrationUpdated.Remove(value);
		}
	}

	public event Func<IGuild, ulong, Optional<ulong>, Task> IntegrationDeleted
	{
		add
		{
			_integrationDeleted.Add(value);
		}
		remove
		{
			_integrationDeleted.Remove(value);
		}
	}

	public event Func<SocketGuildUser, Task> UserJoined
	{
		add
		{
			_userJoinedEvent.Add(value);
		}
		remove
		{
			_userJoinedEvent.Remove(value);
		}
	}

	public event Func<SocketGuild, SocketUser, Task> UserLeft
	{
		add
		{
			_userLeftEvent.Add(value);
		}
		remove
		{
			_userLeftEvent.Remove(value);
		}
	}

	public event Func<SocketUser, SocketGuild, Task> UserBanned
	{
		add
		{
			_userBannedEvent.Add(value);
		}
		remove
		{
			_userBannedEvent.Remove(value);
		}
	}

	public event Func<SocketUser, SocketGuild, Task> UserUnbanned
	{
		add
		{
			_userUnbannedEvent.Add(value);
		}
		remove
		{
			_userUnbannedEvent.Remove(value);
		}
	}

	public event Func<SocketUser, SocketUser, Task> UserUpdated
	{
		add
		{
			_userUpdatedEvent.Add(value);
		}
		remove
		{
			_userUpdatedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<SocketGuildUser, ulong>, SocketGuildUser, Task> GuildMemberUpdated
	{
		add
		{
			_guildMemberUpdatedEvent.Add(value);
		}
		remove
		{
			_guildMemberUpdatedEvent.Remove(value);
		}
	}

	public event Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated
	{
		add
		{
			_userVoiceStateUpdatedEvent.Add(value);
		}
		remove
		{
			_userVoiceStateUpdatedEvent.Remove(value);
		}
	}

	public event Func<SocketVoiceServer, Task> VoiceServerUpdated
	{
		add
		{
			_voiceServerUpdatedEvent.Add(value);
		}
		remove
		{
			_voiceServerUpdatedEvent.Remove(value);
		}
	}

	public event Func<SocketSelfUser, SocketSelfUser, Task> CurrentUserUpdated
	{
		add
		{
			_selfUpdatedEvent.Add(value);
		}
		remove
		{
			_selfUpdatedEvent.Remove(value);
		}
	}

	public event Func<Cacheable<IUser, ulong>, Cacheable<IMessageChannel, ulong>, Task> UserIsTyping
	{
		add
		{
			_userIsTypingEvent.Add(value);
		}
		remove
		{
			_userIsTypingEvent.Remove(value);
		}
	}

	public event Func<SocketGroupUser, Task> RecipientAdded
	{
		add
		{
			_recipientAddedEvent.Add(value);
		}
		remove
		{
			_recipientAddedEvent.Remove(value);
		}
	}

	public event Func<SocketGroupUser, Task> RecipientRemoved
	{
		add
		{
			_recipientRemovedEvent.Add(value);
		}
		remove
		{
			_recipientRemovedEvent.Remove(value);
		}
	}

	public event Func<SocketUser, SocketPresence, SocketPresence, Task> PresenceUpdated
	{
		add
		{
			_presenceUpdated.Add(value);
		}
		remove
		{
			_presenceUpdated.Remove(value);
		}
	}

	public event Func<SocketInvite, Task> InviteCreated
	{
		add
		{
			_inviteCreatedEvent.Add(value);
		}
		remove
		{
			_inviteCreatedEvent.Remove(value);
		}
	}

	public event Func<SocketGuildChannel, string, Task> InviteDeleted
	{
		add
		{
			_inviteDeletedEvent.Add(value);
		}
		remove
		{
			_inviteDeletedEvent.Remove(value);
		}
	}

	public event Func<SocketInteraction, Task> InteractionCreated
	{
		add
		{
			_interactionCreatedEvent.Add(value);
		}
		remove
		{
			_interactionCreatedEvent.Remove(value);
		}
	}

	public event Func<SocketMessageComponent, Task> ButtonExecuted
	{
		add
		{
			_buttonExecuted.Add(value);
		}
		remove
		{
			_buttonExecuted.Remove(value);
		}
	}

	public event Func<SocketMessageComponent, Task> SelectMenuExecuted
	{
		add
		{
			_selectMenuExecuted.Add(value);
		}
		remove
		{
			_selectMenuExecuted.Remove(value);
		}
	}

	public event Func<SocketSlashCommand, Task> SlashCommandExecuted
	{
		add
		{
			_slashCommandExecuted.Add(value);
		}
		remove
		{
			_slashCommandExecuted.Remove(value);
		}
	}

	public event Func<SocketUserCommand, Task> UserCommandExecuted
	{
		add
		{
			_userCommandExecuted.Add(value);
		}
		remove
		{
			_userCommandExecuted.Remove(value);
		}
	}

	public event Func<SocketMessageCommand, Task> MessageCommandExecuted
	{
		add
		{
			_messageCommandExecuted.Add(value);
		}
		remove
		{
			_messageCommandExecuted.Remove(value);
		}
	}

	public event Func<SocketAutocompleteInteraction, Task> AutocompleteExecuted
	{
		add
		{
			_autocompleteExecuted.Add(value);
		}
		remove
		{
			_autocompleteExecuted.Remove(value);
		}
	}

	public event Func<SocketModal, Task> ModalSubmitted
	{
		add
		{
			_modalSubmitted.Add(value);
		}
		remove
		{
			_modalSubmitted.Remove(value);
		}
	}

	public event Func<SocketApplicationCommand, Task> ApplicationCommandCreated
	{
		add
		{
			_applicationCommandCreated.Add(value);
		}
		remove
		{
			_applicationCommandCreated.Remove(value);
		}
	}

	public event Func<SocketApplicationCommand, Task> ApplicationCommandUpdated
	{
		add
		{
			_applicationCommandUpdated.Add(value);
		}
		remove
		{
			_applicationCommandUpdated.Remove(value);
		}
	}

	public event Func<SocketApplicationCommand, Task> ApplicationCommandDeleted
	{
		add
		{
			_applicationCommandDeleted.Add(value);
		}
		remove
		{
			_applicationCommandDeleted.Remove(value);
		}
	}

	public event Func<SocketThreadChannel, Task> ThreadCreated
	{
		add
		{
			_threadCreated.Add(value);
		}
		remove
		{
			_threadCreated.Remove(value);
		}
	}

	public event Func<Cacheable<SocketThreadChannel, ulong>, SocketThreadChannel, Task> ThreadUpdated
	{
		add
		{
			_threadUpdated.Add(value);
		}
		remove
		{
			_threadUpdated.Remove(value);
		}
	}

	public event Func<Cacheable<SocketThreadChannel, ulong>, Task> ThreadDeleted
	{
		add
		{
			_threadDeleted.Add(value);
		}
		remove
		{
			_threadDeleted.Remove(value);
		}
	}

	public event Func<SocketThreadUser, Task> ThreadMemberJoined
	{
		add
		{
			_threadMemberJoined.Add(value);
		}
		remove
		{
			_threadMemberJoined.Remove(value);
		}
	}

	public event Func<SocketThreadUser, Task> ThreadMemberLeft
	{
		add
		{
			_threadMemberLeft.Add(value);
		}
		remove
		{
			_threadMemberLeft.Remove(value);
		}
	}

	public event Func<SocketStageChannel, Task> StageStarted
	{
		add
		{
			_stageStarted.Add(value);
		}
		remove
		{
			_stageStarted.Remove(value);
		}
	}

	public event Func<SocketStageChannel, Task> StageEnded
	{
		add
		{
			_stageEnded.Add(value);
		}
		remove
		{
			_stageEnded.Remove(value);
		}
	}

	public event Func<SocketStageChannel, SocketStageChannel, Task> StageUpdated
	{
		add
		{
			_stageUpdated.Add(value);
		}
		remove
		{
			_stageUpdated.Remove(value);
		}
	}

	public event Func<SocketStageChannel, SocketGuildUser, Task> RequestToSpeak
	{
		add
		{
			_requestToSpeak.Add(value);
		}
		remove
		{
			_requestToSpeak.Remove(value);
		}
	}

	public event Func<SocketStageChannel, SocketGuildUser, Task> SpeakerAdded
	{
		add
		{
			_speakerAdded.Add(value);
		}
		remove
		{
			_speakerAdded.Remove(value);
		}
	}

	public event Func<SocketStageChannel, SocketGuildUser, Task> SpeakerRemoved
	{
		add
		{
			_speakerRemoved.Add(value);
		}
		remove
		{
			_speakerRemoved.Remove(value);
		}
	}

	public event Func<SocketCustomSticker, Task> GuildStickerCreated
	{
		add
		{
			_guildStickerCreated.Add(value);
		}
		remove
		{
			_guildStickerCreated.Remove(value);
		}
	}

	public event Func<SocketCustomSticker, SocketCustomSticker, Task> GuildStickerUpdated
	{
		add
		{
			_guildStickerUpdated.Add(value);
		}
		remove
		{
			_guildStickerUpdated.Remove(value);
		}
	}

	public event Func<SocketCustomSticker, Task> GuildStickerDeleted
	{
		add
		{
			_guildStickerDeleted.Add(value);
		}
		remove
		{
			_guildStickerDeleted.Remove(value);
		}
	}

	public event Func<SocketGuild, SocketChannel, Task> WebhooksUpdated
	{
		add
		{
			_webhooksUpdated.Add(value);
		}
		remove
		{
			_webhooksUpdated.Remove(value);
		}
	}

	internal BaseSocketClient(DiscordSocketConfig config, DiscordRestApiClient client)
		: base(config, client)
	{
		BaseConfig = config;
	}

	private static DiscordSocketApiClient CreateApiClient(DiscordSocketConfig config)
	{
		return new DiscordSocketApiClient(config.RestClientProvider, config.WebSocketProvider, DiscordConfig.UserAgent, config.GatewayHost, RetryMode.AlwaysRetry, null, config.UseSystemClock, config.DefaultRatelimitCallback);
	}

	public abstract Task<RestApplication> GetApplicationInfoAsync(RequestOptions options = null);

	public abstract SocketUser GetUser(ulong id);

	public abstract SocketUser GetUser(string username, string discriminator);

	public abstract SocketChannel GetChannel(ulong id);

	public abstract SocketGuild GetGuild(ulong id);

	public abstract ValueTask<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null);

	public abstract ValueTask<RestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null);

	public abstract Task StartAsync();

	public abstract Task StopAsync();

	public abstract Task SetStatusAsync(UserStatus status);

	public abstract Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing);

	public abstract Task SetActivityAsync(IActivity activity);

	public abstract Task DownloadUsersAsync(IEnumerable<IGuild> guilds);

	public Task<RestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null)
	{
		return ClientHelper.CreateGuildAsync(this, name, region, jpegIcon, options ?? RequestOptions.Default);
	}

	public Task<IReadOnlyCollection<RestConnection>> GetConnectionsAsync(RequestOptions options = null)
	{
		return ClientHelper.GetConnectionsAsync(this, options ?? RequestOptions.Default);
	}

	public Task<RestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null)
	{
		return ClientHelper.GetInviteAsync(this, inviteId, options ?? RequestOptions.Default);
	}

	public abstract Task<SocketSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	async Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
	{
		return await GetApplicationInfoAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IChannel)GetChannel(id));
	}

	Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IPrivateChannel>)PrivateChannels);
	}

	async Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
	{
		return await GetConnectionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
	{
		return await GetInviteAsync(inviteId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuild)GetGuild(id));
	}

	Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGuild>)Guilds);
	}

	async Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		return await CreateGuildAsync(name, region, jpegIcon, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await Rest.GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IUser> IDiscordClient.GetUserAsync(string username, string discriminator, RequestOptions options)
	{
		return Task.FromResult((IUser)GetUser(username, discriminator));
	}

	async Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return await GetVoiceRegionAsync(id).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
	{
		return await GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}
}
