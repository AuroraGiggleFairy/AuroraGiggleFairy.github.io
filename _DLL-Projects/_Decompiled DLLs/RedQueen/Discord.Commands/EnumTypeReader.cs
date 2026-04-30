using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Commands;

internal static class EnumTypeReader
{
	public static TypeReader GetReader(Type type)
	{
		Type underlyingType = Enum.GetUnderlyingType(type);
		return (TypeReader)typeof(EnumTypeReader<>).MakeGenericType(underlyingType).GetTypeInfo().DeclaredConstructors.First().Invoke(new object[2]
		{
			type,
			PrimitiveParsers.Get(underlyingType)
		});
	}
}
internal class EnumTypeReader<T> : TypeReader
{
	private readonly IReadOnlyDictionary<string, object> _enumsByName;

	private readonly IReadOnlyDictionary<T, object> _enumsByValue;

	private readonly Type _enumType;

	private readonly TryParseDelegate<T> _tryParse;

	public EnumTypeReader(Type type, TryParseDelegate<T> parser)
	{
		_enumType = type;
		_tryParse = parser;
		ImmutableDictionary<string, object>.Builder builder = ImmutableDictionary.CreateBuilder<string, object>();
		ImmutableDictionary<T, object>.Builder builder2 = ImmutableDictionary.CreateBuilder<T, object>();
		string[] names = Enum.GetNames(_enumType);
		foreach (string text in names)
		{
			object obj = Enum.Parse(_enumType, text);
			builder.Add(text.ToLower(), obj);
			if (!builder2.ContainsKey((T)obj))
			{
				builder2.Add((T)obj, obj);
			}
		}
		_enumsByName = builder.ToImmutable();
		_enumsByValue = builder2.ToImmutable();
	}

	public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		object value2;
		if (_tryParse(input, out var value))
		{
			if (_enumsByValue.TryGetValue(value, out value2))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(value2));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Value is not a " + _enumType.Name + "."));
		}
		if (_enumsByName.TryGetValue(input.ToLower(), out value2))
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(value2));
		}
		return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Value is not a " + _enumType.Name + "."));
	}
}
