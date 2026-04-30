using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Net;

namespace Discord.Rest;

internal static class InteractionHelper
{
	public const double ResponseTimeLimit = 3.0;

	public const double ResponseAndFollowupLimit = 15.0;

	public static bool CanSendResponse(IDiscordInteraction interaction)
	{
		return (DateTime.UtcNow - interaction.CreatedAt).TotalSeconds < 3.0;
	}

	public static bool CanRespondOrFollowup(IDiscordInteraction interaction)
	{
		return (DateTime.UtcNow - interaction.CreatedAt).TotalMinutes <= 15.0;
	}

	public static Task DeleteAllGuildCommandsAsync(BaseDiscordClient client, ulong guildId, RequestOptions options = null)
	{
		return client.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(guildId, Array.Empty<CreateApplicationCommandParams>(), options);
	}

	public static Task DeleteAllGlobalCommandsAsync(BaseDiscordClient client, RequestOptions options = null)
	{
		return client.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<CreateApplicationCommandParams>(), options);
	}

	public static async Task SendInteractionResponseAsync(BaseDiscordClient client, InteractionResponse response, IDiscordInteraction interaction, IMessageChannel channel = null, RequestOptions options = null)
	{
		await client.ApiClient.CreateInteractionResponseAsync(response, interaction.Id, interaction.Token, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task SendInteractionResponseAsync(BaseDiscordClient client, UploadInteractionFileParams response, IDiscordInteraction interaction, IMessageChannel channel = null, RequestOptions options = null)
	{
		await client.ApiClient.CreateInteractionResponseAsync(response, interaction.Id, interaction.Token, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<RestInteractionMessage> GetOriginalResponseAsync(BaseDiscordClient client, IMessageChannel channel, IDiscordInteraction interaction, RequestOptions options = null)
	{
		Message message = await client.ApiClient.GetInteractionResponseAsync(interaction.Token, options).ConfigureAwait(continueOnCapturedContext: false);
		if (message != null)
		{
			return RestInteractionMessage.Create(client, message, interaction.Token, channel);
		}
		return null;
	}

	public static async Task<RestFollowupMessage> SendFollowupAsync(BaseDiscordClient client, CreateWebhookMessageParams args, string token, IMessageChannel channel, RequestOptions options = null)
	{
		return RestFollowupMessage.Create(client, await client.ApiClient.CreateInteractionFollowupMessageAsync(args, token, options).ConfigureAwait(continueOnCapturedContext: false), token, channel);
	}

	public static async Task<RestFollowupMessage> SendFollowupAsync(BaseDiscordClient client, UploadWebhookFileParams args, string token, IMessageChannel channel, RequestOptions options = null)
	{
		return RestFollowupMessage.Create(client, await client.ApiClient.CreateInteractionFollowupMessageAsync(args, token, options).ConfigureAwait(continueOnCapturedContext: false), token, channel);
	}

	public static async Task<RestGlobalCommand> GetGlobalCommandAsync(BaseDiscordClient client, ulong id, RequestOptions options = null)
	{
		return RestGlobalCommand.Create(client, await client.ApiClient.GetGlobalApplicationCommandAsync(id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static Task<ApplicationCommand> CreateGlobalCommandAsync<TArg>(BaseDiscordClient client, Action<TArg> func, RequestOptions options = null) where TArg : ApplicationCommandProperties
	{
		object obj = Activator.CreateInstance(typeof(TArg));
		func((TArg)obj);
		return CreateGlobalCommandAsync(client, (TArg)obj, options);
	}

	public static async Task<ApplicationCommand> CreateGlobalCommandAsync(BaseDiscordClient client, ApplicationCommandProperties arg, RequestOptions options = null)
	{
		Preconditions.NotNullOrEmpty(arg.Name, "Name");
		CreateApplicationCommandParams createApplicationCommandParams = new CreateApplicationCommandParams
		{
			Name = arg.Name.Value,
			Type = arg.Type,
			DefaultPermission = (arg.IsDefaultPermission.IsSpecified ? ((Optional<bool>)arg.IsDefaultPermission.Value) : Optional<bool>.Unspecified),
			NameLocalizations = arg.NameLocalizations?.ToDictionary(),
			DescriptionLocalizations = arg.DescriptionLocalizations?.ToDictionary(),
			DefaultMemberPermission = arg.DefaultMemberPermissions.ToNullable(),
			DmPermission = arg.IsDMEnabled.ToNullable()
		};
		if (arg is SlashCommandProperties slashCommandProperties)
		{
			Preconditions.NotNullOrEmpty(slashCommandProperties.Description, "Description");
			createApplicationCommandParams.Description = slashCommandProperties.Description.Value;
			createApplicationCommandParams.Options = (slashCommandProperties.Options.IsSpecified ? ((Optional<ApplicationCommandOption[]>)slashCommandProperties.Options.Value.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray()) : Optional<ApplicationCommandOption[]>.Unspecified);
		}
		return await client.ApiClient.CreateGlobalApplicationCommandAsync(createApplicationCommandParams, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<ApplicationCommand[]> BulkOverwriteGlobalCommandsAsync(BaseDiscordClient client, ApplicationCommandProperties[] args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		List<CreateApplicationCommandParams> list = new List<CreateApplicationCommandParams>();
		foreach (ApplicationCommandProperties applicationCommandProperties in args)
		{
			Preconditions.NotNullOrEmpty(applicationCommandProperties.Name, "Name");
			CreateApplicationCommandParams createApplicationCommandParams = new CreateApplicationCommandParams
			{
				Name = applicationCommandProperties.Name.Value,
				Type = applicationCommandProperties.Type,
				DefaultPermission = (applicationCommandProperties.IsDefaultPermission.IsSpecified ? ((Optional<bool>)applicationCommandProperties.IsDefaultPermission.Value) : Optional<bool>.Unspecified),
				NameLocalizations = applicationCommandProperties.NameLocalizations?.ToDictionary(),
				DescriptionLocalizations = applicationCommandProperties.DescriptionLocalizations?.ToDictionary(),
				DefaultMemberPermission = applicationCommandProperties.DefaultMemberPermissions.ToNullable(),
				DmPermission = applicationCommandProperties.IsDMEnabled.ToNullable()
			};
			if (applicationCommandProperties is SlashCommandProperties slashCommandProperties)
			{
				Preconditions.NotNullOrEmpty(slashCommandProperties.Description, "Description");
				createApplicationCommandParams.Description = slashCommandProperties.Description.Value;
				createApplicationCommandParams.Options = (slashCommandProperties.Options.IsSpecified ? ((Optional<ApplicationCommandOption[]>)slashCommandProperties.Options.Value.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray()) : Optional<ApplicationCommandOption[]>.Unspecified);
			}
			list.Add(createApplicationCommandParams);
		}
		return await client.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(list.ToArray(), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IReadOnlyCollection<ApplicationCommand>> BulkOverwriteGuildCommandsAsync(BaseDiscordClient client, ulong guildId, ApplicationCommandProperties[] args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		List<CreateApplicationCommandParams> list = new List<CreateApplicationCommandParams>();
		foreach (ApplicationCommandProperties applicationCommandProperties in args)
		{
			Preconditions.NotNullOrEmpty(applicationCommandProperties.Name, "Name");
			CreateApplicationCommandParams createApplicationCommandParams = new CreateApplicationCommandParams
			{
				Name = applicationCommandProperties.Name.Value,
				Type = applicationCommandProperties.Type,
				DefaultPermission = (applicationCommandProperties.IsDefaultPermission.IsSpecified ? ((Optional<bool>)applicationCommandProperties.IsDefaultPermission.Value) : Optional<bool>.Unspecified),
				NameLocalizations = applicationCommandProperties.NameLocalizations?.ToDictionary(),
				DescriptionLocalizations = applicationCommandProperties.DescriptionLocalizations?.ToDictionary(),
				DefaultMemberPermission = applicationCommandProperties.DefaultMemberPermissions.ToNullable(),
				DmPermission = applicationCommandProperties.IsDMEnabled.ToNullable()
			};
			if (applicationCommandProperties is SlashCommandProperties slashCommandProperties)
			{
				Preconditions.NotNullOrEmpty(slashCommandProperties.Description, "Description");
				createApplicationCommandParams.Description = slashCommandProperties.Description.Value;
				createApplicationCommandParams.Options = (slashCommandProperties.Options.IsSpecified ? ((Optional<ApplicationCommandOption[]>)slashCommandProperties.Options.Value.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray()) : Optional<ApplicationCommandOption[]>.Unspecified);
			}
			list.Add(createApplicationCommandParams);
		}
		return await client.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(guildId, list.ToArray(), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static TArg GetApplicationCommandProperties<TArg>(IApplicationCommand command) where TArg : ApplicationCommandProperties
	{
		bool flag = typeof(TArg) == typeof(ApplicationCommandProperties);
		if ((typeof(TArg) == typeof(SlashCommandProperties) || flag) && command.Type == ApplicationCommandType.Slash)
		{
			return new SlashCommandProperties() as TArg;
		}
		if ((typeof(TArg) == typeof(MessageCommandProperties) || flag) && command.Type == ApplicationCommandType.Message)
		{
			return new MessageCommandProperties() as TArg;
		}
		if ((typeof(TArg) == typeof(UserCommandProperties) || flag) && command.Type == ApplicationCommandType.User)
		{
			return new UserCommandProperties() as TArg;
		}
		throw new InvalidOperationException($"Cannot modify application command of type {command.Type} with the parameter type {typeof(TArg).FullName}");
	}

	public static Task<ApplicationCommand> ModifyGlobalCommandAsync<TArg>(BaseDiscordClient client, IApplicationCommand command, Action<TArg> func, RequestOptions options = null) where TArg : ApplicationCommandProperties
	{
		TArg applicationCommandProperties = GetApplicationCommandProperties<TArg>(command);
		func(applicationCommandProperties);
		return ModifyGlobalCommandAsync(client, command, applicationCommandProperties, options);
	}

	public static async Task<ApplicationCommand> ModifyGlobalCommandAsync(BaseDiscordClient client, IApplicationCommand command, ApplicationCommandProperties args, RequestOptions options = null)
	{
		if (args.Name.IsSpecified)
		{
			Preconditions.AtMost(args.Name.Value.Length, 32, "Name");
			Preconditions.AtLeast(args.Name.Value.Length, 1, "Name");
		}
		ModifyApplicationCommandParams modifyApplicationCommandParams = new ModifyApplicationCommandParams
		{
			Name = args.Name,
			DefaultPermission = (args.IsDefaultPermission.IsSpecified ? ((Optional<bool>)args.IsDefaultPermission.Value) : Optional<bool>.Unspecified),
			NameLocalizations = args.NameLocalizations?.ToDictionary(),
			DescriptionLocalizations = args.DescriptionLocalizations?.ToDictionary()
		};
		if (args is SlashCommandProperties { Description: var description } slashCommandProperties)
		{
			if (description.IsSpecified)
			{
				Preconditions.AtMost(slashCommandProperties.Description.Value.Length, 100, "Description");
				Preconditions.AtLeast(slashCommandProperties.Description.Value.Length, 1, "Description");
			}
			if (slashCommandProperties.Options.IsSpecified && slashCommandProperties.Options.Value.Count > 10)
			{
				throw new ArgumentException("Option count must be 10 or less");
			}
			modifyApplicationCommandParams.Description = slashCommandProperties.Description;
			modifyApplicationCommandParams.Options = (slashCommandProperties.Options.IsSpecified ? ((Optional<ApplicationCommandOption[]>)slashCommandProperties.Options.Value.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray()) : Optional<ApplicationCommandOption[]>.Unspecified);
		}
		return await client.ApiClient.ModifyGlobalApplicationCommandAsync(modifyApplicationCommandParams, command.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteGlobalCommandAsync(BaseDiscordClient client, IApplicationCommand command, RequestOptions options = null)
	{
		Preconditions.NotNull(command, "command");
		Preconditions.NotEqual(command.Id, 0uL, "Id");
		await client.ApiClient.DeleteGlobalApplicationCommandAsync(command.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task<ApplicationCommand> CreateGuildCommandAsync<TArg>(BaseDiscordClient client, ulong guildId, Action<TArg> func, RequestOptions options) where TArg : ApplicationCommandProperties
	{
		object obj = Activator.CreateInstance(typeof(TArg));
		func((TArg)obj);
		return CreateGuildCommandAsync(client, guildId, (TArg)obj, options);
	}

	public static async Task<ApplicationCommand> CreateGuildCommandAsync(BaseDiscordClient client, ulong guildId, ApplicationCommandProperties arg, RequestOptions options = null)
	{
		CreateApplicationCommandParams createApplicationCommandParams = new CreateApplicationCommandParams
		{
			Name = arg.Name.Value,
			Type = arg.Type,
			DefaultPermission = (arg.IsDefaultPermission.IsSpecified ? ((Optional<bool>)arg.IsDefaultPermission.Value) : Optional<bool>.Unspecified),
			NameLocalizations = arg.NameLocalizations?.ToDictionary(),
			DescriptionLocalizations = arg.DescriptionLocalizations?.ToDictionary(),
			DefaultMemberPermission = arg.DefaultMemberPermissions.ToNullable(),
			DmPermission = arg.IsDMEnabled.ToNullable()
		};
		if (arg is SlashCommandProperties slashCommandProperties)
		{
			Preconditions.NotNullOrEmpty(slashCommandProperties.Description, "Description");
			createApplicationCommandParams.Description = slashCommandProperties.Description.Value;
			createApplicationCommandParams.Options = (slashCommandProperties.Options.IsSpecified ? ((Optional<ApplicationCommandOption[]>)slashCommandProperties.Options.Value.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray()) : Optional<ApplicationCommandOption[]>.Unspecified);
		}
		return await client.ApiClient.CreateGuildApplicationCommandAsync(createApplicationCommandParams, guildId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task<ApplicationCommand> ModifyGuildCommandAsync<TArg>(BaseDiscordClient client, IApplicationCommand command, ulong guildId, Action<TArg> func, RequestOptions options = null) where TArg : ApplicationCommandProperties
	{
		TArg applicationCommandProperties = GetApplicationCommandProperties<TArg>(command);
		func(applicationCommandProperties);
		return ModifyGuildCommandAsync(client, command, guildId, applicationCommandProperties, options);
	}

	public static async Task<ApplicationCommand> ModifyGuildCommandAsync(BaseDiscordClient client, IApplicationCommand command, ulong guildId, ApplicationCommandProperties arg, RequestOptions options = null)
	{
		ModifyApplicationCommandParams modifyApplicationCommandParams = new ModifyApplicationCommandParams
		{
			Name = arg.Name,
			DefaultPermission = (arg.IsDefaultPermission.IsSpecified ? ((Optional<bool>)arg.IsDefaultPermission.Value) : Optional<bool>.Unspecified),
			NameLocalizations = arg.NameLocalizations?.ToDictionary(),
			DescriptionLocalizations = arg.DescriptionLocalizations?.ToDictionary()
		};
		if (arg is SlashCommandProperties slashCommandProperties)
		{
			Preconditions.NotNullOrEmpty(slashCommandProperties.Description, "Description");
			modifyApplicationCommandParams.Description = slashCommandProperties.Description.Value;
			modifyApplicationCommandParams.Options = (slashCommandProperties.Options.IsSpecified ? ((Optional<ApplicationCommandOption[]>)slashCommandProperties.Options.Value.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray()) : Optional<ApplicationCommandOption[]>.Unspecified);
		}
		return await client.ApiClient.ModifyGuildApplicationCommandAsync(modifyApplicationCommandParams, guildId, command.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteGuildCommandAsync(BaseDiscordClient client, ulong guildId, IApplicationCommand command, RequestOptions options = null)
	{
		Preconditions.NotNull(command, "command");
		Preconditions.NotEqual(command.Id, 0uL, "Id");
		await client.ApiClient.DeleteGuildApplicationCommandAsync(guildId, command.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task DeleteUnknownApplicationCommandAsync(BaseDiscordClient client, ulong? guildId, IApplicationCommand command, RequestOptions options = null)
	{
		if (!guildId.HasValue)
		{
			return DeleteGlobalCommandAsync(client, command, options);
		}
		return DeleteGuildCommandAsync(client, guildId.Value, command, options);
	}

	public static async Task<Message> ModifyFollowupMessageAsync(BaseDiscordClient client, RestFollowupMessage message, Action<MessageProperties> func, RequestOptions options = null)
	{
		MessageProperties messageProperties = new MessageProperties();
		func(messageProperties);
		Optional<Embed> embed = messageProperties.Embed;
		Optional<Embed[]> embeds = messageProperties.Embeds;
		bool flag = (messageProperties.Content.IsSpecified ? (!string.IsNullOrEmpty(messageProperties.Content.Value)) : (!string.IsNullOrEmpty(message.Content)));
		if (embed.IsSpecified && embed.Value != null)
		{
			goto IL_00b4;
		}
		if (embeds.IsSpecified)
		{
			Embed[] value = embeds.Value;
			if (value != null && value.Length != 0)
			{
				goto IL_00b4;
			}
		}
		int num = (message.Embeds.Any() ? 1 : 0);
		goto IL_00b5;
		IL_00b4:
		num = 1;
		goto IL_00b5;
		IL_00b5:
		bool flag2 = (byte)num != 0;
		if ((!messageProperties.Components.IsSpecified || messageProperties.Components.Value == null) && !flag && !flag2)
		{
			Preconditions.NotNullOrEmpty(messageProperties.Content.IsSpecified ? messageProperties.Content.Value : string.Empty, "Content");
		}
		List<Discord.API.Embed> list = ((embed.IsSpecified || embeds.IsSpecified) ? new List<Discord.API.Embed>() : null);
		if (embed.IsSpecified && embed.Value != null)
		{
			list.Add(embed.Value.ToModel());
		}
		if (embeds.IsSpecified && embeds.Value != null)
		{
			list.AddRange(embeds.Value.Select((Embed x) => x.ToModel()));
		}
		Preconditions.AtMost(list?.Count ?? 0, 10, "Embeds", "A max of 10 embeds are allowed.");
		ModifyInteractionResponseParams obj = new ModifyInteractionResponseParams
		{
			Content = messageProperties.Content
		};
		Discord.API.Embed[] array = list?.ToArray();
		obj.Embeds = ((array != null) ? ((Optional<Discord.API.Embed[]>)array) : Optional<Discord.API.Embed[]>.Unspecified);
		obj.AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)messageProperties.AllowedMentions.Value.ToModel()) : Optional<Discord.API.AllowedMentions>.Unspecified);
		obj.Components = (messageProperties.Components.IsSpecified ? ((Optional<Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray())) : Optional<Discord.API.ActionRowComponent[]>.Unspecified);
		ModifyInteractionResponseParams args = obj;
		return await client.ApiClient.ModifyInteractionFollowupMessageAsync(args, message.Id, message.Token, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteFollowupMessageAsync(BaseDiscordClient client, RestFollowupMessage message, RequestOptions options = null)
	{
		await client.ApiClient.DeleteInteractionFollowupMessageAsync(message.Id, message.Token, options);
	}

	public static async Task<Message> ModifyInteractionResponseAsync(BaseDiscordClient client, string token, Action<MessageProperties> func, RequestOptions options = null)
	{
		MessageProperties messageProperties = new MessageProperties();
		func(messageProperties);
		Optional<Embed> embed = messageProperties.Embed;
		Optional<Embed[]> embeds = messageProperties.Embeds;
		bool flag = !string.IsNullOrEmpty(messageProperties.Content.GetValueOrDefault());
		int num;
		if (!embed.IsSpecified || !(embed.Value != null))
		{
			if (embeds.IsSpecified)
			{
				Embed[] value = embeds.Value;
				num = ((value != null && value.Length != 0) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool flag2 = (byte)num != 0;
		if ((!messageProperties.Components.IsSpecified || messageProperties.Components.Value == null) && !flag && !flag2)
		{
			Preconditions.NotNullOrEmpty(messageProperties.Content.IsSpecified ? messageProperties.Content.Value : string.Empty, "Content");
		}
		List<Discord.API.Embed> list = ((embed.IsSpecified || embeds.IsSpecified) ? new List<Discord.API.Embed>() : null);
		if (embed.IsSpecified && embed.Value != null)
		{
			list.Add(embed.Value.ToModel());
		}
		if (embeds.IsSpecified && embeds.Value != null)
		{
			list.AddRange(embeds.Value.Select((Embed x) => x.ToModel()));
		}
		Preconditions.AtMost(list?.Count ?? 0, 10, "Embeds", "A max of 10 embeds are allowed.");
		Discord.API.Embed[] array;
		if (!messageProperties.Attachments.IsSpecified)
		{
			ModifyInteractionResponseParams obj = new ModifyInteractionResponseParams
			{
				Content = messageProperties.Content
			};
			array = list?.ToArray();
			obj.Embeds = ((array != null) ? ((Optional<Discord.API.Embed[]>)array) : Optional<Discord.API.Embed[]>.Unspecified);
			obj.AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)(messageProperties.AllowedMentions.Value?.ToModel())) : Optional<Discord.API.AllowedMentions>.Unspecified);
			obj.Components = (messageProperties.Components.IsSpecified ? ((Optional<Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray())) : Optional<Discord.API.ActionRowComponent[]>.Unspecified);
			obj.Flags = messageProperties.Flags;
			ModifyInteractionResponseParams args = obj;
			return await client.ApiClient.ModifyInteractionResponseAsync(args, token, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		UploadWebhookFileParams obj2 = new UploadWebhookFileParams(messageProperties.Attachments.Value.ToArray())
		{
			Content = messageProperties.Content
		};
		array = list?.ToArray();
		obj2.Embeds = ((array != null) ? ((Optional<Discord.API.Embed[]>)array) : Optional<Discord.API.Embed[]>.Unspecified);
		obj2.AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)(messageProperties.AllowedMentions.Value?.ToModel())) : Optional<Discord.API.AllowedMentions>.Unspecified);
		obj2.MessageComponents = (messageProperties.Components.IsSpecified ? ((Optional<Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray())) : Optional<Discord.API.ActionRowComponent[]>.Unspecified);
		UploadWebhookFileParams args2 = obj2;
		return await client.ApiClient.ModifyInteractionResponseAsync(args2, token, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task DeleteInteractionResponseAsync(BaseDiscordClient client, RestInteractionMessage message, RequestOptions options = null)
	{
		await client.ApiClient.DeleteInteractionResponseAsync(message.Token, options);
	}

	public static async Task DeleteInteractionResponseAsync(BaseDiscordClient client, IDiscordInteraction interaction, RequestOptions options = null)
	{
		await client.ApiClient.DeleteInteractionResponseAsync(interaction.Token, options);
	}

	public static Task SendAutocompleteResultAsync(BaseDiscordClient client, IEnumerable<AutocompleteResult> result, ulong interactionId, string interactionToken, RequestOptions options)
	{
		if (result == null)
		{
			result = Array.Empty<AutocompleteResult>();
		}
		Preconditions.AtMost(result.Count(), 25, "result", "A maximum of 25 choices are allowed!");
		InteractionResponse response = new InteractionResponse
		{
			Type = InteractionResponseType.ApplicationCommandAutocompleteResult,
			Data = new InteractionCallbackData
			{
				Choices = (result.Any() ? result.Select((AutocompleteResult x) => new ApplicationCommandOptionChoice
				{
					Name = x.Name,
					Value = x.Value
				}).ToArray() : Array.Empty<ApplicationCommandOptionChoice>())
			}
		};
		return client.ApiClient.CreateInteractionResponseAsync(response, interactionId, interactionToken, options);
	}

	public static async Task<IReadOnlyCollection<GuildApplicationCommandPermission>> GetGuildCommandPermissionsAsync(BaseDiscordClient client, ulong guildId, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildApplicationCommandPermissionsAsync(guildId, options)).Select((Discord.API.GuildApplicationCommandPermission x) => new GuildApplicationCommandPermission(x.Id, x.ApplicationId, guildId, x.Permissions.Select((ApplicationCommandPermissions y) => new ApplicationCommandPermission(y.Id, y.Type, y.Permission)).ToArray())).ToArray();
	}

	public static async Task<GuildApplicationCommandPermission> GetGuildCommandPermissionAsync(BaseDiscordClient client, ulong guildId, ulong commandId, RequestOptions options)
	{
		try
		{
			Discord.API.GuildApplicationCommandPermission guildApplicationCommandPermission = await client.ApiClient.GetGuildApplicationCommandPermissionAsync(guildId, commandId, options);
			return new GuildApplicationCommandPermission(guildApplicationCommandPermission.Id, guildApplicationCommandPermission.ApplicationId, guildId, guildApplicationCommandPermission.Permissions.Select((ApplicationCommandPermissions y) => new ApplicationCommandPermission(y.Id, y.Type, y.Permission)).ToArray());
		}
		catch (HttpException ex)
		{
			if (ex.HttpCode == HttpStatusCode.NotFound)
			{
				return null;
			}
			throw;
		}
	}

	public static async Task<GuildApplicationCommandPermission> ModifyGuildCommandPermissionsAsync(BaseDiscordClient client, ulong guildId, ulong commandId, ApplicationCommandPermission[] args, RequestOptions options)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.AtMost(args.Length, 10, "args");
		Preconditions.AtLeast(args.Length, 0, "args");
		List<ApplicationCommandPermissions> list = new List<ApplicationCommandPermissions>();
		foreach (ApplicationCommandPermission applicationCommandPermission in args)
		{
			ApplicationCommandPermissions item = new ApplicationCommandPermissions
			{
				Id = applicationCommandPermission.TargetId,
				Permission = applicationCommandPermission.Permission,
				Type = applicationCommandPermission.TargetType
			};
			list.Add(item);
		}
		ModifyGuildApplicationCommandPermissionsParams permissions = new ModifyGuildApplicationCommandPermissionsParams
		{
			Permissions = list.ToArray()
		};
		Discord.API.GuildApplicationCommandPermission guildApplicationCommandPermission = await client.ApiClient.ModifyApplicationCommandPermissionsAsync(permissions, guildId, commandId, options);
		return new GuildApplicationCommandPermission(guildApplicationCommandPermission.Id, guildApplicationCommandPermission.ApplicationId, guildId, guildApplicationCommandPermission.Permissions.Select((ApplicationCommandPermissions x) => new ApplicationCommandPermission(x.Id, x.Type, x.Permission)).ToArray());
	}

	public static async Task<IReadOnlyCollection<GuildApplicationCommandPermission>> BatchEditGuildCommandPermissionsAsync(BaseDiscordClient client, ulong guildId, IDictionary<ulong, ApplicationCommandPermission[]> args, RequestOptions options)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(args.Count, 0, "args");
		List<ModifyGuildApplicationCommandPermissions> list = new List<ModifyGuildApplicationCommandPermissions>();
		foreach (KeyValuePair<ulong, ApplicationCommandPermission[]> arg in args)
		{
			Preconditions.AtMost(arg.Value.Length, 10, "args");
			ModifyGuildApplicationCommandPermissions item = new ModifyGuildApplicationCommandPermissions
			{
				Id = arg.Key,
				Permissions = arg.Value.Select((ApplicationCommandPermission x) => new ApplicationCommandPermissions
				{
					Id = x.TargetId,
					Permission = x.Permission,
					Type = x.TargetType
				}).ToArray()
			};
			list.Add(item);
		}
		return (await client.ApiClient.BatchModifyApplicationCommandPermissionsAsync(list.ToArray(), guildId, options)).Select((Discord.API.GuildApplicationCommandPermission x) => new GuildApplicationCommandPermission(x.Id, x.ApplicationId, x.GuildId, x.Permissions.Select((ApplicationCommandPermissions y) => new ApplicationCommandPermission(y.Id, y.Type, y.Permission)).ToArray())).ToArray();
	}
}
