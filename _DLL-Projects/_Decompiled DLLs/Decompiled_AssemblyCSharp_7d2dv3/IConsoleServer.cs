using System;
using UnityEngine;

public interface IConsoleServer
{
	void Disconnect();

	void SendLine(string _line);

	void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime);
}
