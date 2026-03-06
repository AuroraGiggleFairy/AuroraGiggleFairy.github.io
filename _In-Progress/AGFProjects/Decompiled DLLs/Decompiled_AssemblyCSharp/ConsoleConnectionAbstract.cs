using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConsoleConnectionAbstract : IConsoleConnection
{
	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<LogType> enabledLogLevels = new HashSet<LogType>
	{
		LogType.Log,
		LogType.Warning,
		LogType.Error,
		LogType.Exception,
		LogType.Assert
	};

	public abstract void SendLines(List<string> _output);

	public abstract void SendLine(string _text);

	public abstract void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime);

	public abstract string GetDescription();

	public void EnableLogLevel(LogType _type, bool _enable)
	{
		if (_enable)
		{
			enabledLogLevels.Add(_type);
		}
		else
		{
			enabledLogLevels.Remove(_type);
		}
	}

	public bool IsLogLevelEnabled(LogType _type)
	{
		return enabledLogLevels.Contains(_type);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ConsoleConnectionAbstract()
	{
	}
}
