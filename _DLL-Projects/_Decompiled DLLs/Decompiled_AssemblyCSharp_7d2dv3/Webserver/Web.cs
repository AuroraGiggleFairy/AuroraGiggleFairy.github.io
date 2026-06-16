using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SpaceWizards.HttpListener;
using UnityEngine;
using UnityEngine.Profiling;
using Webserver.FileCache;
using Webserver.Permissions;
using Webserver.UrlHandlers;
using Webserver.WebAPI;

namespace Webserver;

public class Web : IConsoleServer
{
	public const string DataPath = "Data/Web";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string indexPageUrl = "/app";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<AbsHandler> handlers = new List<AbsHandler>();

	public readonly List<WebMod> WebMods = new List<WebMod>();

	public readonly ConnectionHandler ConnectionHandler;

	public readonly OpenApiHelpers OpenApiHelpers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SpaceWizards.HttpListener.HttpListener listener = new SpaceWizards.HttpListener.HttpListener();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Version httpProtocolVersion = new Version(1, 1);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AsyncCallback handleRequestDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shutdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CustomSampler authSampler = CustomSampler.Create("Auth");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CustomSampler cookieSampler = CustomSampler.Create("ConCookie");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CustomSampler handlerSampler = CustomSampler.Create("Handler");

	public static event Action<Web> ServerInitialized;

