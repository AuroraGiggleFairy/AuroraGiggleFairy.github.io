using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord.Interactions;

internal static class ApplicationCommandRestUtil
{
	public static ApplicationCommandOptionProperties ToApplicationCommandOptionProps(this SlashCommandParameterInfo parameterInfo)
	{
		ILocalizationManager localizationManager = parameterInfo.Command.Module.CommandService.LocalizationManager;
		IList<string> parameterPath = parameterInfo.GetParameterPath();
		ApplicationCommandOptionProperties applicationCommandOptionProperties = new ApplicationCommandOptionProperties
		{
			Name = parameterInfo.Name,
			Description = parameterInfo.Description,
			Type = parameterInfo.DiscordOptionType.Value,
			IsRequired = parameterInfo.IsRequired,
			Choices = parameterInfo.Choices?.Select((ParameterChoice x) => new ApplicationCommandOptionChoiceProperties
			{
				Name = x.Name,
				Value = x.Value,
				NameLocalizations = (localizationManager?.GetAllNames(parameterInfo.GetChoicePath(x), LocalizationTarget.Choice) ?? ImmutableDictionary<string, string>.Empty)
			})?.ToList(),
			ChannelTypes = parameterInfo.ChannelTypes?.ToList(),
			IsAutocomplete = parameterInfo.IsAutocomplete,
			MaxValue = parameterInfo.MaxValue,
			MinValue = parameterInfo.MinValue,
			NameLocalizations = (localizationManager?.GetAllNames(parameterPath, LocalizationTarget.Parameter) ?? ImmutableDictionary<string, string>.Empty),
			DescriptionLocalizations = (localizationManager?.GetAllDescriptions(parameterPath, LocalizationTarget.Parameter) ?? ImmutableDictionary<string, string>.Empty),
			MinLength = parameterInfo.MinLength,
			MaxLength = parameterInfo.MaxLength
		};
		parameterInfo.TypeConverter.Write(applicationCommandOptionProperties, parameterInfo);
		return applicationCommandOptionProperties;
	}

	public static SlashCommandProperties ToApplicationCommandProps(this SlashCommandInfo commandInfo)
	{
		IList<string> commandPath = commandInfo.GetCommandPath();
		ILocalizationManager localizationManager = commandInfo.Module.CommandService.LocalizationManager;
		SlashCommandProperties slashCommandProperties = new SlashCommandBuilder
		{
			Name = commandInfo.Name,
			Description = commandInfo.Description,
			IsDefaultPermission = commandInfo.DefaultPermission,
			IsDMEnabled = commandInfo.IsEnabledInDm,
			DefaultMemberPermissions = (commandInfo.DefaultMemberPermissions.GetValueOrDefault() | commandInfo.Module.DefaultMemberPermissions.GetValueOrDefault()).SanitizeGuildPermissions()
		}.WithNameLocalizations(localizationManager?.GetAllNames(commandPath, LocalizationTarget.Command) ?? ImmutableDictionary<string, string>.Empty).WithDescriptionLocalizations(localizationManager?.GetAllDescriptions(commandPath, LocalizationTarget.Command) ?? ImmutableDictionary<string, string>.Empty).Build();
		if (commandInfo.Parameters.Count > 25)
		{
			throw new InvalidOperationException($"Slash Commands cannot have more than {25} command parameters");
		}
		List<ApplicationCommandOptionProperties> list = commandInfo.FlattenedParameters.Select((SlashCommandParameterInfo x) => x.ToApplicationCommandOptionProps())?.ToList();
		slashCommandProperties.Options = ((list != null) ? ((Optional<List<ApplicationCommandOptionProperties>>)list) : Optional<List<ApplicationCommandOptionProperties>>.Unspecified);
		return slashCommandProperties;
	}

