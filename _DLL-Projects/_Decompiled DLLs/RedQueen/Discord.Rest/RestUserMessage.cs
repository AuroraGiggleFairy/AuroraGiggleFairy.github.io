using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestUserMessage : RestMessage, IUserMessage, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private bool _isMentioningEveryone;

	private bool _isTTS;

	private bool _isPinned;

	private long? _editedTimestampTicks;

	private IUserMessage _referencedMessage;

	private System.Collections.Immutable.ImmutableArray<Attachment> _attachments = System.Collections.Immutable.ImmutableArray.Create<Attachment>();

	private System.Collections.Immutable.ImmutableArray<Embed> _embeds = System.Collections.Immutable.ImmutableArray.Create<Embed>();

	private System.Collections.Immutable.ImmutableArray<ITag> _tags = System.Collections.Immutable.ImmutableArray.Create<ITag>();

	private System.Collections.Immutable.ImmutableArray<ulong> _roleMentionIds = System.Collections.Immutable.ImmutableArray.Create<ulong>();

	private System.Collections.Immutable.ImmutableArray<StickerItem> _stickers = System.Collections.Immutable.ImmutableArray.Create<StickerItem>();

	public override bool IsTTS => _isTTS;

	public override bool IsPinned => _isPinned;

	public override bool IsSuppressed
	{
		get
		{
			if (base.Flags.HasValue)
			{
				return base.Flags.Value.HasFlag(MessageFlags.SuppressEmbeds);
			}
			return false;
		}
	}

	public override DateTimeOffset? EditedTimestamp => DateTimeUtils.FromTicks(_editedTimestampTicks);

	public override bool MentionedEveryone => _isMentioningEveryone;

	public override IReadOnlyCollection<Attachment> Attachments => _attachments;

	public override IReadOnlyCollection<Embed> Embeds => _embeds;

	public override IReadOnlyCollection<ulong> MentionedChannelIds => MessageHelper.FilterTagsByKey(TagType.ChannelMention, _tags);

	public override IReadOnlyCollection<ulong> MentionedRoleIds => _roleMentionIds;

	public override IReadOnlyCollection<ITag> Tags => _tags;

	public override IReadOnlyCollection<StickerItem> Stickers => _stickers;

	public IUserMessage ReferencedMessage => _referencedMessage;

	private string DebuggerDisplay => string.Format("{0}: {1} ({2}{3})", base.Author, base.Content, base.Id, (Attachments.Count > 0) ? $", {Attachments.Count} Attachments" : "");

	internal RestUserMessage(BaseDiscordClient discord, ulong id, IMessageChannel channel, IUser author, MessageSource source)
		: base(discord, id, channel, author, source)
	{
	}

	internal new static RestUserMessage Create(BaseDiscordClient discord, IMessageChannel channel, IUser author, Message model)
	{
		RestUserMessage restUserMessage = new RestUserMessage(discord, model.Id, channel, author, MessageHelper.GetSource(model));
		restUserMessage.Update(model);
		return restUserMessage;
	}

	internal override void Update(Message model)
	{
		base.Update(model);
		if (model.IsTextToSpeech.IsSpecified)
		{
			_isTTS = model.IsTextToSpeech.Value;
		}
		if (model.Pinned.IsSpecified)
		{
			_isPinned = model.Pinned.Value;
		}
		if (model.EditedTimestamp.IsSpecified)
		{
			_editedTimestampTicks = model.EditedTimestamp.Value?.UtcTicks;
		}
		if (model.MentionEveryone.IsSpecified)
		{
			_isMentioningEveryone = model.MentionEveryone.Value;
		}
		if (model.RoleMentions.IsSpecified)
		{
			_roleMentionIds = model.RoleMentions.Value.ToImmutableArray();
		}
		if (model.Attachments.IsSpecified)
		{
			global::Discord.API.Attachment[] value = model.Attachments.Value;
			if (value.Length != 0)
			{
				System.Collections.Immutable.ImmutableArray<Attachment>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<Attachment>(value.Length);
				for (int i = 0; i < value.Length; i++)
				{
					builder.Add(Attachment.Create(value[i]));
				}
				_attachments = builder.ToImmutable();
			}
			else
			{
				_attachments = System.Collections.Immutable.ImmutableArray.Create<Attachment>();
			}
		}
		if (model.Embeds.IsSpecified)
		{
			global::Discord.API.Embed[] value2 = model.Embeds.Value;
			if (value2.Length != 0)
			{
				System.Collections.Immutable.ImmutableArray<Embed>.Builder builder2 = System.Collections.Immutable.ImmutableArray.CreateBuilder<Embed>(value2.Length);
				for (int j = 0; j < value2.Length; j++)
				{
					builder2.Add(value2[j].ToEntity());
				}
				_embeds = builder2.ToImmutable();
			}
			else
			{
				_embeds = System.Collections.Immutable.ImmutableArray.Create<Embed>();
			}
		}
		ulong? num = (base.Channel as IGuildChannel)?.GuildId;
		IGuild guild = (num.HasValue ? ((IDiscordClient)base.Discord).GetGuildAsync(num.Value, CacheMode.CacheOnly, (RequestOptions)null).Result : null);
		if (model.Content.IsSpecified)
		{
			string value3 = model.Content.Value;
			_tags = MessageHelper.ParseTags(value3, null, guild, base.MentionedUsers);
			model.Content = value3;
		}
		if (model.ReferencedMessage.IsSpecified && model.ReferencedMessage.Value != null)
		{
			Message value4 = model.ReferencedMessage.Value;
			IUser author = MessageHelper.GetAuthor(base.Discord, guild, value4.Author.Value, value4.WebhookId.ToNullable());
			_referencedMessage = Create(base.Discord, base.Channel, author, value4);
		}
		if (!model.StickerItems.IsSpecified)
		{
			return;
		}
		global::Discord.API.StickerItem[] value5 = model.StickerItems.Value;
		if (value5.Length != 0)
		{
			System.Collections.Immutable.ImmutableArray<StickerItem>.Builder builder3 = System.Collections.Immutable.ImmutableArray.CreateBuilder<StickerItem>(value5.Length);
			for (int k = 0; k < value5.Length; k++)
			{
				builder3.Add(new StickerItem(base.Discord, value5[k]));
			}
			_stickers = builder3.ToImmutable();
		}
		else
		{
			_stickers = System.Collections.Immutable.ImmutableArray.Create<StickerItem>();
		}
	}

	public async Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		Update(await MessageHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task PinAsync(RequestOptions options = null)
	{
		return MessageHelper.PinAsync(this, base.Discord, options);
	}

	public Task UnpinAsync(RequestOptions options = null)
	{
		return MessageHelper.UnpinAsync(this, base.Discord, options);
	}

	public string Resolve(int startIndex, TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
	{
		return MentionUtils.Resolve(this, startIndex, userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);
	}

	public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
	{
		return MentionUtils.Resolve(this, 0, userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);
	}

	public async Task CrosspostAsync(RequestOptions options = null)
	{
		if (!(base.Channel is INewsChannel))
		{
			throw new InvalidOperationException("Publishing (crossposting) is only valid in news channels.");
		}
		await MessageHelper.CrosspostAsync(this, base.Discord, options);
	}
}
