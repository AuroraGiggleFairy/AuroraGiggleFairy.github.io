using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Commands;

internal static class NullableTypeReader
{
	public static TypeReader Create(Type type, TypeReader reader)
	{
		return (TypeReader)typeof(NullableTypeReader<>).MakeGenericType(type).GetTypeInfo().DeclaredConstructors.First().Invoke(new object[1] { reader });
	}
}
internal class NullableTypeReader<T> : TypeReader where T : struct
{
	private readonly TypeReader _baseTypeReader;

	public NullableTypeReader(TypeReader baseTypeReader)
	{
		_baseTypeReader = baseTypeReader;
	}

	public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		if (string.Equals(input, "null", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "nothing", StringComparison.OrdinalIgnoreCase))
		{
			return TypeReaderResult.FromSuccess((object)null);
		}
		return await _baseTypeReader.ReadAsync(context, input, services).ConfigureAwait(continueOnCapturedContext: false);
	}
}
