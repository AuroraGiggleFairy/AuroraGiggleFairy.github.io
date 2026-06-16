using System;
using System.Collections.Generic;
using UnityEngine;

public interface IConsoleConnection
{
	void SendLines(List<string> _output);

	void SendLine(string _text);

	void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime);

	void EnableLogLevel(LogType _type, bool _enable);

	string GetDescription();
}