	public static ApplicationCommandOptionProperties ToApplicationCommandOptionProps(this SlashCommandInfo commandInfo)
	{
		ILocalizationManager localizationManager = commandInfo.Module.CommandService.LocalizationManager;
		IList<string> commandPath = commandInfo.GetCommandPath();
		return new ApplicationCommandOptionProperties
		{
			Name = commandInfo.Name,
			Description = commandInfo.Description,
			Type = ApplicationCommandOptionType.SubCommand,
			IsRequired = false,
			Options = commandInfo.FlattenedParameters?.Select((SlashCommandParameterInfo x) => x.ToApplicationCommandOptionProps())?.ToList(),
			NameLocalizations = (localizationManager?.GetAllNames(commandPath, LocalizationTarget.Command) ?? ImmutableDictionary<string, string>.Empty),
			DescriptionLocalizations = (localizationManager?.GetAllDescriptions(commandPath, LocalizationTarget.Command) ?? ImmutableDictionary<string, string>.Empty)
		};
	}

	public static ApplicationCommandProperties ToApplicationCommandProps(this ContextCommandInfo commandInfo)
	{
		ILocalizationManager localizationManager = commandInfo.Module.CommandService.LocalizationManager;
		IList<string> commandPath = commandInfo.GetCommandPath();
		return commandInfo.CommandType switch
		{
			ApplicationCommandType.Message => new MessageCommandBuilder
			{
				Name = commandInfo.Name,
				IsDefaultPermission = commandInfo.DefaultPermission,
				DefaultMemberPermissions = (commandInfo.DefaultMemberPermissions.GetValueOrDefault() | commandInfo.Module.DefaultMemberPermissions.GetValueOrDefault()).SanitizeGuildPermissions(),
				IsDMEnabled = commandInfo.IsEnabledInDm
			}.WithNameLocalizations(localizationManager?.GetAllNames(commandPath, LocalizationTarget.Command) ?? ImmutableDictionary<string, string>.Empty).Build(), 
			ApplicationCommandType.User => new UserCommandBuilder
			{
				Name = commandInfo.Name,
				IsDefaultPermission = commandInfo.DefaultPermission,
				DefaultMemberPermissions = (commandInfo.DefaultMemberPermissions.GetValueOrDefault() | commandInfo.Module.DefaultMemberPermissions.GetValueOrDefault()).SanitizeGuildPermissions(),
				IsDMEnabled = commandInfo.IsEnabledInDm
			}.WithNameLocalizations(localizationManager?.GetAllNames(commandPath, LocalizationTarget.Command) ?? ImmutableDictionary<string, string>.Empty).Build(), 
			_ => throw new InvalidOperationException($"{commandInfo.CommandType} isn't a supported command type."), 
		};
	}

	public static IReadOnlyCollection<ApplicationCommandProperties> ToApplicationCommandProps(this ModuleInfo moduleInfo, bool ignoreDontRegister = false)
	{
		List<ApplicationCommandProperties> list = new List<ApplicationCommandProperties>();
		moduleInfo.ParseModuleModel(list, ignoreDontRegister);
		return list;
	}

