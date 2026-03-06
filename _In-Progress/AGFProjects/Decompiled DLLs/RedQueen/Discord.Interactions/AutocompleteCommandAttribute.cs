using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class AutocompleteCommandAttribute : Attribute
{
	public string ParameterName { get; }

	public string CommandName { get; }

	public RunMode RunMode { get; }

	public AutocompleteCommandAttribute(string parameterName, string commandName, RunMode runMode = RunMode.Default)
	{
		ParameterName = parameterName;
		CommandName = commandName;
		RunMode = runMode;
	}
}
