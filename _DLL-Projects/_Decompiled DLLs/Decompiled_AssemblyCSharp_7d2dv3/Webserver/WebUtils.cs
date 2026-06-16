using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using SpaceWizards.HttpListener;
using UnityEngine.Profiling;
using Utf8Json;
using Webserver.WebAPI;

namespace Webserver;

public static class WebUtils
{
	public const string MimePlain = "text/plain";

	public const string MimeHtml = "text/html";

	public const string MimeJson = "application/json";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly CustomSampler envelopeBuildSampler;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly CustomSampler netWriteSampler;

	public static readonly byte[] JsonEmptyData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawDataKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaServertimeKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaRequestMethodKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaRequestSubpathKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaRequestBodyKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaErrorCodeKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaExceptionMessageKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawMetaExceptionTraceKey;

	[PublicizedFrom(EAccessModifier.Private)]
	static WebUtils()
	{
		envelopeBuildSampler = CustomSampler.Create("JSON_EnvelopeBuilding");
		netWriteSampler = CustomSampler.Create("JSON_Write");
		jsonRawDataKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("data");
		jsonRawMetaKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("meta");
		jsonRawMetaServertimeKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("serverTime");
		jsonRawMetaRequestMethodKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requestMethod");
		jsonRawMetaRequestSubpathKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requestSubpath");
		jsonRawMetaRequestBodyKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requestBody");
		jsonRawMetaErrorCodeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("errorCode");
		jsonRawMetaExceptionMessageKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("exceptionMessage");
		jsonRawMetaExceptionTraceKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("exceptionTrace");
		JsonWriter jsonWriter = default(JsonWriter);
		jsonWriter.WriteBeginArray();
		jsonWriter.WriteEndArray();
		JsonEmptyData = jsonWriter.ToUtf8ByteArray();
	}

	public static void WriteText(SpaceWizards.HttpListener.HttpListenerResponse _resp, string _text, HttpStatusCode _statusCode = HttpStatusCode.OK, string _mimeType = null)
	{
		_resp.StatusCode = (int)_statusCode;
		_resp.ContentType = _mimeType ?? "text/plain";
		_resp.ContentEncoding = Encoding.UTF8;
		if (string.IsNullOrEmpty(_text))
		{
			_resp.ContentLength64 = 0L;
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(_text);
		_resp.ContentLength64 = bytes.Length;
		_resp.OutputStream.Write(bytes, 0, bytes.Length);
	}

	public static bool IsSslRedirected(SpaceWizards.HttpListener.HttpListenerRequest _req)
	{
		string text = _req.Headers["X-Forwarded-Proto"];
		if (!string.IsNullOrEmpty(text))
		{
			return text.Equals("https", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static string GenerateGuid()
	{
		return Guid.NewGuid().ToString();
	}

	public static void WriteJsonData(SpaceWizards.HttpListener.HttpListenerResponse _resp, ref JsonWriter _jsonWriter, HttpStatusCode _statusCode = HttpStatusCode.OK)
	{
		ArraySegment<byte> buffer = _jsonWriter.GetBuffer();
		_resp.StatusCode = (int)_statusCode;
		_resp.ContentType = "application/json";
		_resp.ContentEncoding = Encoding.UTF8;
		_resp.ContentLength64 = buffer.Count;
		_resp.OutputStream.Write(buffer.Array, 0, buffer.Count);
	}

	public static void PrepareEnvelopedResult(out JsonWriter _writer)
	{
		_writer = default(JsonWriter);
		_writer.WriteRaw(jsonRawDataKey);
	}

	public static void SendEnvelopedResult(RequestContext _context, ref JsonWriter _writer, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
	{
		_writer.WriteRaw(jsonRawMetaKey);
		_writer.WriteRaw(jsonRawMetaServertimeKey);
		JsonCommons.WriteDateTime(ref _writer, DateTime.Now);
		if (!string.IsNullOrEmpty(_errorCode))
		{
			_writer.WriteRaw(jsonRawMetaRequestMethodKey);
			_writer.WriteString(_context.Request.HttpMethod);
			_writer.WriteRaw(jsonRawMetaRequestSubpathKey);
			_writer.WriteString(_context.RequestPath);
			_writer.WriteRaw(jsonRawMetaRequestBodyKey);
			if (_jsonInputData != null)
			{
				_writer.WriteRaw(_jsonInputData);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteRaw(jsonRawMetaErrorCodeKey);
			_writer.WriteString(_errorCode);
			if (_exception != null)
			{
				_writer.WriteRaw(jsonRawMetaExceptionMessageKey);
				_writer.WriteString(_exception.Message);
				_writer.WriteRaw(jsonRawMetaExceptionTraceKey);
				_writer.WriteString(_exception.StackTrace);
			}
		}
		_writer.WriteEndObject();
		_writer.WriteEndObject();
		WriteJsonData(_context.Response, ref _writer, _statusCode);
	}

	public static void SendEmptyResponse(RequestContext _context, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
	{
		PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(JsonEmptyData);
		SendEnvelopedResult(_context, ref _writer, _statusCode, _jsonInputData, _errorCode, _exception);
	}

	public static bool TryGetValue(this NameValueCollection _nameValueCollection, string _name, out string _result)
	{
		_result = _nameValueCollection[_name];
		return _result != null;
	}
}
