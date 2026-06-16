using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Webserver.Permissions;
using Webserver.SSE;

namespace Webserver.UrlHandlers;

public class SseHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AbsEvent> events = new CaseInsensitiveStringDictionary<AbsEvent>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo queueThead;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AutoResetEvent evSendRequest = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shutdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Type[] ctorTypes = new Type[1] { typeof(SseHandler) };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object[] ctorParams = new object[1];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SseClient> clients = new List<SseClient>();

	public SseHandler(string _moduleName = null)
		: base(_moduleName)
	{
		ctorParams[0] = this;
		ReflectionHelpers.FindTypesImplementingBase(typeof(AbsEvent), apiFoundCallback);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiFoundCallback(Type _type)
	{
		ConstructorInfo constructor = _type.GetConstructor(ctorTypes);
		if (!(constructor == null))
		{
			AbsEvent absEvent = (AbsEvent)constructor.Invoke(ctorParams);
			AddEvent(absEvent.Name, absEvent);
		}
	}

	public override void SetBasePathAndParent(Web _parent, string _relativePath)
	{
		base.SetBasePathAndParent(_parent, _relativePath);
		queueThead = ThreadManager.StartThread("SSE-Processing_" + urlBasePath, QueueProcessThread, ThreadPriority.BelowNormal, null, _useRealThread: true);
	}

	public override void Shutdown()
	{
		base.Shutdown();
		shutdown = true;
		SignalSendQueue();
	}

	public void AddEvent(string _eventName, AbsEvent _eventInstance)
	{
		events.Add(_eventName, _eventInstance);
		AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule("webevent." + _eventName, _eventInstance.DefaultPermissionLevel(), _isDefault: true));
	}

	public override void HandleRequest(RequestContext _context)
	{
		string text = _context.QueryParameters["events"];
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("[Web] [SSE] In SseHandler.HandleRequest(): No 'events' query parameter given");
			_context.Response.StatusCode = 400;
			return;
		}
		SseClient sseClient;
		try
		{
			sseClient = new SseClient(this, _context);
		}
		catch (Exception e)
		{
			Log.Error("[Web] [SSE] In SseHandler.HandleRequest(): Could not create client:");
			Log.Exception(e);
			_context.Response.StatusCode = 500;
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string[] array = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text2 in array)
		{
			if (!events.TryGetValue(text2, out var value))
			{
				Log.Warning("[Web] [SSE] In SseHandler.HandleRequest(): No handler found for event \"" + text2 + "\"");
				continue;
			}
			num++;
			if (IsAuthorizedForEvent(text2, _context.PermissionLevel))
			{
				num2++;
				try
				{
					value.AddListener(sseClient);
				}
				catch (Exception e2)
				{
					Log.Error("[Web] [SSE] In SseHandler.HandleRequest(): Handler " + value.Name + " threw an exception:");
					Log.Exception(e2);
					_context.Response.StatusCode = 500;
				}
				num3++;
			}
		}
		if (num == 0)
		{
			_context.Response.StatusCode = 400;
			_context.Response.Close();
		}
		else if (num2 == 0)
		{
			_context.Response.StatusCode = 403;
			_ = _context.Connection;
			_context.Response.Close();
		}
		else
		{
			clients.Add(sseClient);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAuthorizedForEvent(string _eventName, int _permissionLevel)
	{
		return AdminWebModules.Instance.ModuleAllowedWithLevel("webevent." + _eventName, _permissionLevel);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueueProcessThread(ThreadManager.ThreadInfo _threadInfo)
	{
		while (!shutdown && !_threadInfo.TerminationRequested())
		{
			evSendRequest.WaitOne(500);
			foreach (var (text2, absEvent2) in events)
			{
				try
				{
					absEvent2.ProcessSendQueue();
				}
				catch (Exception e)
				{
					Log.Error("[Web] [SSE] '" + text2 + "': Error processing send queue");
					Log.Exception(e);
				}
			}
			for (int num = clients.Count - 1; num >= 0; num--)
			{
				clients[num].HandleKeepAlive();
			}
		}
	}

	public void SignalSendQueue()
	{
		evSendRequest.Set();
	}

	public void ClientClosed(SseClient _client)
	{
		foreach (var (text2, absEvent2) in events)
		{
			try
			{
				absEvent2.ClientClosed(_client);
			}
			catch (Exception e)
			{
				Log.Error("[Web] [SSE] '" + text2 + "': Error closing client");
				Log.Exception(e);
			}
		}
		clients.Remove(_client);
	}
}
