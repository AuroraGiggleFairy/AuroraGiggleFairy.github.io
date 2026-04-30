using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class ModuleInfo
{
	internal ILookup<string, PreconditionAttribute> GroupedPreconditions { get; }

	public InteractionService CommandService { get; }

	public string Name { get; }

	public string SlashGroupName { get; }

	public bool IsSlashGroup => !string.IsNullOrEmpty(SlashGroupName);

	public string Description { get; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	public bool DefaultPermission { get; }

	public bool IsEnabledInDm { get; }

	public GuildPermission? DefaultMemberPermissions { get; }

	public IReadOnlyList<ModuleInfo> SubModules { get; }

	public IReadOnlyList<SlashCommandInfo> SlashCommands { get; }

	public IReadOnlyList<ContextCommandInfo> ContextCommands { get; }

	public IReadOnlyCollection<ComponentCommandInfo> ComponentCommands { get; }

	public IReadOnlyCollection<AutocompleteCommandInfo> AutocompleteCommands { get; }

	public IReadOnlyCollection<ModalCommandInfo> ModalCommands { get; }

	public ModuleInfo Parent { get; }

	public bool IsSubModule => Parent != null;

	public IReadOnlyCollection<Attribute> Attributes { get; }

	public IReadOnlyCollection<PreconditionAttribute> Preconditions { get; }

	public bool IsTopLevelGroup { get; }

	public bool DontAutoRegister { get; }

	internal ModuleInfo(ModuleBuilder builder, InteractionService commandService, IServiceProvider services, ModuleInfo parent = null)
	{
		CommandService = commandService;
		Name = builder.Name;
		SlashGroupName = builder.SlashGroupName;
		Description = builder.Description;
		Parent = parent;
		DefaultPermission = builder.DefaultPermission;
		IsEnabledInDm = builder.IsEnabledInDm;
		DefaultMemberPermissions = BuildDefaultMemberPermissions(builder);
		SlashCommands = BuildSlashCommands(builder).ToImmutableArray();
		ContextCommands = BuildContextCommands(builder).ToImmutableArray();
		ComponentCommands = BuildComponentCommands(builder).ToImmutableArray();
		AutocompleteCommands = BuildAutocompleteCommands(builder).ToImmutableArray();
		ModalCommands = BuildModalCommands(builder).ToImmutableArray();
		SubModules = BuildSubModules(builder, commandService, services).ToImmutableArray();
		Attributes = BuildAttributes(builder).ToImmutableArray();
		Preconditions = BuildPreconditions(builder).ToImmutableArray();
		IsTopLevelGroup = IsSlashGroup && CheckTopLevel(parent);
		DontAutoRegister = builder.DontAutoRegister;
		GroupedPreconditions = Preconditions.ToLookup((PreconditionAttribute x) => x.Group, (PreconditionAttribute x) => x, StringComparer.Ordinal);
	}

	private IEnumerable<ModuleInfo> BuildSubModules(ModuleBuilder builder, InteractionService commandService, IServiceProvider services)
	{
		List<ModuleInfo> list = new List<ModuleInfo>();
		foreach (ModuleBuilder subModule in builder.SubModules)
		{
			list.Add(subModule.Build(commandService, services, this));
		}
		return list;
	}

	private IEnumerable<SlashCommandInfo> BuildSlashCommands(ModuleBuilder builder)
	{
		List<SlashCommandInfo> list = new List<SlashCommandInfo>();
		foreach (Discord.Interactions.Builders.SlashCommandBuilder slashCommand in builder.SlashCommands)
		{
			list.Add(slashCommand.Build(this, CommandService));
		}
		return list;
	}

	private IEnumerable<ContextCommandInfo> BuildContextCommands(ModuleBuilder builder)
	{
		List<ContextCommandInfo> list = new List<ContextCommandInfo>();
		foreach (ContextCommandBuilder contextCommand in builder.ContextCommands)
		{
			list.Add(contextCommand.Build(this, CommandService));
		}
		return list;
	}

	private IEnumerable<ComponentCommandInfo> BuildComponentCommands(ModuleBuilder builder)
	{
		List<ComponentCommandInfo> list = new List<ComponentCommandInfo>();
		foreach (ComponentCommandBuilder componentCommand in builder.ComponentCommands)
		{
			list.Add(componentCommand.Build(this, CommandService));
		}
		return list;
	}

	private IEnumerable<AutocompleteCommandInfo> BuildAutocompleteCommands(ModuleBuilder builder)
	{
		List<AutocompleteCommandInfo> list = new List<AutocompleteCommandInfo>();
		foreach (AutocompleteCommandBuilder autocompleteCommand in builder.AutocompleteCommands)
		{
			list.Add(autocompleteCommand.Build(this, CommandService));
		}
		return list;
	}

	private IEnumerable<ModalCommandInfo> BuildModalCommands(ModuleBuilder builder)
	{
		List<ModalCommandInfo> list = new List<ModalCommandInfo>();
		foreach (ModalCommandBuilder modalCommand in builder.ModalCommands)
		{
			list.Add(modalCommand.Build(this, CommandService));
		}
		return list;
	}

	private IEnumerable<Attribute> BuildAttributes(ModuleBuilder builder)
	{
		List<Attribute> list = new List<Attribute>();
		for (ModuleBuilder moduleBuilder = builder; moduleBuilder != null; moduleBuilder = moduleBuilder.Parent)
		{
			list.AddRange(moduleBuilder.Attributes);
		}
		return list;
	}

	private static IEnumerable<PreconditionAttribute> BuildPreconditions(ModuleBuilder builder)
	{
		List<PreconditionAttribute> list = new List<PreconditionAttribute>();
		for (ModuleBuilder moduleBuilder = builder; moduleBuilder != null; moduleBuilder = moduleBuilder.Parent)
		{
			list.AddRange(moduleBuilder.Preconditions);
		}
		return list;
	}

	private static bool CheckTopLevel(ModuleInfo parent)
	{
		for (ModuleInfo moduleInfo = parent; moduleInfo != null; moduleInfo = moduleInfo.Parent)
		{
			if (moduleInfo.IsSlashGroup)
			{
				return false;
			}
		}
		return true;
	}

	private static GuildPermission? BuildDefaultMemberPermissions(ModuleBuilder builder)
	{
		GuildPermission? result = builder.DefaultMemberPermissions;
		for (ModuleBuilder parent = builder.Parent; parent != null; parent = parent.Parent)
		{
			result = result.GetValueOrDefault() | parent.DefaultMemberPermissions.GetValueOrDefault().SanitizeGuildPermissions();
		}
		return result;
	}
}
