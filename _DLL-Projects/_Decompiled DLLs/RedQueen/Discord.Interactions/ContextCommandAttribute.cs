using System;
using System.Reflection;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal abstract class ContextCommandAttribute : Attribute
{
	public string Name { get; }

	public ApplicationCommandType CommandType { get; }

	public RunMode RunMode { get; }

	internal ContextCommandAttribute(string name, ApplicationCommandType commandType, RunMode runMode = RunMode.Default)
	{
		Name = name;
		CommandType = commandType;
		RunMode = runMode;
	}

	internal virtual void CheckMethodDefinition(MethodInfo methodInfo)
	{
	}
}
