using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal abstract class JsonSerializerInternalBase
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(0)]
	private class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			return x == y;
		}

		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		int IEqualityComparer<object>.GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private ErrorContext _currentErrorContext;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
	private BidirectionalDictionary<string, object> _mappings;

	internal readonly JsonSerializer Serializer;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal readonly ITraceWriter TraceWriter;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	protected JsonSerializerProxy InternalSerializer;

	internal BidirectionalDictionary<string, object> DefaultReferenceMappings
	{
		get
		{
			if (_mappings == null)
			{
				_mappings = new BidirectionalDictionary<string, object>(EqualityComparer<string>.Default, new ReferenceEqualsEqualityComparer(), "A different value already has the Id '{0}'.", "A different Id has already been assigned for value '{0}'. This error may be caused by an object being reused multiple times during deserialization and can be fixed with the setting ObjectCreationHandling.Replace.");
			}
			return _mappings;
		}
	}

	protected JsonSerializerInternalBase(JsonSerializer serializer)
	{
		ValidationUtils.ArgumentNotNull(serializer, "serializer");
		Serializer = serializer;
		TraceWriter = serializer.TraceWriter;
	}

	protected NullValueHandling ResolvedNullValueHandling([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonObjectContract containerContract, JsonProperty property)
	{
		return property.NullValueHandling ?? containerContract?.ItemNullValueHandling ?? Serializer._nullValueHandling;
	}

	private ErrorContext GetErrorContext([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object currentObject, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object member, string path, Exception error)
	{
		if (_currentErrorContext == null)
		{
			_currentErrorContext = new ErrorContext(currentObject, member, path, error);
		}
		if (_currentErrorContext.Error != error)
		{
			throw new InvalidOperationException("Current error context error is different to requested error.");
		}
		return _currentErrorContext;
	}

	protected void ClearErrorContext()
	{
		if (_currentErrorContext == null)
		{
			throw new InvalidOperationException("Could not clear error context. Error context is already null.");
		}
		_currentErrorContext = null;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	protected bool IsErrorHandled(object currentObject, JsonContract contract, object keyValue, IJsonLineInfo lineInfo, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string path, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] Exception ex)
	{
		ErrorContext errorContext = GetErrorContext(currentObject, keyValue, path, ex);
		if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Error && !errorContext.Traced)
		{
			errorContext.Traced = true;
			string text = ((GetType() == typeof(JsonSerializerInternalWriter)) ? "Error serializing" : "Error deserializing");
			if (contract != null)
			{
				text = text + " " + contract.UnderlyingType;
			}
			text = text + ". " + ex.Message;
			if (!(ex is JsonException))
			{
				text = JsonPosition.FormatMessage(lineInfo, path, text);
			}
			TraceWriter.Trace(TraceLevel.Error, text, ex);
		}
		if (contract != null && currentObject != null)
		{
			contract.InvokeOnError(currentObject, Serializer.Context, errorContext);
		}
		if (!errorContext.Handled)
		{
			Serializer.OnError(new ErrorEventArgs(currentObject, errorContext));
		}
		return errorContext.Handled;
	}
}
