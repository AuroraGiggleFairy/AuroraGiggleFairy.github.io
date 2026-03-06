using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class ComponentCommandParameterInfo : CommandParameterInfo
{
	public ComponentTypeConverter TypeConverter { get; }

	public TypeReader TypeReader { get; }

	public bool IsRouteSegmentParameter { get; }

	internal ComponentCommandParameterInfo(ComponentCommandParameterBuilder builder, ICommandInfo command)
		: base(builder, command)
	{
		TypeConverter = builder.TypeConverter;
		TypeReader = builder.TypeReader;
		IsRouteSegmentParameter = builder.IsRouteSegmentParameter;
	}
}
