using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal abstract class ComponentTypeConverter : ITypeConverter<IComponentInteractionData>
{
	public abstract bool CanConvertTo(Type type);

	public abstract Task<TypeConverterResult> ReadAsync(IInteractionContext context, IComponentInteractionData option, IServiceProvider services);
}
internal abstract class ComponentTypeConverter<T> : ComponentTypeConverter
{
	public sealed override bool CanConvertTo(Type type)
	{
		return typeof(T).IsAssignableFrom(type);
	}
}
