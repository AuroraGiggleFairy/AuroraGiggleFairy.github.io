using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal abstract class RestMessage : RestEntity<ulong>, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable, IUpdateable
{
	private long _timestampTicks;

	private System.Collections.Immutable.ImmutableArray<RestReaction> _reactions = System.Collections.Immutable.ImmutableArray.Create<RestReaction>();

	private System.Collections.Immutable.ImmutableArray<RestUser> _userMentions = System.Collections.Immutable.ImmutableArray.Create<RestUser>();

	public IMessageChannel Channel { get; }

	public IUser Author { get; }

	public MessageSource Source { get; }

	public string Content { get; private set; }

	public string CleanContent => MessageHelper.SanitizeMessage(this);

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public virtual bool IsTTS => false;

	public virtual bool IsPinned => false;

	public virtual bool IsSuppressed => false;

	public virtual DateTimeOffset? EditedTimestamp => null;

	public virtual bool MentionedEveryone => false;

	public virtual IReadOnlyCollection<Attachment> Attachments => System.Collections.Immutable.ImmutableArray.Create<Attachment>();

	public virtual IReadOnlyCollection<Embed> Embeds => System.Collections.Immutable.ImmutableArray.Create<Embed>();

	public virtual IReadOnlyCollection<ulong> MentionedChannelIds => System.Collections.Immutable.ImmutableArray.Create<ulong>();

	public virtual IReadOnlyCollection<ulong> MentionedRoleIds => System.Collections.Immutable.ImmutableArray.Create<ulong>();

	public virtual IReadOnlyCollection<ITag> Tags => System.Collections.Immutable.ImmutableArray.Create<ITag>();

	public virtual IReadOnlyCollection<StickerItem> Stickers => System.Collections.Immutable.ImmutableArray.Create<StickerItem>();

	public DateTimeOffset Timestamp => DateTimeUtils.FromTicks(_timestampTicks);

	public MessageActivity Activity { get; private set; }

	public MessageApplication Application { get; private set; }

	public MessageReference Reference { get; private set; }

	public MessageInteraction<RestUser> Interaction { get; private set; }

	public MessageFlags? Flags { get; private set; }

	public MessageType Type { get; private set; }

	public IReadOnlyCollection<ActionRowComponent> Components { get; private set; }

	public IReadOnlyCollection<RestUser> MentionedUsers => _userMentions;

	IUser IMessage.Author => Author;

	IReadOnlyCollection<IAttachment> IMessage.Attachments => Attachments;

	IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;

	IReadOnlyCollection<ulong> IMessage.MentionedUserIds => MentionedUsers.Select((RestUser x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<IMessageComponent> IMessage.Components => Components;

	IMessageInteraction IMessage.Interaction => Interaction;

	IReadOnlyCollection<IStickerItem> IMessage.Stickers => Stickers;

	public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => _reactions.ToDictionary((RestReaction x) => x.Emote, (RestReaction x) => new ReactionMetadata
	{
		ReactionCount = x.Count,
		IsMe = x.Me
	});

	internal RestMessage(BaseDiscordClient discord, ulong id, IMessageChannel channel, IUser author, MessageSource source)
		: base(discord, id)
	{
		Channel = channel;
		Author = author;
		Source = source;
	}

	internal static RestMessage Create(BaseDiscordClient discord, IMessageChannel channel, IUser author, Message model)
	{
		if (model.Type == MessageType.Default || model.Type == MessageType.Reply || model.Type == MessageType.ApplicationCommand || model.Type == MessageType.ThreadStarterMessage)
		{
			return RestUserMessage.Create(discord, channel, author, model);
		}
		return RestSystemMessage.Create(discord, channel, author, model);
	}

	internal virtual void Update(Message model)
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
						string value3 = z.Value;
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
						return new SelectMenuOption(label, value3, valueOrDefault2, (IEmote)emote3, z.Default.ToNullable());
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
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
		if (model.Reactions.IsSpecified)
		{
			Reaction[] value = model.Reactions.Value;
			if (value.Length != 0)
			{
				System.Collections.Immutable.ImmutableArray<RestReaction>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<RestReaction>(value.Length);
				for (int num = 0; num < value.Length; num++)
				{
					builder.Add(RestReaction.Create(value[num]));
				}
				_reactions = builder.ToImmutable();
			}
			else
			{
				_reactions = System.Collections.Immutable.ImmutableArray.Create<RestReaction>();
			}
		}
		else
		{
			_reactions = System.Collections.Immutable.ImmutableArray.Create<RestReaction>();
		}
		if (model.Interaction.IsSpecified)
		{
			Interaction = new MessageInteraction<RestUser>(model.Interaction.Value.Id, model.Interaction.Value.Type, model.Interaction.Value.Name, RestUser.Create(base.Discord, model.Interaction.Value.User));
		}
		if (!model.UserMentions.IsSpecified)
		{
			return;
		}
		User[] value2 = model.UserMentions.Value;
		if (value2.Length == 0)
		{
			return;
		}
		System.Collections.Immutable.ImmutableArray<RestUser>.Builder builder2 = System.Collections.Immutable.ImmutableArray.CreateBuilder<RestUser>(value2.Length);
		foreach (User user in value2)
		{
			if (user != null)
			{
				builder2.Add(RestUser.Create(base.Discord, user));
			}
		}
		_userMentions = builder2.ToImmutable();
	}

	public async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetChannelMessageAsync(Channel.Id, base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return MessageHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Content;
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