	public Web()
	{
		try
		{
			if (!GamePrefs.GetBool(EnumUtils.Parse<EnumGamePrefs>("WebDashboardEnabled")))
			{
				Log.Out("[Web] Webserver not started, WebDashboardEnabled set to false");
				return;
			}
			int num = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("WebDashboardPort"));
			if (num < 1 || num > 65533)
			{
				Log.Out("[Web] Webserver not started (WebDashboardPort not within 1-65535)");
				return;
			}
			if (!SpaceWizards.HttpListener.HttpListener.IsSupported)
			{
				Log.Out("[Web] Webserver not started (HttpListener.IsSupported returned false)");
				return;
			}
			if (string.IsNullOrEmpty(GamePrefs.GetString(EnumUtils.Parse<EnumGamePrefs>("WebDashboardUrl"))))
			{
				Log.Warning("[Web] WebDashboardUrl not set. Recommended to set it to the public URL pointing to your dashboard / reverse proxy");
			}
			ConnectionHandler = new ConnectionHandler();
			OpenApiHelpers = new OpenApiHelpers();
			RegisterDefaultHandlers();
			Web.ServerInitialized?.Invoke(this);
			listener.Prefixes.Add($"http://+:{num}/");
			listener.Start();
			handleRequestDelegate = HandleRequest;
			listener.BeginGetContext(handleRequestDelegate, listener);
			SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterServer(this);
			Log.Out($"[Web] Started Webserver on port {num}");
		}
		catch (Exception e)
		{
			Log.Error("[Web] Error in Web.ctor: ");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegisterDefaultHandlers()
	{
		bool flag = StringParsers.ParseBool("false");
		string filePath = DetectWebserverFolder();
		RegisterPathHandler("/", new RewriteHandler("/files/"));
		RegisterPathHandler("/app", new RewriteHandler("/files/index.html", _fixedTarget: true));
		RegisterWebMods(flag);
		RegisterPathHandler("/session/", new SessionHandler());
		RegisterPathHandler("/userstatus", new UserStatusHandler());
		RegisterPathHandler("/sse/", new SseHandler());
		RegisterPathHandler("/files/", new StaticHandler(filePath, flag ? ((AbstractCache)new SimpleCache()) : ((AbstractCache)new DirectAccess()), _logMissingFiles: false));
		RegisterPathHandler("/itemicons/", new ItemIconHandler(_logMissingFiles: true));
		RegisterPathHandler("/api/", new ApiHandler());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DetectWebserverFolder()
	{
		string text = GameIO.GetGameDir("Data/Web") + "/webroot";
		foreach (Mod loadedMod in ModManager.GetLoadedMods())
		{
			string text2 = loadedMod.Path + "/webroot";
			if (Directory.Exists(text2))
			{
				text = text2;
			}
		}
		Log.Out("[Web] Serving basic webserver files from " + text);
		return text;
	}

	public void RegisterPathHandler(string _urlBasePath, AbsHandler _handler)
	{
		foreach (AbsHandler handler in handlers)
		{
			if (!(handler.UrlBasePath != _urlBasePath))
			{
				Log.Error("[Web] Handler for relative path " + _urlBasePath + " already registerd.");
				return;
			}
		}
		handlers.Add(_handler);
		_handler.SetBasePathAndParent(this, _urlBasePath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegisterWebMods(bool _useStaticCache)
	{
		foreach (Mod loadedMod in ModManager.GetLoadedMods())
		{
			try
			{
				try
				{
					WebMod item = new WebMod(this, loadedMod, _useStaticCache);
					WebMods.Add(item);
				}
				catch (InvalidDataException ex)
				{
					Log.Error("[Web] Could not load webmod from mod " + loadedMod.Name + ": " + ex.Message);
				}
			}
			catch (Exception e)
			{
				Log.Error("[Web] Failed loading web mods from mod " + loadedMod.Name);
				Log.Exception(e);
			}
		}
	}

	public void Disconnect()
	{
		if (shutdown)
		{
			return;
		}
		shutdown = true;
		try
		{
			foreach (AbsHandler handler in handlers)
			{
				handler.Shutdown();
			}
			listener.Stop();
			listener.Close();
		}
		catch (Exception arg)
		{
			Log.Out($"[Web] Error in Web.Disconnect: {arg}");
		}
	}

	public void SendLine(string _line)
	{
		ConnectionHandler.SendLine(_line);
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRequest(IAsyncResult _result)
	{
		SpaceWizards.HttpListener.HttpListener httpListener = (SpaceWizards.HttpListener.HttpListener)_result.AsyncState;
		if (!httpListener.IsListening)
		{
			return;
		}
		SpaceWizards.HttpListener.HttpListenerContext httpListenerContext = httpListener.EndGetContext(_result);
		httpListener.BeginGetContext(handleRequestDelegate, httpListener);
		try
		{
			SpaceWizards.HttpListener.HttpListenerRequest request = httpListenerContext.Request;
			SpaceWizards.HttpListener.HttpListenerResponse response = httpListenerContext.Response;
			response.SendChunked = false;
			response.ProtocolVersion = httpProtocolVersion;
			if (GameManager.Instance.World == null)
			{
				response.StatusCode = 503;
				return;
			}
			if (request.Url == null)
			{
				response.StatusCode = 400;
				return;
			}
			WebConnection _con;
			int permissionLevel = DoAuthentication(request, out _con);
			if (_con != null)
			{
				Cookie cookie = new Cookie("sid", _con.SessionID, "/")
				{
					Expired = false,
					Expires = DateTime.MinValue,
					HttpOnly = true,
					Secure = false
				};
				response.AppendCookie(cookie);
			}
			string absolutePath = request.Url.AbsolutePath;
			if (absolutePath.Length < 2)
			{
				response.Redirect("/app");
				return;
			}
			request.ContentEncoding = Encoding.UTF8;
			RequestContext requestContext = new RequestContext(absolutePath, request, response, _con, permissionLevel);
			if (requestContext.Method == ERequestMethod.Other)
			{
				requestContext.Response.StatusCode = 400;
			}
			else
			{
				ApplyPathHandler(requestContext);
			}
		}
		catch (IOException ex)
		{
			if (ex.InnerException is SocketException)
			{
				Log.Out("[Web] Error in Web.HandleRequest(): Remote host closed connection: " + ex.InnerException.Message);
			}
			else
			{
				Log.Out($"[Web] Error (IO) in Web.HandleRequest(): {ex}");
			}
		}
		catch (Exception e)
		{
			Log.Error("[Web] Error in Web.HandleRequest(): ");
			Log.Exception(e);
		}
		finally
		{
			if (!httpListenerContext.Response.SendChunked)
			{
				httpListenerContext.Response.Close();
			}
		}
	}

	public void ApplyPathHandler(RequestContext _context)
	{
		for (int num = handlers.Count - 1; num >= 0; num--)
		{
			AbsHandler absHandler = handlers[num];
			if (_context.RequestPath.StartsWith(absHandler.UrlBasePath))
			{
				if (!absHandler.IsAuthorizedForHandler(_context))
				{
					_context.Response.StatusCode = 403;
					_ = _context.Connection;
				}
				else
				{
					absHandler.HandleRequest(_context);
				}
				return;
			}
		}
		_context.Response.StatusCode = 404;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int DoAuthentication(SpaceWizards.HttpListener.HttpListenerRequest _req, out WebConnection _con)
	{
		_con = null;
		string text = _req.Cookies["sid"]?.Value;
		IPEndPoint remoteEndPoint = _req.RemoteEndPoint;
		if (remoteEndPoint == null)
		{
			Log.Warning("[Web] No RemoteEndPoint on web request");
			return 2000;
		}
		if (!string.IsNullOrEmpty(text))
		{
			_con = ConnectionHandler.IsLoggedIn(text, remoteEndPoint.Address);
			if (_con != null)
			{
				int userPermissionLevel = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_con.UserId);
				int val = int.MaxValue;
				if (_con.CrossplatformUserId != null)
				{
					val = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_con.CrossplatformUserId);
				}
				return Math.Min(userPermissionLevel, val);
			}
		}
		if (!_req.Headers.TryGetValue("X-SDTD-API-TOKENNAME", out var _result) || !_req.Headers.TryGetValue("X-SDTD-API-SECRET", out var _result2))
		{
			return 2000;
		}
		int permissionLevel = AdminApiTokens.Instance.GetPermissionLevel(_result, _result2);
		if (permissionLevel < int.MaxValue)
		{
			return permissionLevel;
		}
		Log.Warning($"[Web] Invalid Admintoken used from {remoteEndPoint}");
		return 2000;
	}
}
