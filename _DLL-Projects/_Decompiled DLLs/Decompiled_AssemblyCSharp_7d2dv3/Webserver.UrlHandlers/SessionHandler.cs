using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Platform.Steam;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.UrlHandlers;

public class SessionHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string pageBasePath = "/app";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string pageErrorPath = "/app/error/";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string steamOpenIdVerifyUrl = "verifysteamopenid";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string steamLoginUrl = "loginsteam";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string steamLoginName = "Steam OpenID";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string steamLoginFailedPage = "SteamLoginFailed";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string userPassLoginUrl = "login";

	public const string userPassLoginName = "User/pass";

	public SessionHandler()
		: base(null)
	{
	}

	public override void HandleRequest(RequestContext _context)
	{
		if (_context.Request.RemoteEndPoint == null)
		{
			WebUtils.WriteText(_context.Response, "NoRemoteEndpoint", HttpStatusCode.BadRequest);
			return;
		}
		string text = _context.RequestPath.Remove(0, urlBasePath.Length);
		string remoteEndpointString = _context.Request.RemoteEndPoint.ToString();
		if (text.StartsWith("verifysteamopenid"))
		{
			if (HandleSteamVerification(parent.ConnectionHandler, _context, remoteEndpointString))
			{
				_context.Response.Redirect("/app");
			}
			else
			{
				_context.Response.Redirect("/app/error/SteamLoginFailed");
			}
		}
		else if (text.StartsWith("logout"))
		{
			HandleLogout(parent.ConnectionHandler, _context, "/app");
		}
		else if (text.StartsWith("loginsteam"))
		{
			HandleSteamLogin(_context, urlBasePath + "verifysteamopenid");
		}
		else if (text.StartsWith("login"))
		{
			HandleUserPassLogin(parent.ConnectionHandler, _context, remoteEndpointString);
		}
		else
		{
			WebUtils.WriteText(_context.Response, "InvalidSessionsCommand", HttpStatusCode.BadRequest);
		}
	}

	public static bool HandleUserPassLogin(ConnectionHandler _connectionHandler, RequestContext _context, string _remoteEndpointString)
	{
		if (!_context.Request.HasEntityBody)
		{
			WebUtils.WriteText(_context.Response, "NoLoginData", HttpStatusCode.BadRequest);
			return false;
		}
		Stream inputStream = _context.Request.InputStream;
		byte[] array = new byte[_context.Request.ContentLength64];
		inputStream.Read(array, 0, (int)_context.Request.ContentLength64);
		IDictionary<string, object> dictionary;
		try
		{
			dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(array);
		}
		catch (Exception e)
		{
			Log.Error("Error deserializing JSON from user/password login:");
			Log.Exception(e);
			WebUtils.WriteText(_context.Response, "InvalidLoginJson", HttpStatusCode.BadRequest);
			return false;
		}
		if (!dictionary.TryGetValue("username", out var value) || !(value is string name))
		{
			WebUtils.WriteText(_context.Response, "InvalidLoginJson", HttpStatusCode.BadRequest);
			return false;
		}
		if (!dictionary.TryGetValue("password", out value) || !(value is string password))
		{
			WebUtils.WriteText(_context.Response, "InvalidLoginJson", HttpStatusCode.BadRequest);
			return false;
		}
		if (!AdminWebUsers.Instance.TryGetUser(name, password, out var _result))
		{
			WebUtils.WriteText(_context.Response, "UserPassInvalid", HttpStatusCode.Unauthorized);
			Log.Out("[Web] User/pass login failed from " + _remoteEndpointString);
			return false;
		}
		bool num = HandleUserIdLogin(_connectionHandler, _context, _remoteEndpointString, "User/pass", _result.Name, _result.PlatformUser, _result.CrossPlatformUser);
		if (num)
		{
			WebUtils.WriteText(_context.Response, "");
			return num;
		}
		WebUtils.WriteText(_context.Response, "LoginError", HttpStatusCode.InternalServerError);
		return num;
	}

	public static void HandleSteamLogin(RequestContext _context, string _verificationCallbackUrl)
	{
		string text = (WebUtils.IsSslRedirected(_context.Request) ? "https://" : "http://") + _context.Request.UserHostName;
		string openIdLoginUrl = OpenID.GetOpenIdLoginUrl(text, text + _verificationCallbackUrl);
		_context.Response.Redirect(openIdLoginUrl);
	}

	public static bool HandleLogout(ConnectionHandler _connectionHandler, RequestContext _context, string _pageBase)
	{
		Cookie cookie = new Cookie("sid", "", "/")
		{
			Expired = true
		};
		_context.Response.AppendCookie(cookie);
		if (_context.Connection == null)
		{
			_context.Response.Redirect(_pageBase);
			return false;
		}
		_connectionHandler.LogOut(_context.Connection.SessionID);
		_context.Response.Redirect(_pageBase);
		return true;
	}

	public static bool HandleSteamVerification(ConnectionHandler _connectionHandler, RequestContext _context, string _remoteEndpointString)
	{
		ulong num;
		try
		{
			num = OpenID.Validate(_context.Request);
		}
		catch (Exception e)
		{
			Log.Error("[Web] Error validating Steam login from " + _remoteEndpointString + ":");
			Log.Exception(e);
			return false;
		}
		if (num == 0)
		{
			Log.Out("[Web] Steam OpenID login failed (invalid ID) from " + _remoteEndpointString);
			return false;
		}
		UserIdentifierSteam userIdentifierSteam = new UserIdentifierSteam(num);
		return HandleUserIdLogin(_connectionHandler, _context, _remoteEndpointString, "Steam OpenID", userIdentifierSteam.ToString(), userIdentifierSteam);
	}

	public static bool HandleUserIdLogin(ConnectionHandler _connectionHandler, RequestContext _context, string _remoteEndpointString, string _loginName, string _username, PlatformUserIdentifierAbs _userId, PlatformUserIdentifierAbs _crossUserId = null)
	{
		try
		{
			WebConnection webConnection = _connectionHandler.LogIn(_context.Request.RemoteEndPoint.Address, _username, _userId, _crossUserId);
			int userPermissionLevel = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_userId);
			int val = int.MaxValue;
			if (_crossUserId != null)
			{
				val = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_crossUserId);
			}
			int num = Math.Min(userPermissionLevel, val);
			Log.Out(string.Format("[Web] {0} login from {1}, name {2} with ID {3}, CID {4}, permission level {5}", _loginName, _remoteEndpointString, _username, _userId, (_crossUserId != null) ? _crossUserId.ToString() : "none", num));
			Cookie cookie = new Cookie("sid", webConnection.SessionID, "/")
			{
				Expired = false,
				Expires = DateTime.MinValue,
				HttpOnly = true,
				Secure = false
			};
			_context.Response.AppendCookie(cookie);
			return true;
		}
		catch (Exception e)
		{
			Log.Error("[Web] Error during " + _loginName + " login:");
			Log.Exception(e);
		}
		return false;
	}
}
