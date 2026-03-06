using System;

namespace Discord.Interactions.Builders;

internal sealed class ContextCommandBuilder : CommandBuilder<ContextCommandInfo, ContextCommandBuilder, CommandParameterBuilder>
{
	protected override ContextCommandBuilder Instance => this;

	public ApplicationCommandType CommandType { get; set; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	public bool DefaultPermission { get; set; } = true;

	public bool IsEnabledInDm { get; set; } = true;

	public GuildPermission? DefaultMemberPermissions { get; set; }

	internal ContextCommandBuilder(ModuleBuilder module)
		: base(module)
	{
	}

	public ContextCommandBuilder(ModuleBuilder module, string name, ExecuteCallback callback)
		: base(module, name, callback)
	{
	}

	public ContextCommandBuilder SetType(ApplicationCommandType commandType)
	{
		CommandType = commandType;
		return this;
	}

	[Obsolete("To be deprecated soon, use SetEnabledInDm and WithDefaultMemberPermissions instead.")]
	public ContextCommandBuilder SetDefaultPermission(bool defaultPermision)
	{
		DefaultPermission = defaultPermision;
		return this;
	}

	public override ContextCommandBuilder AddParameter(Action<CommandParameterBuilder> configure)
	{
		CommandParameterBuilder commandParameterBuilder = new CommandParameterBuilder(this);
		configure(commandParameterBuilder);
		AddParameters(commandParameterBuilder);
		return this;
	}

	public ContextCommandBuilder SetEnabledInDm(bool isEnabled)
	{
		IsEnabledInDm = isEnabled;
		return this;
	}

	public ContextCommandBuilder WithDefaultMemberPermissions(GuildPermission permissions)
	{
		DefaultMemberPermissions = permissions;
		return this;
	}

	internal override ContextCommandInfo Build(ModuleInfo module, InteractionService commandService)
	{
		return ContextCommandInfo.Create(this, module, commandService);
	}
}
