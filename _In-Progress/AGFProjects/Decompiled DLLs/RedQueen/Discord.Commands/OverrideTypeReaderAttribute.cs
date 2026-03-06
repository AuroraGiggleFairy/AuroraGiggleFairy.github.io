using System;
using System.Reflection;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal sealed class OverrideTypeReaderAttribute : Attribute
{
	private static readonly TypeInfo TypeReaderTypeInfo = typeof(TypeReader).GetTypeInfo();

	public Type TypeReader { get; }

	public OverrideTypeReaderAttribute(Type overridenTypeReader)
	{
		if (!TypeReaderTypeInfo.IsAssignableFrom(overridenTypeReader.GetTypeInfo()))
		{
			throw new ArgumentException("overridenTypeReader must inherit from TypeReader.");
		}
		TypeReader = overridenTypeReader;
	}
}
