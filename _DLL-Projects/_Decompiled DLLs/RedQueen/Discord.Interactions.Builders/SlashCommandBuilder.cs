using System;

namespace Discord.Interactions.Builders;

internal sealed class SlashCommandBuilder : CommandBuilder<SlashCommandInfo, SlashCommandBuilder, SlashCommandParameterBuilder>
{
	protected override SlashCommandBuilder Instance => this;

	public string Description { get; set; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	public bool DefaultPermission { get; set; } = true;

	public bool IsEnabledInDm { get; set; } = true;

	public GuildPermission? DefaultMemberPermissions { get; set; }

	internal SlashCommandBuilder(ModuleBuilder module)
		: base(module)
	{
	}

	public SlashCommandBuilder(ModuleBuilder module, string name, ExecuteCallback callback)
		: base(module, name, callback)
	{
	}

	public SlashCommandBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	[Obsolete("To be deprecated soon, use SetEnabledInDm and WithDefaultMemberPermissions instead.")]
	public SlashCommandBuilder WithDefaultPermission(bool permission)
	{
		DefaultPermission = permission;
		return Instance;
	}

	public override SlashCommandBuilder AddParameter(Action<SlashCommandParameterBuilder> configure)
	{
		SlashCommandParameterBuilder slashCommandParameterBuilder = new SlashCommandParameterBuilder(this);
		configure(slashCommandParameterBuilder);
		AddParameters(slashCommandParameterBuilder);
		return this;
	}

	public SlashCommandBuilder SetEnabledInDm(bool isEnabled)
	{
		IsEnabledInDm = isEnabled;
		return this;
	}

	public SlashCommandBuilder WithDefaultMemberPermissions(GuildPermission permissions)
	{
		DefaultMemberPermissions = permissions;
		return this;
	}

	internal override SlashCommandInfo Build(ModuleInfo module, InteractionService commandService)
	{
		return new SlashCommandInfo(this, module, commandService);
	}
}
