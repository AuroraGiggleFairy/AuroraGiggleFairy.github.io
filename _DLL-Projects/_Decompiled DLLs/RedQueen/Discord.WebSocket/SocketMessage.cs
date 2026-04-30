using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

internal abstract class SocketMessage : SocketEntity<ulong>, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private long _timestampTicks;

	private readonly List<SocketReaction> _reactions = new List<SocketReaction>();

	private System.Collections.Immutable.ImmutableArray<SocketUser> _userMentions = System.Collections.Immutable.ImmutableArray.Create<SocketUser>();

	public SocketUser Author { get; }

	public ISocketMessageChannel Channel { get; }

	public MessageSource Source { get; }

	public string Content { get; private set; }

	public string CleanContent => MessageHelper.SanitizeMessage(this);

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public virtual bool IsTTS => false;

	public virtual bool IsPinned => false;

	public virtual bool IsSuppressed => false;

	public virtual DateTimeOffset? EditedTimestamp => null;

	public virtual bool MentionedEveryone => false;

	public MessageActivity Activity { get; private set; }

	public MessageApplication Application { get; private set; }

	public MessageReference Reference { get; private set; }

	public IReadOnlyCollection<ActionRowComponent> Components { get; private set; }

	public MessageInteraction<SocketUser> Interaction { get; private set; }

	public MessageFlags? Flags { get; private set; }

	public MessageType Type { get; private set; }

	public virtual IReadOnlyCollection<Attachment> Attachments => System.Collections.Immutable.ImmutableArray.Create<Attachment>();

	public virtual IReadOnlyCollection<Embed> Embeds => System.Collections.Immutable.ImmutableArray.Create<Embed>();

	public virtual IReadOnlyCollection<SocketGuildChannel> MentionedChannels => System.Collections.Immutable.ImmutableArray.Create<SocketGuildChannel>();

	public virtual IReadOnlyCollection<SocketRole> MentionedRoles => System.Collections.Immutable.ImmutableArray.Create<SocketRole>();

	public virtual IReadOnlyCollection<ITag> Tags => System.Collections.Immutable.ImmutableArray.Create<ITag>();

	public virtual IReadOnlyCollection<SocketSticker> Stickers => System.Collections.Immutable.ImmutableArray.Create<SocketSticker>();

	public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => (from r in _reactions
		group r by r.Emote).ToDictionary((IGrouping<IEmote, SocketReaction> x) => x.Key, (IGrouping<IEmote, SocketReaction> x) => new ReactionMetadata
	{
		ReactionCount = x.Count(),
		IsMe = x.Any((SocketReaction y) => y.UserId == base.Discord.CurrentUser.Id)
	});

	public IReadOnlyCollection<SocketUser> MentionedUsers => _userMentions;

	public DateTimeOffset Timestamp => DateTimeUtils.FromTicks(_timestampTicks);

	IUser IMessage.Author => Author;

	IMessageChannel IMessage.Channel => Channel;

	IReadOnlyCollection<IAttachment> IMessage.Attachments => Attachments;

	IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;

	IReadOnlyCollection<ulong> IMessage.MentionedChannelIds => MentionedChannels.Select((SocketGuildChannel x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<ulong> IMessage.MentionedRoleIds => MentionedRoles.Select((SocketRole x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<ulong> IMessage.MentionedUserIds => MentionedUsers.Select((SocketUser x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<IMessageComponent> IMessage.Components => Components;

	IMessageInteraction IMessage.Interaction => Interaction;

	IReadOnlyCollection<IStickerItem> IMessage.Stickers => Stickers;

	internal SocketMessage(DiscordSocketClient discord, ulong id, ISocketMessageChannel channel, SocketUser author, MessageSource source)
		: base(discord, id)
	{
		Channel = channel;
		Author = author;
		Source = source;
	}

	internal static SocketMessage Create(DiscordSocketClient discord, ClientState state, SocketUser author, ISocketMessageChannel channel, Message model)
	{
		if (model.Type == MessageType.Default || model.Type == MessageType.Reply || model.Type == MessageType.ApplicationCommand || model.Type == MessageType.ThreadStarterMessage || model.Type == MessageType.ContextMenuCommand)
		{
			return SocketUserMessage.Create(discord, state, author, channel, model);
		}
		return SocketSystemMessage.Create(discord, state, author, channel, model);
	}

	internal virtual void Update(ClientState state, Message model)
	{
		Type = model.Type;
		if (model.Timestamp.IsSpecified)
		{
			_timestampTicks = model.Timestamp.Value.UtcTicks;
		}
		if (model.Content.IsSpecified)
		{
			Content = model.Content.Value;
		}
		if (model.Application.IsSpecified)
		{
			Application = new MessageApplication
			{
				Id = model.Application.Value.Id,
				CoverImage = model.Application.Value.CoverImage,
				Description = model.Application.Value.Description,
				Icon = model.Application.Value.Icon,
				Name = model.Application.Value.Name
			};
		}
		if (model.Activity.IsSpecified)
		{
			Activity = new MessageActivity
			{
				Type = model.Activity.Value.Type.Value,
				PartyId = model.Activity.Value.PartyId.GetValueOrDefault()
			};
		}
		if (model.Reference.IsSpecified)
		{
			Reference = new MessageReference
			{
				GuildId = model.Reference.Value.GuildId,
				InternalChannelId = model.Reference.Value.ChannelId,
				MessageId = model.Reference.Value.MessageId,
				FailIfNotExists = model.Reference.Value.FailIfNotExists
			};
		}
		if (model.Components.IsSpecified)
		{
			Components = model.Components.Value.Select((global::Discord.API.ActionRowComponent x) => new ActionRowComponent(x.Components.Select(delegate(IMessageComponent y)
			{
				switch (y.Type)
				{
				case ComponentType.Button:
				{
					global::Discord.API.ButtonComponent buttonComponent = (global::Discord.API.ButtonComponent)y;
					ButtonStyle style = buttonComponent.Style;
					string valueOrDefault = buttonComponent.Label.GetValueOrDefault();
					object emote;
					if (!buttonComponent.Emote.IsSpecified)
					{
						emote = null;
					}
					else if (!buttonComponent.Emote.Value.Id.HasValue)
					{
						IEmote emote2 = new Emoji(buttonComponent.Emote.Value.Name);
						emote = emote2;
					}
					else
					{
						IEmote emote2 = new Emote(buttonComponent.Emote.Value.Id.Value, buttonComponent.Emote.Value.Name, buttonComponent.Emote.Value.Animated == true);
						emote = emote2;
					}
					return new ButtonComponent(style, valueOrDefault, (IEmote)emote, buttonComponent.CustomId.GetValueOrDefault(), buttonComponent.Url.GetValueOrDefault(), buttonComponent.Disabled.GetValueOrDefault());
				}
				case ComponentType.SelectMenu:
				{
					global::Discord.API.SelectMenuComponent selectMenuComponent = (global::Discord.API.SelectMenuComponent)y;
					return new SelectMenuComponent(selectMenuComponent.CustomId, selectMenuComponent.Options.Select(delegate(global::Discord.API.SelectMenuOption z)
					{
						string label = z.Label;
						string value2 = z.Value;
						string valueOrDefault2 = z.Description.GetValueOrDefault();
						object emote3;
						if (!z.Emoji.IsSpecified)
						{
							emote3 = null;
						}
						else if (!z.Emoji.Value.Id.HasValue)
						{
							IEmote emote4 = new Emoji(z.Emoji.Value.Name);
							emote3 = emote4;
						}
						else
						{
							IEmote emote4 = new Emote(z.Emoji.Value.Id.Value, z.Emoji.Value.Name, z.Emoji.Value.Animated == true);
							emote3 = emote4;
						}
						return new SelectMenuOption(label, value2, valueOrDefault2, (IEmote)emote3, z.Default.ToNullable());
					}).ToList(), selectMenuComponent.Placeholder.GetValueOrDefault(), selectMenuComponent.MinValues, selectMenuComponent.MaxValues, selectMenuComponent.Disabled);
				}
				default:
					return (IMessageComponent)null;
				}
			}).ToList())).ToImmutableArray();
		}
		else
		{
			Components = new List<ActionRowComponent>();
		}
		if (model.UserMentions.IsSpecified)
		{
			User[] value = model.UserMentions.Value;
			if (value.Length != 0)
			{
				System.Collections.Immutable.ImmutableArray<SocketUser>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<SocketUser>(value.Length);
				foreach (User user in value)
				{
					if (user != null)
					{
						if (Channel.GetUserAsync(user.Id, CacheMode.CacheOnly).GetAwaiter().GetResult() is SocketUser item)
						{
							builder.Add(item);
						}
						else
						{
							builder.Add(SocketUnknownUser.Create(base.Discord, state, user));
						}
					}
				}
				_userMentions = builder.ToImmutable();
			}
		}
		if (model.Interaction.IsSpecified)
		{
			Interaction = new MessageInteraction<SocketUser>(model.Interaction.Value.Id, model.Interaction.Value.Type, model.Interaction.Value.Name, SocketGlobalUser.Create(base.Discord, state, model.Interaction.Value.User));
		}
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return MessageHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Content;
	}

	internal SocketMessage Clone()
	{
		return MemberwiseClone() as SocketMessage;
	}

	internal void AddReaction(SocketReaction reaction)
	{
		_reactions.Add(reaction);
	}

	internal void RemoveReaction(SocketReaction reaction)
	{
		if (_reactions.Contains(reaction))
		{
			_reactions.Remove(reaction);
		}
	}

	internal void ClearReactions()
	{
		_reactions.Clear();
	}

	internal void RemoveReactionsForEmote(IEmote emote)
	{
		_reactions.RemoveAll((SocketReaction x) => x.Emote.Equals(emote));
	}

	public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.AddReactionAsync(this, emote, base.Discord, options);
	}

	public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
	{
		return MessageHelper.RemoveReactionAsync(this, user.Id, emote, base.Discord, options);
	}

	public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null)
	{
		return MessageHelper.RemoveReactionAsync(this, userId, emote, base.Discord, options);
	}

	public Task RemoveAllReactionsAsync(RequestOptions options = null)
	{
		return MessageHelper.RemoveAllReactionsAsync(this, base.Discord, options);
	}

	public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.RemoveAllReactionsForEmoteAsync(this, emote, base.Discord, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emote, int limit, RequestOptions options = null)
	{
		return MessageHelper.GetReactionUsersAsync(this, emote, limit, base.Discord, options);
	}
}
