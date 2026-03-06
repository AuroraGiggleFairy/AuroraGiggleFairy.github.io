using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JsonSerializerProxy : JsonSerializer
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private readonly JsonSerializerInternalReader _serializerReader;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private readonly JsonSerializerInternalWriter _serializerWriter;

	internal readonly JsonSerializer _serializer;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override IReferenceResolver ReferenceResolver
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _serializer.ReferenceResolver;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_serializer.ReferenceResolver = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override ITraceWriter TraceWriter
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _serializer.TraceWriter;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_serializer.TraceWriter = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override IEqualityComparer EqualityComparer
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _serializer.EqualityComparer;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_serializer.EqualityComparer = value;
		}
	}

	public override JsonConverterCollection Converters => _serializer.Converters;

	public override DefaultValueHandling DefaultValueHandling
	{
		get
		{
			return _serializer.DefaultValueHandling;
		}
		set
		{
			_serializer.DefaultValueHandling = value;
		}
	}

	public override IContractResolver ContractResolver
	{
		get
		{
			return _serializer.ContractResolver;
		}
		set
		{
			_serializer.ContractResolver = value;
		}
	}

	public override MissingMemberHandling MissingMemberHandling
	{
		get
		{
			return _serializer.MissingMemberHandling;
		}
		set
		{
			_serializer.MissingMemberHandling = value;
		}
	}

	public override NullValueHandling NullValueHandling
	{
		get
		{
			return _serializer.NullValueHandling;
		}
		set
		{
			_serializer.NullValueHandling = value;
		}
	}

	public override ObjectCreationHandling ObjectCreationHandling
	{
		get
		{
			return _serializer.ObjectCreationHandling;
		}
		set
		{
			_serializer.ObjectCreationHandling = value;
		}
	}

	public override ReferenceLoopHandling ReferenceLoopHandling
	{
		get
		{
			return _serializer.ReferenceLoopHandling;
		}
		set
		{
			_serializer.ReferenceLoopHandling = value;
		}
	}

	public override PreserveReferencesHandling PreserveReferencesHandling
	{
		get
		{
			return _serializer.PreserveReferencesHandling;
		}
		set
		{
			_serializer.PreserveReferencesHandling = value;
		}
	}

	public override TypeNameHandling TypeNameHandling
	{
		get
		{
			return _serializer.TypeNameHandling;
		}
		set
		{
			_serializer.TypeNameHandling = value;
		}
	}

	public override MetadataPropertyHandling MetadataPropertyHandling
	{
		get
		{
			return _serializer.MetadataPropertyHandling;
		}
		set
		{
			_serializer.MetadataPropertyHandling = value;
		}
	}

	[Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
	public override FormatterAssemblyStyle TypeNameAssemblyFormat
	{
		get
		{
			return _serializer.TypeNameAssemblyFormat;
		}
		set
		{
			_serializer.TypeNameAssemblyFormat = value;
		}
	}

	public override TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
	{
		get
		{
			return _serializer.TypeNameAssemblyFormatHandling;
		}
		set
		{
			_serializer.TypeNameAssemblyFormatHandling = value;
		}
	}

	public override ConstructorHandling ConstructorHandling
	{
		get
		{
			return _serializer.ConstructorHandling;
		}
		set
		{
			_serializer.ConstructorHandling = value;
		}
	}

	[Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
	public override SerializationBinder Binder
	{
		get
		{
			return _serializer.Binder;
		}
		set
		{
			_serializer.Binder = value;
		}
	}

	public override ISerializationBinder SerializationBinder
	{
		get
		{
			return _serializer.SerializationBinder;
		}
		set
		{
			_serializer.SerializationBinder = value;
		}
	}

	public override StreamingContext Context
	{
		get
		{
			return _serializer.Context;
		}
		set
		{
			_serializer.Context = value;
		}
	}

	public override Formatting Formatting
	{
		get
		{
			return _serializer.Formatting;
		}
		set
		{
			_serializer.Formatting = value;
		}
	}

	public override DateFormatHandling DateFormatHandling
	{
		get
		{
			return _serializer.DateFormatHandling;
		}
		set
		{
			_serializer.DateFormatHandling = value;
		}
	}

	public override DateTimeZoneHandling DateTimeZoneHandling
	{
		get
		{
			return _serializer.DateTimeZoneHandling;
		}
		set
		{
			_serializer.DateTimeZoneHandling = value;
		}
	}

	public override DateParseHandling DateParseHandling
	{
		get
		{
			return _serializer.DateParseHandling;
		}
		set
		{
			_serializer.DateParseHandling = value;
		}
	}

	public override FloatFormatHandling FloatFormatHandling
	{
		get
		{
			return _serializer.FloatFormatHandling;
		}
		set
		{
			_serializer.FloatFormatHandling = value;
		}
	}

	public override FloatParseHandling FloatParseHandling
	{
		get
		{
			return _serializer.FloatParseHandling;
		}
		set
		{
			_serializer.FloatParseHandling = value;
		}
	}

	public override StringEscapeHandling StringEscapeHandling
	{
		get
		{
			return _serializer.StringEscapeHandling;
		}
		set
		{
			_serializer.StringEscapeHandling = value;
		}
	}

	public override string DateFormatString
	{
		get
		{
			return _serializer.DateFormatString;
		}
		set
		{
			_serializer.DateFormatString = value;
		}
	}

	public override CultureInfo Culture
	{
		get
		{
			return _serializer.Culture;
		}
		set
		{
			_serializer.Culture = value;
		}
	}

	public override int? MaxDepth
	{
		get
		{
			return _serializer.MaxDepth;
		}
		set
		{
			_serializer.MaxDepth = value;
		}
	}

	public override bool CheckAdditionalContent
	{
		get
		{
			return _serializer.CheckAdditionalContent;
		}
		set
		{
			_serializer.CheckAdditionalContent = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public override event EventHandler<ErrorEventArgs> Error
	{
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		add
		{
			_serializer.Error += value;
		}
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		remove
		{
			_serializer.Error -= value;
		}
	}

	internal JsonSerializerInternalBase GetInternalSerializer()
	{
		if (_serializerReader != null)
		{
			return _serializerReader;
		}
		return _serializerWriter;
	}

	public JsonSerializerProxy(JsonSerializerInternalReader serializerReader)
	{
		ValidationUtils.ArgumentNotNull(serializerReader, "serializerReader");
		_serializerReader = serializerReader;
		_serializer = serializerReader.Serializer;
	}

	public JsonSerializerProxy(JsonSerializerInternalWriter serializerWriter)
	{
		ValidationUtils.ArgumentNotNull(serializerWriter, "serializerWriter");
		_serializerWriter = serializerWriter;
		_serializer = serializerWriter.Serializer;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal override object DeserializeInternal([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonReader reader, Type objectType)
	{
		if (_serializerReader != null)
		{
			return _serializerReader.Deserialize(reader, objectType, checkAdditionalContent: false);
		}
		return _serializer.Deserialize(reader, objectType);
	}

	internal override void PopulateInternal(JsonReader reader, object target)
	{
		if (_serializerReader != null)
		{
			_serializerReader.Populate(reader, target);
		}
		else
		{
			_serializer.Populate(reader, target);
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal override void SerializeInternal([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonWriter jsonWriter, object value, Type rootType)
	{
		if (_serializerWriter != null)
		{
			_serializerWriter.Serialize(jsonWriter, value, rootType);
		}
		else
		{
			_serializer.Serialize(jsonWriter, value);
		}
	}
}
