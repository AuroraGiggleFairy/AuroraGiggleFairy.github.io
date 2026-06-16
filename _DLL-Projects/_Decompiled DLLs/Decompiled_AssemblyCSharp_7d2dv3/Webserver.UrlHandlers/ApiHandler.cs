using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Profiling;
using Webserver.WebAPI;

namespace Webserver.UrlHandlers;

public class ApiHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AbsWebAPI> apis = new CaseInsensitiveStringDictionary<AbsWebAPI>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Type[] apiWithParentCtorTypes = new Type[1] { typeof(Web) };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object[] apiWithParentCtorArgs = new object[1];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Type[] apiEmptyCtorTypes = new Type[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object[] apiEmptyCtorArgs = new object[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly CustomSampler apiHandlerSampler = CustomSampler.Create("API_Handler");

	public ApiHandler()
		: base(null)
	{
	}

	public override void SetBasePathAndParent(Web _parent, string _relativePath)
	{
		base.SetBasePathAndParent(_parent, _relativePath);
		apiWithParentCtorArgs[0] = _parent;
		ReflectionHelpers.FindTypesImplementingBase(typeof(AbsWebAPI), apiFoundCallback);
		addApi(new Null("viewallclaims"));
		addApi(new Null("viewallplayers"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiFoundCallback(Type _type)
	{
		ConstructorInfo constructor = _type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, apiWithParentCtorTypes, null);
		if (constructor != null)
		{
			AbsWebAPI api = (AbsWebAPI)constructor.Invoke(apiWithParentCtorArgs);
			addApi(api);
			return;
		}
		constructor = _type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, apiEmptyCtorTypes, null);
		if (constructor != null)
		{
			AbsWebAPI api2 = (AbsWebAPI)constructor.Invoke(apiEmptyCtorArgs);
			addApi(api2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addApi(AbsWebAPI _api)
	{
		apis.Add(_api.Name, _api);
		parent.OpenApiHelpers.LoadOpenApiSpec(_api);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleCors(RequestContext _context)
	{
		_context.Request.Headers.TryGetValue("Origin", out var _result);
		_context.Response.AddHeader("Access-Control-Allow-Origin", _result ?? "*");
		if (_context.Method != ERequestMethod.OPTIONS)
		{
			return false;
		}
		if (!_context.Request.Headers.TryGetValue("Access-Control-Request-Method", out var _))
		{
			return false;
		}
		_context.Response.AddHeader("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS, HEAD");
		_context.Response.AddHeader("Access-Control-Allow-Headers", "X-SDTD-API-TOKENNAME, X-SDTD-API-SECRET");
		_context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
		return true;
	}

	public override void HandleRequest(RequestContext _context)
	{
		string requestPath = null;
		int num = _context.RequestPath.IndexOf('/', urlBasePath.Length);
		string text;
		if (num >= 0)
		{
			text = _context.RequestPath.Substring(urlBasePath.Length, num - urlBasePath.Length);
			requestPath = _context.RequestPath.Substring(num + 1);
		}
		else
		{
			text = _context.RequestPath.Substring(urlBasePath.Length);
		}
		if (!apis.TryGetValue(text, out var value))
		{
			Log.Warning("[Web] In ApiHandler.HandleRequest(): No handler found for API \"" + text + "\"");
			_context.Response.StatusCode = 404;
		}
		else
		{
			if (HandleCors(_context))
			{
				return;
			}
			_context.RequestPath = requestPath;
			if (value.Authorized(_context))
			{
				try
				{
					value.HandleRequest(_context);
					return;
				}
				catch (Exception e)
				{
					Log.Error("[Web] In ApiHandler.HandleRequest(): Handler " + value.Name + " threw an exception:");
					Log.Exception(e);
					_context.Response.StatusCode = 500;
					return;
				}
			}
			_context.Response.StatusCode = 403;
			_ = _context.Connection;
		}
	}
}