	private static void ParseModuleModel(this ModuleInfo moduleInfo, List<ApplicationCommandProperties> args, bool ignoreDontRegister)
	{
		if (moduleInfo.DontAutoRegister && !ignoreDontRegister)
		{
			return;
		}
		args.AddRange(moduleInfo.ContextCommands?.Select((ContextCommandInfo x) => x.ToApplicationCommandProps()));
		if (!moduleInfo.IsSlashGroup)
		{
			args.AddRange(moduleInfo.SlashCommands?.Select((SlashCommandInfo x) => x.ToApplicationCommandProps()));
			{
				foreach (ModuleInfo subModule in moduleInfo.SubModules)
				{
					subModule.ParseModuleModel(args, ignoreDontRegister);
				}
				return;
			}
		}
		List<ApplicationCommandOptionProperties> list = new List<ApplicationCommandOptionProperties>();
		foreach (SlashCommandInfo slashCommand in moduleInfo.SlashCommands)
		{
			if (slashCommand.IgnoreGroupNames)
			{
				args.Add(slashCommand.ToApplicationCommandProps());
			}
			else
			{
				list.Add(slashCommand.ToApplicationCommandOptionProps());
			}
		}
		list.AddRange(moduleInfo.SubModules?.SelectMany((ModuleInfo x) => x.ParseSubModule(args, ignoreDontRegister)));
		ILocalizationManager localizationManager = moduleInfo.CommandService.LocalizationManager;
		IList<string> modulePath = moduleInfo.GetModulePath();
		SlashCommandProperties slashCommandProperties = new SlashCommandBuilder
		{
			Name = moduleInfo.SlashGroupName,
			Description = moduleInfo.Description,
			IsDefaultPermission = moduleInfo.DefaultPermission,
			IsDMEnabled = moduleInfo.IsEnabledInDm,
			DefaultMemberPermissions = moduleInfo.DefaultMemberPermissions
		}.WithNameLocalizations(localizationManager?.GetAllNames(modulePath, LocalizationTarget.Group) ?? ImmutableDictionary<string, string>.Empty).WithDescriptionLocalizations(localizationManager?.GetAllDescriptions(modulePath, LocalizationTarget.Group) ?? ImmutableDictionary<string, string>.Empty).Build();
		if (list.Count > 25)
		{
			throw new InvalidOperationException($"Slash Commands cannot have more than {25} command parameters");
		}
		slashCommandProperties.Options = list;
		args.Add(slashCommandProperties);
	}

	private static IReadOnlyCollection<ApplicationCommandOptionProperties> ParseSubModule(this ModuleInfo moduleInfo, List<ApplicationCommandProperties> args, bool ignoreDontRegister)
	{
		if (moduleInfo.DontAutoRegister && !ignoreDontRegister)
		{
			return Array.Empty<ApplicationCommandOptionProperties>();
		}
		args.AddRange(moduleInfo.ContextCommands?.Select((ContextCommandInfo x) => x.ToApplicationCommandProps()));
		List<ApplicationCommandOptionProperties> list = new List<ApplicationCommandOptionProperties>();
		list.AddRange(moduleInfo.SubModules?.SelectMany((ModuleInfo x) => x.ParseSubModule(args, ignoreDontRegister)));
		foreach (SlashCommandInfo slashCommand in moduleInfo.SlashCommands)
		{
			if (slashCommand.IgnoreGroupNames)
			{
				args.Add(slashCommand.ToApplicationCommandProps());
			}
			else
			{
				list.Add(slashCommand.ToApplicationCommandOptionProps());
			}
		}
		if (!moduleInfo.IsSlashGroup)
		{
			return list;
		}
		return new List<ApplicationCommandOptionProperties>
		{
			new ApplicationCommandOptionProperties
			{
				Name = moduleInfo.SlashGroupName,
				Description = moduleInfo.Description,
				Type = ApplicationCommandOptionType.SubCommandGroup,
				Options = list,
				NameLocalizations = (moduleInfo.CommandService.LocalizationManager?.GetAllNames(moduleInfo.GetModulePath(), LocalizationTarget.Group) ?? ImmutableDictionary<string, string>.Empty),
				DescriptionLocalizations = (moduleInfo.CommandService.LocalizationManager?.GetAllDescriptions(moduleInfo.GetModulePath(), LocalizationTarget.Group) ?? ImmutableDictionary<string, string>.Empty)
			}
		};
	}

