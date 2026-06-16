using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Webserver;

public class WebConnection : ConsoleConnectionAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DateTime login;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string conDescription;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string SessionID { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPAddress Endpoint { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Username { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs UserId { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs CrossplatformUserId { get; }

	public TimeSpan Age => DateTime.Now - lastAction;

	public WebConnection(string _sessionId, IPAddress _endpoint, string _username, PlatformUserIdentifierAbs _userId, PlatformUserIdentifierAbs _crossUserId = null)
	{
		SessionID = _sessionId;
		Endpoint = _endpoint;
		Username = _username;
		UserId = _userId;
		CrossplatformUserId = _crossUserId;
		login = DateTime.Now;
		lastAction = login;
		conDescription = $"WebPanel from {Endpoint}";
	}

	public void UpdateUsage()
	{
		lastAction = DateTime.Now;
	}

	public override string GetDescription()
	{
		return conDescription;
	}

	public override void SendLine(string _text)
	{
	}

	public override void SendLines(List<string> _output)
	{
	}

	public override void SendLog(string _formattedMsg, string _plainMsg, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
	}
}
