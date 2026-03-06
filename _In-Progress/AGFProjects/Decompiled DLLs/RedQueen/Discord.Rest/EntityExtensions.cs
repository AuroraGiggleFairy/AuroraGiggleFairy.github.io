using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal static class EntityExtensions
{
	public static IEmote ToIEmote(this Discord.API.Emoji model)
	{
		if (model.Id.HasValue)
		{
			return model.ToEntity();
		}
		return new Emoji(model.Name);
	}

	public static GuildEmote ToEntity(this Discord.API.Emoji model)
	{
		return new GuildEmote(model.Id.Value, model.Name, model.Animated == true, model.Managed, model.RequireColons, System.Collections.Immutable.ImmutableArray.Create(model.Roles), model.User.IsSpecified ? new ulong?(model.User.Value.Id) : ((ulong?)null));
	}

	public static Embed ToEntity(this Discord.API.Embed model)
	{
		return new Embed(model.Type, model.Title, model.Description, model.Url, model.Timestamp, model.Color.HasValue ? new Color?(new Color(model.Color.Value)) : ((Color?)null), model.Image.IsSpecified ? new EmbedImage?(model.Image.Value.ToEntity()) : ((EmbedImage?)null), model.Video.IsSpecified ? new EmbedVideo?(model.Video.Value.ToEntity()) : ((EmbedVideo?)null), model.Author.IsSpecified ? new EmbedAuthor?(model.Author.Value.ToEntity()) : ((EmbedAuthor?)null), model.Footer.IsSpecified ? new EmbedFooter?(model.Footer.Value.ToEntity()) : ((EmbedFooter?)null), model.Provider.IsSpecified ? new EmbedProvider?(model.Provider.Value.ToEntity()) : ((EmbedProvider?)null), model.Thumbnail.IsSpecified ? new EmbedThumbnail?(model.Thumbnail.Value.ToEntity()) : ((EmbedThumbnail?)null), model.Fields.IsSpecified ? model.Fields.Value.Select((Discord.API.EmbedField x) => x.ToEntity()).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<EmbedField>());
	}

	public static RoleTags ToEntity(this Discord.API.RoleTags model)
	{
		return new RoleTags(model.BotId.IsSpecified ? new ulong?(model.BotId.Value) : ((ulong?)null), model.IntegrationId.IsSpecified ? new ulong?(model.IntegrationId.Value) : ((ulong?)null), model.IsPremiumSubscriber.IsSpecified);
	}

	public static Discord.API.Embed ToModel(this Embed entity)
	{
		if (entity == null)
		{
			return null;
		}
		Discord.API.Embed embed = new Discord.API.Embed
		{
			Type = entity.Type,
			Title = entity.Title,
			Description = entity.Description,
			Url = entity.Url,
			Timestamp = entity.Timestamp,
			Color = entity.Color?.RawValue
		};
		if (entity.Author != null)
		{
			embed.Author = entity.Author.Value.ToModel();
		}
		embed.Fields = entity.Fields.Select((EmbedField x) => x.ToModel()).ToArray();
		if (entity.Footer != null)
		{
			embed.Footer = entity.Footer.Value.ToModel();
		}
		if (entity.Image != null)
		{
			embed.Image = entity.Image.Value.ToModel();
		}
		if (entity.Provider != null)
		{
			embed.Provider = entity.Provider.Value.ToModel();
		}
		if (entity.Thumbnail != null)
		{
			embed.Thumbnail = entity.Thumbnail.Value.ToModel();
		}
		if (entity.Video != null)
		{
			embed.Video = entity.Video.Value.ToModel();
		}
		return embed;
	}

	public static Discord.API.AllowedMentions ToModel(this AllowedMentions entity)
	{
		if (entity == null)
		{
			return null;
		}
		return new Discord.API.AllowedMentions
		{
			Parse = entity.AllowedTypes?.EnumerateMentionTypes().ToArray(),
			Roles = entity.RoleIds?.ToArray(),
			Users = entity.UserIds?.ToArray(),
			RepliedUser = (((Optional<bool>?)entity.MentionRepliedUser) ?? Optional.Create<bool>())
		};
	}

	public static Discord.API.MessageReference ToModel(this MessageReference entity)
	{
		return new Discord.API.MessageReference
		{
			ChannelId = entity.InternalChannelId,
			GuildId = entity.GuildId,
			MessageId = entity.MessageId,
			FailIfNotExists = entity.FailIfNotExists
		};
	}

	public static IEnumerable<string> EnumerateMentionTypes(this AllowedMentionTypes mentionTypes)
	{
		if (mentionTypes.HasFlag(AllowedMentionTypes.Everyone))
		{
			yield return "everyone";
		}
		if (mentionTypes.HasFlag(AllowedMentionTypes.Roles))
		{
			yield return "roles";
		}
		if (mentionTypes.HasFlag(AllowedMentionTypes.Users))
		{
			yield return "users";
		}
	}

	public static EmbedAuthor ToEntity(this Discord.API.EmbedAuthor model)
	{
		return new EmbedAuthor(model.Name, model.Url, model.IconUrl, model.ProxyIconUrl);
	}

	public static Discord.API.EmbedAuthor ToModel(this EmbedAuthor entity)
	{
		return new Discord.API.EmbedAuthor
		{
			Name = entity.Name,
			Url = entity.Url,
			IconUrl = entity.IconUrl
		};
	}

	public static EmbedField ToEntity(this Discord.API.EmbedField model)
	{
		return new EmbedField(model.Name, model.Value, model.Inline);
	}

	public static Discord.API.EmbedField ToModel(this EmbedField entity)
	{
		return new Discord.API.EmbedField
		{
			Name = entity.Name,
			Value = entity.Value,
			Inline = entity.Inline
		};
	}

	public static EmbedFooter ToEntity(this Discord.API.EmbedFooter model)
	{
		return new EmbedFooter(model.Text, model.IconUrl, model.ProxyIconUrl);
	}

	public static Discord.API.EmbedFooter ToModel(this EmbedFooter entity)
	{
		return new Discord.API.EmbedFooter
		{
			Text = entity.Text,
			IconUrl = entity.IconUrl
		};
	}

	public static EmbedImage ToEntity(this Discord.API.EmbedImage model)
	{
		return new EmbedImage(model.Url, model.ProxyUrl, model.Height.IsSpecified ? new int?(model.Height.Value) : ((int?)null), model.Width.IsSpecified ? new int?(model.Width.Value) : ((int?)null));
	}

	public static Discord.API.EmbedImage ToModel(this EmbedImage entity)
	{
		return new Discord.API.EmbedImage
		{
			Url = entity.Url
		};
	}

	public static EmbedProvider ToEntity(this Discord.API.EmbedProvider model)
	{
		return new EmbedProvider(model.Name, model.Url);
	}

	public static Discord.API.EmbedProvider ToModel(this EmbedProvider entity)
	{
		return new Discord.API.EmbedProvider
		{
			Name = entity.Name,
			Url = entity.Url
		};
	}

	public static EmbedThumbnail ToEntity(this Discord.API.EmbedThumbnail model)
	{
		return new EmbedThumbnail(model.Url, model.ProxyUrl, model.Height.IsSpecified ? new int?(model.Height.Value) : ((int?)null), model.Width.IsSpecified ? new int?(model.Width.Value) : ((int?)null));
	}

	public static Discord.API.EmbedThumbnail ToModel(this EmbedThumbnail entity)
	{
		return new Discord.API.EmbedThumbnail
		{
			Url = entity.Url
		};
	}

	public static EmbedVideo ToEntity(this Discord.API.EmbedVideo model)
	{
		return new EmbedVideo(model.Url, model.Height.IsSpecified ? new int?(model.Height.Value) : ((int?)null), model.Width.IsSpecified ? new int?(model.Width.Value) : ((int?)null));
	}

	public static Discord.API.EmbedVideo ToModel(this EmbedVideo entity)
	{
		return new Discord.API.EmbedVideo
		{
			Url = entity.Url
		};
	}

	public static Discord.API.Image ToModel(this Image entity)
	{
		return new Discord.API.Image(entity.Stream);
	}

	public static Overwrite ToEntity(this Discord.API.Overwrite model)
	{
		return new Overwrite(model.TargetId, model.TargetType, new OverwritePermissions(model.Allow, model.Deny));
	}

	public static Message ToMessage(this InteractionResponse model, IDiscordInteraction interaction)
	{
		if (model.Data.IsSpecified)
		{
			InteractionCallbackData value = model.Data.Value;
			Message message = new Message
			{
				IsTextToSpeech = value.TTS,
				Content = ((value.Content.IsSpecified && value.Content.Value == null) ? Optional<string>.Unspecified : value.Content),
				Embeds = value.Embeds,
				AllowedMentions = value.AllowedMentions,
				Components = value.Components,
				Flags = value.Flags
			};
			if (interaction is IApplicationCommandInteraction applicationCommandInteraction)
			{
				message.Interaction = new MessageInteraction
				{
					Id = applicationCommandInteraction.Id,
					Name = applicationCommandInteraction.Data.Name,
					Type = InteractionType.ApplicationCommand,
					User = new User
					{
						Username = applicationCommandInteraction.User.Username,
						Avatar = applicationCommandInteraction.User.AvatarId,
						Bot = applicationCommandInteraction.User.IsBot,
						Discriminator = applicationCommandInteraction.User.Discriminator,
						PublicFlags = (applicationCommandInteraction.User.PublicFlags.HasValue ? ((Optional<UserProperties>)applicationCommandInteraction.User.PublicFlags.Value) : Optional<UserProperties>.Unspecified),
						Id = applicationCommandInteraction.User.Id
					}
				};
			}
			return message;
		}
		return new Message
		{
			Id = interaction.Id
		};
	}
}
