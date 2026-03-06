using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Commands.Builders;

internal class CommandBuilder
{
	private readonly List<PreconditionAttribute> _preconditions;

	private readonly List<ParameterBuilder> _parameters;

	private readonly List<Attribute> _attributes;

	private readonly List<string> _aliases;

	public ModuleBuilder Module { get; }

	internal Func<ICommandContext, object[], IServiceProvider, CommandInfo, Task> Callback { get; set; }

	public string Name { get; set; }

	public string Summary { get; set; }

	public string Remarks { get; set; }

	public string PrimaryAlias { get; set; }

	public RunMode RunMode { get; set; }

	public int Priority { get; set; }

	public bool IgnoreExtraArgs { get; set; }

	public IReadOnlyList<PreconditionAttribute> Preconditions => _preconditions;

	public IReadOnlyList<ParameterBuilder> Parameters => _parameters;

	public IReadOnlyList<Attribute> Attributes => _attributes;

	public IReadOnlyList<string> Aliases => _aliases;

	internal CommandBuilder(ModuleBuilder module)
	{
		Module = module;
		_preconditions = new List<PreconditionAttribute>();
		_parameters = new List<ParameterBuilder>();
		_attributes = new List<Attribute>();
		_aliases = new List<string>();
	}

	internal CommandBuilder(ModuleBuilder module, string primaryAlias, Func<ICommandContext, object[], IServiceProvider, CommandInfo, Task> callback)
		: this(module)
	{
		Discord.Preconditions.NotNull(primaryAlias, "primaryAlias");
		Discord.Preconditions.NotNull(callback, "callback");
		Callback = callback;
		PrimaryAlias = primaryAlias;
		_aliases.Add(primaryAlias);
	}

	public CommandBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public CommandBuilder WithSummary(string summary)
	{
		Summary = summary;
		return this;
	}

	public CommandBuilder WithRemarks(string remarks)
	{
		Remarks = remarks;
		return this;
	}

	public CommandBuilder WithRunMode(RunMode runMode)
	{
		RunMode = runMode;
		return this;
	}

	public CommandBuilder WithPriority(int priority)
	{
		Priority = priority;
		return this;
	}

	public CommandBuilder AddAliases(params string[] aliases)
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

	public CommandBuilder AddAttributes(params Attribute[] attributes)
	{
		_attributes.AddRange(attributes);
		return this;
	}

	public CommandBuilder AddPrecondition(PreconditionAttribute precondition)
	{
		_preconditions.Add(precondition);
		return this;
	}

	public CommandBuilder AddParameter<T>(string name, Action<ParameterBuilder> createFunc)
	{
		ParameterBuilder parameterBuilder = new ParameterBuilder(this, name, typeof(T));
		createFunc(parameterBuilder);
		_parameters.Add(parameterBuilder);
		return this;
	}

	public CommandBuilder AddParameter(string name, Type type, Action<ParameterBuilder> createFunc)
	{
		ParameterBuilder parameterBuilder = new ParameterBuilder(this, name, type);
		createFunc(parameterBuilder);
		_parameters.Add(parameterBuilder);
		return this;
	}

	internal CommandBuilder AddParameter(Action<ParameterBuilder> createFunc)
	{
		ParameterBuilder parameterBuilder = new ParameterBuilder(this);
		createFunc(parameterBuilder);
		_parameters.Add(parameterBuilder);
		return this;
	}

	internal CommandInfo Build(ModuleInfo info, CommandService service)
	{
		if (Name == null)
		{
			Name = PrimaryAlias;
		}
		if (_parameters.Count > 0)
		{
			ParameterBuilder parameterBuilder = _parameters[_parameters.Count - 1];
			ParameterBuilder parameterBuilder2 = _parameters.FirstOrDefault((ParameterBuilder x) => x.IsMultiple);
			if (parameterBuilder2 != null && parameterBuilder2 != parameterBuilder)
			{
				throw new InvalidOperationException("Only the last parameter in a command may have the Multiple flag. Parameter: " + parameterBuilder2.Name + " in " + PrimaryAlias);
			}
			ParameterBuilder parameterBuilder3 = _parameters.FirstOrDefault((ParameterBuilder x) => x.IsRemainder);
			if (parameterBuilder3 != null && parameterBuilder3 != parameterBuilder)
			{
				throw new InvalidOperationException("Only the last parameter in a command may have the Remainder flag. Parameter: " + parameterBuilder3.Name + " in " + PrimaryAlias);
			}
		}
		return new CommandInfo(this, info, service);
	}
}
