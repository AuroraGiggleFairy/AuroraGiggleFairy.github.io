using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine.Profiling;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI;

public abstract class AbsRestApi : AbsWebAPI
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly CustomSampler jsonDeserializeSampler = CustomSampler.Create("JSON_Deserialize");

	public virtual bool AllowPostWithId
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsRestApi(string _name = null)
		: this(null, _name)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsRestApi(Web _parentWeb, string _name = null)
		: base(_parentWeb, _name)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RegisterPermissions()
	{
		AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule(CachedApiModuleName, DefaultPermissionLevel(), DefaultMethodPermissionLevels(), _isDefault: true));
	}

	public sealed override void HandleRequest(RequestContext _context)
	{
		IDictionary<string, object> dictionary = null;
		byte[] array = null;
		if (_context.Request.HasEntityBody)
		{
			Stream inputStream = _context.Request.InputStream;
			array = new byte[_context.Request.ContentLength64];
			inputStream.Read(array, 0, (int)_context.Request.ContentLength64);
			try
			{
				dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(array);
			}
			catch (Exception exception)
			{
				SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "INVALID_BODY", exception);
				return;
			}
		}
		try
		{
			switch (_context.Method)
			{
			case ERequestMethod.GET:
				if (dictionary != null)
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "GET_WITH_BODY");
				}
				else
				{
					HandleRestGet(_context);
				}
				break;
			case ERequestMethod.POST:
				if (!AllowPostWithId && !string.IsNullOrEmpty(_context.RequestPath))
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "POST_WITH_ID");
				}
				else if (dictionary == null)
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "POST_WITHOUT_BODY");
				}
				else
				{
					HandleRestPost(_context, dictionary, array);
				}
				break;
			case ERequestMethod.PUT:
				if (string.IsNullOrEmpty(_context.RequestPath))
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "PUT_WITHOUT_ID");
				}
				else if (dictionary == null)
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "PUT_WITHOUT_BODY");
				}
				else
				{
					HandleRestPut(_context, dictionary, array);
				}
				break;
			case ERequestMethod.DELETE:
				if (string.IsNullOrEmpty(_context.RequestPath))
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "DELETE_WITHOUT_ID");
				}
				else if (dictionary != null)
				{
					SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "DELETE_WITH_BODY");
				}
				else
				{
					HandleRestDelete(_context);
				}
				break;
			default:
				SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "INVALID_METHOD");
				break;
			}
		}
		catch (Exception exception2)
		{
			try
			{
				SendEmptyResponse(_context, HttpStatusCode.InternalServerError, array, "ERROR_PROCESSING", exception2);
			}
			catch (Exception e)
			{
				Log.Error("[Web] In AbsRestApi.HandleRequest(): Handler " + Name + " threw an exception while trying to send a previous exception to the client:");
				Log.Exception(e);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleRestGet(RequestContext _context)
	{
		SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, null, "Unsupported");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, _jsonInputData, "Unsupported");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleRestPut(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, _jsonInputData, "Unsupported");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleRestDelete(RequestContext _context)
	{
		SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, null, "Unsupported");
	}

	public override bool Authorized(RequestContext _context)
	{
		AdminWebModules.WebModule module = AdminWebModules.Instance.GetModule(CachedApiModuleName);
		if (module.LevelPerMethod == null)
		{
			return module.LevelGlobal >= _context.PermissionLevel;
		}
		int num = module.LevelPerMethod[(int)_context.Method];
		return num switch
		{
			-2147483647 => false, 
			int.MinValue => module.LevelGlobal >= _context.PermissionLevel, 
			_ => num >= _context.PermissionLevel, 
		};
	}

	public virtual int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483648, -2147483648 };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void PrepareEnvelopedResult(out JsonWriter _writer)
	{
		WebUtils.PrepareEnvelopedResult(out _writer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void SendEnvelopedResult(RequestContext _context, ref JsonWriter _writer, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
	{
		WebUtils.SendEnvelopedResult(_context, ref _writer, _statusCode, _jsonInputData, _errorCode, _exception);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void SendEmptyResponse(RequestContext _context, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
	{
		PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(WebUtils.JsonEmptyData);
		SendEnvelopedResult(_context, ref _writer, _statusCode, _jsonInputData, _errorCode, _exception);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void SendEmptyResponse(RequestContext _context, HttpStatusCode _statusCode, byte[] _jsonInputData, EApiErrorCode _errorCode, Exception _exception = null)
	{
		SendEmptyResponse(_context, _statusCode, _jsonInputData, _errorCode.ToStringCached(), _exception);
	}
}