	public static ApplicationCommandProperties ToApplicationCommandProps(this IApplicationCommand command)
	{
		switch (command.Type)
		{
		case ApplicationCommandType.Slash:
		{
			SlashCommandProperties obj = new SlashCommandProperties
			{
				Name = command.Name,
				Description = command.Description,
				IsDefaultPermission = command.IsDefaultPermission,
				DefaultMemberPermissions = (GuildPermission)command.DefaultMemberPermissions.RawValue,
				IsDMEnabled = command.IsEnabledInDm
			};
			List<ApplicationCommandOptionProperties> list = command.Options?.Select((IApplicationCommandOption x) => x.ToApplicationCommandOptionProps())?.ToList();
			obj.Options = ((list != null) ? ((Optional<List<ApplicationCommandOptionProperties>>)list) : Optional<List<ApplicationCommandOptionProperties>>.Unspecified);
			obj.NameLocalizations = command.NameLocalizations?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
			obj.DescriptionLocalizations = command.DescriptionLocalizations?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
			return obj;
		}
		case ApplicationCommandType.User:
			return new UserCommandProperties
			{
				Name = command.Name,
				IsDefaultPermission = command.IsDefaultPermission,
				DefaultMemberPermissions = (GuildPermission)command.DefaultMemberPermissions.RawValue,
				IsDMEnabled = command.IsEnabledInDm,
				NameLocalizations = (command.NameLocalizations?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty),
				DescriptionLocalizations = (command.DescriptionLocalizations?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty)
			};
		case ApplicationCommandType.Message:
			return new MessageCommandProperties
			{
				Name = command.Name,
				IsDefaultPermission = command.IsDefaultPermission,
				DefaultMemberPermissions = (GuildPermission)command.DefaultMemberPermissions.RawValue,
				IsDMEnabled = command.IsEnabledInDm,
				NameLocalizations = (command.NameLocalizations?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty),
				DescriptionLocalizations = (command.DescriptionLocalizations?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty)
			};
		default:
			throw new InvalidOperationException($"Cannot create command properties for command type {command.Type}");
		}
	}

	public static ApplicationCommandOptionProperties ToApplicationCommandOptionProps(this IApplicationCommandOption commandOption)
	{
		return new ApplicationCommandOptionProperties
		{
			Name = commandOption.Name,
			Description = commandOption.Description,
			Type = commandOption.Type,
			IsRequired = commandOption.IsRequired,
			ChannelTypes = commandOption.ChannelTypes?.ToList(),
			IsAutocomplete = (commandOption.IsAutocomplete == true),
			MinValue = commandOption.MinValue,
			MaxValue = commandOption.MaxValue,
			Choices = commandOption.Choices?.Select((IApplicationCommandOptionChoice x) => new ApplicationCommandOptionChoiceProperties
			{
				Name = x.Name,
				Value = x.Value
			}).ToList(),
			Options = commandOption.Options?.Select((IApplicationCommandOption x) => x.ToApplicationCommandOptionProps()).ToList(),
			NameLocalizations = commandOption.NameLocalizations?.ToImmutableDictionary(),
			DescriptionLocalizations = commandOption.DescriptionLocalizations?.ToImmutableDictionary(),
			MaxLength = commandOption.MaxLength,
			MinLength = commandOption.MinLength
		};
	}

	public static Modal ToModal(this ModalInfo modalInfo, string customId, Action<ModalBuilder> modifyModal = null)
	{
		ModalBuilder modalBuilder = new ModalBuilder(modalInfo.Title, customId);
		foreach (InputComponentInfo component in modalInfo.Components)
		{
			if (component is TextInputComponentInfo textInputComponentInfo)
			{
				modalBuilder.AddTextInput(textInputComponentInfo.Label, textInputComponentInfo.CustomId, textInputComponentInfo.Style, textInputComponentInfo.Placeholder, textInputComponentInfo.IsRequired ? new int?(textInputComponentInfo.MinLength) : ((int?)null), textInputComponentInfo.MaxLength, textInputComponentInfo.IsRequired, textInputComponentInfo.InitialValue);
				continue;
			}
			throw new InvalidOperationException(component.GetType().FullName + " isn't a valid component info class");
		}
		modifyModal?.Invoke(modalBuilder);
		return modalBuilder.Build();
	}

	public static GuildPermission? SanitizeGuildPermissions(this GuildPermission permissions)
	{
		if (permissions != (GuildPermission)0uL)
		{
			return permissions;
		}
		return null;
	}
}
