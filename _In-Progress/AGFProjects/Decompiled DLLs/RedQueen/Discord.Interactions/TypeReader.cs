using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal abstract class TypeReader : ITypeConverter<string>
{
	public abstract bool CanConvertTo(Type type);

	public abstract Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services);

	public virtual Task<string> SerializeAsync(object obj, IServiceProvider services)
	{
		return Task.FromResult(obj.ToString());
	}
}
internal abstract class TypeReader<T> : TypeReader
{
	public sealed override bool CanConvertTo(Type type)
	{
		return typeof(T).IsAssignableFrom(type);
	}
}
