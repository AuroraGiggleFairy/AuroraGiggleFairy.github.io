using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class AliasAttribute : Attribute
{
	public string[] Aliases { get; }

	public AliasAttribute(params string[] aliases)
	{
		Aliases = aliases;
	}
}
