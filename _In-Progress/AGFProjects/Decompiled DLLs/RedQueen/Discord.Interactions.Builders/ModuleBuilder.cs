using System;
using System.Collections.Generic;
using System.Reflection;

namespace Discord.Interactions.Builders;

internal class ModuleBuilder
{
	private readonly List<Attribute> _attributes;

	private readonly List<PreconditionAttribute> _preconditions;

	private readonly List<ModuleBuilder> _subModules;

	private readonly List<SlashCommandBuilder> _slashCommands;

	private readonly List<ContextCommandBuilder> _contextCommands;

	private readonly List<ComponentCommandBuilder> _componentCommands;

	private readonly List<AutocompleteCommandBuilder> _autocompleteCommands;

	private readonly List<ModalCommandBuilder> _modalCommands;

	public InteractionService InteractionService { get; }

	public ModuleBuilder Parent { get; }

	public string Name { get; internal set; }

	public string SlashGroupName { get; set; }

	public bool IsSlashGroup => !string.IsNullOrEmpty(SlashGroupName);

	public string Description { get; set; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	public bool DefaultPermission { get; set; } = true;

	public bool IsEnabledInDm { get; set; } = true;

	public GuildPermission? DefaultMemberPermissions { get; set; }

	public bool DontAutoRegister { get; set; }

	public IReadOnlyList<Attribute> Attributes => _attributes;

	public IReadOnlyCollection<PreconditionAttribute> Preconditions => _preconditions;

	public IReadOnlyList<ModuleBuilder> SubModules => _subModules;

	public IReadOnlyList<SlashCommandBuilder> SlashCommands => _slashCommands;

	public IReadOnlyList<ContextCommandBuilder> ContextCommands => _contextCommands;

	public IReadOnlyList<ComponentCommandBuilder> ComponentCommands => _componentCommands;

	public IReadOnlyList<AutocompleteCommandBuilder> AutocompleteCommands => _autocompleteCommands;

	public IReadOnlyList<ModalCommandBuilder> ModalCommands => _modalCommands;

	internal TypeInfo TypeInfo { get; set; }

	internal ModuleBuilder(InteractionService interactionService, ModuleBuilder parent = null)
	{
		InteractionService = interactionService;
		Parent = parent;
		_attributes = new List<Attribute>();
		_subModules = new List<ModuleBuilder>();
		_slashCommands = new List<SlashCommandBuilder>();
		_contextCommands = new List<ContextCommandBuilder>();
		_componentCommands = new List<ComponentCommandBuilder>();
		_autocompleteCommands = new List<AutocompleteCommandBuilder>();
		_modalCommands = new List<ModalCommandBuilder>();
		_preconditions = new List<PreconditionAttribute>();
	}

	public ModuleBuilder(InteractionService interactionService, string name, ModuleBuilder parent = null)
		: this(interactionService, parent)
	{
		Name = name;
	}

	public ModuleBuilder WithGroupName(string name)
	{
		SlashGroupName = name;
		return this;
	}

	public ModuleBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	[Obsolete("To be deprecated soon, use SetEnabledInDm and WithDefaultMemberPermissions instead.")]
	public ModuleBuilder WithDefaultPermission(bool permission)
	{
		DefaultPermission = permission;
		return this;
	}

	public ModuleBuilder SetEnabledInDm(bool isEnabled)
	{
		IsEnabledInDm = isEnabled;
		return this;
	}

	public ModuleBuilder WithDefaultMemberPermissions(GuildPermission permissions)
	{
		DefaultMemberPermissions = permissions;
		return this;
	}

	public ModuleBuilder AddAttributes(params Attribute[] attributes)
	{
		_attributes.AddRange(attributes);
		return this;
	}

	public ModuleBuilder AddPreconditions(params PreconditionAttribute[] preconditions)
	{
		_preconditions.AddRange(preconditions);
		return this;
	}

	public ModuleBuilder AddSlashCommand(Action<SlashCommandBuilder> configure)
	{
		SlashCommandBuilder slashCommandBuilder = new SlashCommandBuilder(this);
		configure(slashCommandBuilder);
		_slashCommands.Add(slashCommandBuilder);
		return this;
	}

	public ModuleBuilder AddSlashCommand(string name, ExecuteCallback callback, Action<SlashCommandBuilder> configure)
	{
		SlashCommandBuilder slashCommandBuilder = new SlashCommandBuilder(this, name, callback);
		configure(slashCommandBuilder);
		_slashCommands.Add(slashCommandBuilder);
		return this;
	}

	public ModuleBuilder AddContextCommand(Action<ContextCommandBuilder> configure)
	{
		ContextCommandBuilder contextCommandBuilder = new ContextCommandBuilder(this);
		configure(contextCommandBuilder);
		_contextCommands.Add(contextCommandBuilder);
		return this;
	}

	public ModuleBuilder AddContextCommand(string name, ExecuteCallback callback, Action<ContextCommandBuilder> configure)
	{
		ContextCommandBuilder contextCommandBuilder = new ContextCommandBuilder(this, name, callback);
		configure(contextCommandBuilder);
		_contextCommands.Add(contextCommandBuilder);
		return this;
	}

	public ModuleBuilder AddComponentCommand(Action<ComponentCommandBuilder> configure)
	{
		ComponentCommandBuilder componentCommandBuilder = new ComponentCommandBuilder(this);
		configure(componentCommandBuilder);
		_componentCommands.Add(componentCommandBuilder);
		return this;
	}

	public ModuleBuilder AddComponentCommand(string name, ExecuteCallback callback, Action<ComponentCommandBuilder> configure)
	{
		ComponentCommandBuilder componentCommandBuilder = new ComponentCommandBuilder(this, name, callback);
		configure(componentCommandBuilder);
		_componentCommands.Add(componentCommandBuilder);
		return this;
	}

	public ModuleBuilder AddAutocompleteCommand(Action<AutocompleteCommandBuilder> configure)
	{
		AutocompleteCommandBuilder autocompleteCommandBuilder = new AutocompleteCommandBuilder(this);
		configure(autocompleteCommandBuilder);
		_autocompleteCommands.Add(autocompleteCommandBuilder);
		return this;
	}

	public ModuleBuilder AddSlashCommand(string name, ExecuteCallback callback, Action<AutocompleteCommandBuilder> configure)
	{
		AutocompleteCommandBuilder autocompleteCommandBuilder = new AutocompleteCommandBuilder(this, name, callback);
		configure(autocompleteCommandBuilder);
		_autocompleteCommands.Add(autocompleteCommandBuilder);
		return this;
	}

	public ModuleBuilder AddModalCommand(Action<ModalCommandBuilder> configure)
	{
		ModalCommandBuilder modalCommandBuilder = new ModalCommandBuilder(this);
		configure(modalCommandBuilder);
		_modalCommands.Add(modalCommandBuilder);
		return this;
	}

	public ModuleBuilder AddModule(Action<ModuleBuilder> configure)
	{
		ModuleBuilder moduleBuilder = new ModuleBuilder(InteractionService, this);
		configure(moduleBuilder);
		_subModules.Add(moduleBuilder);
		return this;
	}

	internal ModuleInfo Build(InteractionService interactionService, IServiceProvider services, ModuleInfo parent = null)
	{
		if ((object)TypeInfo != null && ModuleClassBuilder.IsValidModuleDefinition(TypeInfo))
		{
			IInteractionModuleBase interactionModuleBase = ReflectionUtils<IInteractionModuleBase>.CreateObject(TypeInfo, interactionService, services);
			try
			{
				interactionModuleBase.Construct(this, interactionService);
				ModuleInfo moduleInfo = new ModuleInfo(this, interactionService, services, parent);
				interactionModuleBase.OnModuleBuilding(interactionService, moduleInfo);
				return moduleInfo;
			}
			finally
			{
				(interactionModuleBase as IDisposable)?.Dispose();
			}
		}
		return new ModuleInfo(this, interactionService, services, parent);
	}
}
