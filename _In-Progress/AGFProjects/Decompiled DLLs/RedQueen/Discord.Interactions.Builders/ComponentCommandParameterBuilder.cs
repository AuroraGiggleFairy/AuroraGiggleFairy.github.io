using System;

namespace Discord.Interactions.Builders;

internal class ComponentCommandParameterBuilder : ParameterBuilder<ComponentCommandParameterInfo, ComponentCommandParameterBuilder>
{
	public ComponentTypeConverter TypeConverter { get; private set; }

	public TypeReader TypeReader { get; private set; }

	public bool IsRouteSegmentParameter { get; private set; }

	protected override ComponentCommandParameterBuilder Instance => this;

	internal ComponentCommandParameterBuilder(ICommandBuilder command)
		: base(command)
	{
	}

	public ComponentCommandParameterBuilder(ICommandBuilder command, string name, Type type)
		: base(command, name, type)
	{
	}

	public override ComponentCommandParameterBuilder SetParameterType(Type type)
	{
		return SetParameterType(type, null);
	}

	public ComponentCommandParameterBuilder SetParameterType(Type type, IServiceProvider services)
	{
		base.SetParameterType(type);
		if (IsRouteSegmentParameter)
		{
			TypeReader = base.Command.Module.InteractionService.GetTypeReader(type);
		}
		else
		{
			TypeConverter = base.Command.Module.InteractionService.GetComponentTypeConverter(base.ParameterType, services);
		}
		return this;
	}

	public ComponentCommandParameterBuilder SetIsRouteSegment(bool isRouteSegment)
	{
		IsRouteSegmentParameter = isRouteSegment;
		return this;
	}

	internal override ComponentCommandParameterInfo Build(ICommandInfo command)
	{
		return new ComponentCommandParameterInfo(this, command);
	}
}
