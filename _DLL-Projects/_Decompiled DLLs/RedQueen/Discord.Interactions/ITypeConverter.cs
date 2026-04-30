using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal interface ITypeConverter<T>
{
	bool CanConvertTo(Type type);

	Task<TypeConverterResult> ReadAsync(IInteractionContext context, T option, IServiceProvider services);
}
