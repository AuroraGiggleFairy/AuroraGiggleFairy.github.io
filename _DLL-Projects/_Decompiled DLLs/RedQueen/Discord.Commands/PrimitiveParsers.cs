using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord.Commands;

internal static class PrimitiveParsers
{
	private static readonly Lazy<IReadOnlyDictionary<Type, Delegate>> Parsers = new Lazy<IReadOnlyDictionary<Type, Delegate>>(CreateParsers);

	public static IEnumerable<Type> SupportedTypes = Parsers.Value.Keys;

	private static IReadOnlyDictionary<Type, Delegate> CreateParsers()
	{
		ImmutableDictionary<Type, Delegate>.Builder builder = ImmutableDictionary.CreateBuilder<Type, Delegate>();
		builder[typeof(bool)] = new TryParseDelegate<bool>(bool.TryParse);
		builder[typeof(sbyte)] = new TryParseDelegate<sbyte>(sbyte.TryParse);
		builder[typeof(byte)] = new TryParseDelegate<byte>(byte.TryParse);
		builder[typeof(short)] = new TryParseDelegate<short>(short.TryParse);
		builder[typeof(ushort)] = new TryParseDelegate<ushort>(ushort.TryParse);
		builder[typeof(int)] = new TryParseDelegate<int>(int.TryParse);
		builder[typeof(uint)] = new TryParseDelegate<uint>(uint.TryParse);
		builder[typeof(long)] = new TryParseDelegate<long>(long.TryParse);
		builder[typeof(ulong)] = new TryParseDelegate<ulong>(ulong.TryParse);
		builder[typeof(float)] = new TryParseDelegate<float>(float.TryParse);
		builder[typeof(double)] = new TryParseDelegate<double>(double.TryParse);
		builder[typeof(decimal)] = new TryParseDelegate<decimal>(decimal.TryParse);
		builder[typeof(DateTime)] = new TryParseDelegate<DateTime>(DateTime.TryParse);
		builder[typeof(DateTimeOffset)] = new TryParseDelegate<DateTimeOffset>(DateTimeOffset.TryParse);
		builder[typeof(char)] = new TryParseDelegate<char>(char.TryParse);
		return builder.ToImmutable();
	}

	public static TryParseDelegate<T> Get<T>()
	{
		return (TryParseDelegate<T>)Parsers.Value[typeof(T)];
	}

	public static Delegate Get(Type type)
	{
		return Parsers.Value[type];
	}
}
