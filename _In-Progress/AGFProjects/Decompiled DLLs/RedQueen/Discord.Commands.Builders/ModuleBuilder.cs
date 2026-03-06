using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Commands.Builders;

internal class ModuleBuilder
{
	private string _group;

	private readonly List<CommandBuilder> _commands;

	private readonly List<ModuleBuilder> _submodules;

	private readonly List<PreconditionAttribute> _preconditions;

	private readonly List<Attribute> _attributes;

	private readonly List<string> _aliases;

	public CommandService Service { get; }

	public ModuleBuilder Parent { get; }

	public string Name { get; set; }

	public string Summary { get; set; }

	public string Remarks { get; set; }

	public string Group
	{
		get
		{
			return _group;
		}
		set
		{
			_aliases.Remove(_group);
			_group = value;
			AddAliases(value);
		}
	}

	public IReadOnlyList<CommandBuilder> Commands => _commands;

	public IReadOnlyList<ModuleBuilder> Modules => _submodules;

	public IReadOnlyList<PreconditionAttribute> Preconditions => _preconditions;

	public IReadOnlyList<Attribute> Attributes => _attributes;

	public IReadOnlyList<string> Aliases => _aliases;

	internal TypeInfo TypeInfo { get; set; }

	internal ModuleBuilder(CommandService service, ModuleBuilder parent)
	{
		Service = service;
		Parent = parent;
		_commands = new List<CommandBuilder>();
		_submodules = new List<ModuleBuilder>();
		_preconditions = new List<PreconditionAttribute>();
		_attributes = new List<Attribute>();
		_aliases = new List<string>();
	}

	internal ModuleBuilder(CommandService service, ModuleBuilder parent, string primaryAlias)
		: this(service, parent)
	{
		Discord.Preconditions.NotNull(primaryAlias, "primaryAlias");
		_aliases = new List<string> { primaryAlias };
	}

	public ModuleBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public ModuleBuilder WithSummary(string summary)
	{
		Summary = summary;
		return this;
	}

	public ModuleBuilder WithRemarks(string remarks)
	{
		Remarks = remarks;
		return this;
	}

	public ModuleBuilder AddAliases(params string[] aliases)
	{
		for (int i = 0; i < aliases.Length; i++)
		{
			string item = aliases[i] ?? "";
			if (!_aliases.Contains(item))
			{
				_aliases.Add(item);
			}
		}
		return this;
	}

	public ModuleBuilder AddAttributes(params Attribute[] attributes)
	{
		_attributes.AddRange(attributes);
		return this;
	}

	public ModuleBuilder AddPrecondition(PreconditionAttribute precondition)
	{
		_preconditions.Add(precondition);
		return this;
	}

	public ModuleBuilder AddCommand(string primaryAlias, Func<ICommandContext, object[], IServiceProvider, CommandInfo, Task> callback, Action<CommandBuilder> createFunc)
	{
		CommandBuilder commandBuilder = new CommandBuilder(this, primaryAlias, callback);
		createFunc(commandBuilder);
		_commands.Add(commandBuilder);
		return this;
	}

	internal ModuleBuilder AddCommand(Action<CommandBuilder> createFunc)
	{
		CommandBuilder commandBuilder = new CommandBuilder(this);
		createFunc(commandBuilder);
		_commands.Add(commandBuilder);
		return this;
	}

	public ModuleBuilder AddModule(string primaryAlias, Action<ModuleBuilder> createFunc)
	{
		ModuleBuilder moduleBuilder = new ModuleBuilder(Service, this, primaryAlias);
		createFunc(moduleBuilder);
		_submodules.Add(moduleBuilder);
		return this;
	}

	internal ModuleBuilder AddModule(Action<ModuleBuilder> createFunc)
	{
		ModuleBuilder moduleBuilder = new ModuleBuilder(Service, this);
		createFunc(moduleBuilder);
		_submodules.Add(moduleBuilder);
		return this;
	}

	private ModuleInfo BuildImpl(CommandService service, IServiceProvider services, ModuleInfo parent = null)
	{
		if (Name == null)
		{
			Name = _aliases[0];
		}
		if (TypeInfo != null && !TypeInfo.IsAbstract)
		{
			ReflectionUtils.CreateObject<IModuleBase>(TypeInfo, service, services).OnModuleBuilding(service, this);
		}
		return new ModuleInfo(this, service, services, parent);
	}

	public ModuleInfo Build(CommandService service, IServiceProvider services)
	{
		return BuildImpl(service, services);
	}

	internal ModuleInfo Build(CommandService service, IServiceProvider services, ModuleInfo parent)
	{
		return BuildImpl(service, services, parent);
	}
}
