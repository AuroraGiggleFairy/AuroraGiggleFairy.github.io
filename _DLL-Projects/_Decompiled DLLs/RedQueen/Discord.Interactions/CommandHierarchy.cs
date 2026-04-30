using System;
using System.Collections.Generic;

namespace Discord.Interactions;

internal static class CommandHierarchy
{
	public const char EscapeChar = '$';

	public static IList<string> GetModulePath(this ModuleInfo moduleInfo)
	{
		List<string> list = new List<string>();
		for (ModuleInfo moduleInfo2 = moduleInfo; moduleInfo2 != null; moduleInfo2 = moduleInfo2.Parent)
		{
			if (moduleInfo2.IsSlashGroup)
			{
				list.Insert(0, moduleInfo2.SlashGroupName);
			}
		}
		return list;
	}

	public static IList<string> GetCommandPath(this ICommandInfo commandInfo)
	{
		if (commandInfo.IgnoreGroupNames)
		{
			return new List<string> { commandInfo.Name };
		}
		IList<string> modulePath = commandInfo.Module.GetModulePath();
		modulePath.Add(commandInfo.Name);
		return modulePath;
	}

	public static IList<string> GetParameterPath(this IParameterInfo parameterInfo)
	{
		IList<string> commandPath = parameterInfo.Command.GetCommandPath();
		commandPath.Add(parameterInfo.Name);
		return commandPath;
	}

	public static IList<string> GetChoicePath(this IParameterInfo parameterInfo, ParameterChoice choice)
	{
		IList<string> parameterPath = parameterInfo.GetParameterPath();
		parameterPath.Add(choice.Name);
		return parameterPath;
	}

	public static IList<string> GetTypePath(Type type)
	{
		return new List<string> { "$" + type.FullName };
	}
}
