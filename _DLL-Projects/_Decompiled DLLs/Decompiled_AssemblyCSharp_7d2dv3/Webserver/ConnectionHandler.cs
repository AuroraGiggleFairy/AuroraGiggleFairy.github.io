using System;
using System.Collections.Generic;
using System.Net;

namespace Webserver;

public class ConnectionHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, WebConnection> connections = new Dictionary<string, WebConnection>();

	public WebConnection IsLoggedIn(string _sessionId, IPAddress _ip)
	{
		if (!connections.TryGetValue(_sessionId, out var value))
		{
			return null;
		}
		value.UpdateUsage();
		return value;
	}

	public void LogOut(string _sessionId)
	{
		connections.Remove(_sessionId);
	}

	public WebConnection LogIn(IPAddress _ip, string _username, PlatformUserIdentifierAbs _userId, PlatformUserIdentifierAbs _crossUserId = null)
	{
		string text = Guid.NewGuid().ToString();
		WebConnection webConnection = new WebConnection(text, _ip, _username, _userId, _crossUserId);
		connections.Add(text, webConnection);
		return webConnection;
	}

	public void SendLine(string _line)
	{
		foreach (KeyValuePair<string, WebConnection> connection in connections)
		{
			connection.Value.SendLine(_line);
		}
	}
}
