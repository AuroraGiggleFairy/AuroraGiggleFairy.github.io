using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JsonSerializer
{
	internal TypeNameHandling _typeNameHandling;

	internal TypeNameAssemblyFormatHandling _typeNameAssemblyFormatHandling;

	internal PreserveReferencesHandling _preserveReferencesHandling;

	internal ReferenceLoopHandling _referenceLoopHandling;

	internal MissingMemberHandling _missingMemberHandling;

	internal ObjectCreationHandling _objectCreationHandling;

	internal NullValueHandling _nullValueHandling;

	internal DefaultValueHandling _defaultValueHandling;

	internal ConstructorHandling _constructorHandling;

	internal MetadataPropertyHandling _metadataPropertyHandling;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal JsonConverterCollection _converters;

	internal IContractResolver _contractResolver;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal ITraceWriter _traceWriter;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal IEqualityComparer _equalityComparer;

	internal ISerializationBinder _serializationBinder;

	internal StreamingContext _context;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private IReferenceResolver _referenceResolver;

	private Formatting? _formatting;

	private DateFormatHandling? _dateFormatHandling;

	private DateTimeZoneHandling? _dateTimeZoneHandling;

	private DateParseHandling? _dateParseHandling;

	private FloatFormatHandling? _floatFormatHandling;

	private FloatParseHandling? _floatParseHandling;

	private StringEscapeHandling? _stringEscapeHandling;

	private CultureInfo _culture;

	private int? _maxDepth;

	private bool _maxDepthSet;

	private bool? _checkAdditionalContent;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private string _dateFormatString;

	private bool _dateFormatStringSet;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public virtual IReferenceResolver ReferenceResolver
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return GetReferenceResolver();
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Reference resolver cannot be null.");
			}
			_referenceResolver = value;
		}
	}

	[Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
	public virtual SerializationBinder Binder
	{
		get
		{
			if (_serializationBinder is SerializationBinder result)
			{
				return result;
			}
			if (_serializationBinder is SerializationBinderAdapter serializationBinderAdapter)
			{
				return serializationBinderAdapter.SerializationBinder;
			}
			throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Serialization binder cannot be null.");
			}
			_serializationBinder = (value as ISerializationBinder) ?? new SerializationBinderAdapter(value);
		}
	}

	public virtual ISerializationBinder SerializationBinder
	{
		get
		{
			return _serializationBinder;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Serialization binder cannot be null.");
			}
			_serializationBinder = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public virtual ITraceWriter TraceWriter
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _traceWriter;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_traceWriter = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public virtual IEqualityComparer EqualityComparer
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _equalityComparer;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_equalityComparer = value;
		}
	}

	public virtual TypeNameHandling TypeNameHandling
	{
		get
		{
			return _typeNameHandling;
		}
		set
		{
			if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_typeNameHandling = value;
		}
	}

	[Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
	public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
	{
		get
		{
			return (FormatterAssemblyStyle)_typeNameAssemblyFormatHandling;
		}
		set
		{
			if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_typeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
		}
	}

	public virtual TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
	{
		get
		{
			return _typeNameAssemblyFormatHandling;
		}
		set
		{
			if (value < TypeNameAssemblyFormatHandling.Simple || value > TypeNameAssemblyFormatHandling.Full)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_typeNameAssemblyFormatHandling = value;
		}
	}

	public virtual PreserveReferencesHandling PreserveReferencesHandling
	{
		get
		{
			return _preserveReferencesHandling;
		}
		set
		{
			if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_preserveReferencesHandling = value;
		}
	}

	public virtual ReferenceLoopHandling ReferenceLoopHandling
	{
		get
		{
			return _referenceLoopHandling;
		}
		set
		{
			if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_referenceLoopHandling = value;
		}
	}

	public virtual MissingMemberHandling MissingMemberHandling
	{
		get
		{
			return _missingMemberHandling;
		}
		set
		{
			if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_missingMemberHandling = value;
		}
	}

	public virtual NullValueHandling NullValueHandling
	{
		get
		{
			return _nullValueHandling;
		}
		set
		{
			if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_nullValueHandling = value;
		}
	}

	public virtual DefaultValueHandling DefaultValueHandling
	{
		get
		{
			return _defaultValueHandling;
		}
		set
		{
			if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_defaultValueHandling = value;
		}
	}

	public virtual ObjectCreationHandling ObjectCreationHandling
	{
		get
		{
			return _objectCreationHandling;
		}
		set
		{
			if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_objectCreationHandling = value;
		}
	}

	public virtual ConstructorHandling ConstructorHandling
	{
		get
		{
			return _constructorHandling;
		}
		set
		{
			if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_constructorHandling = value;
		}
	}

	public virtual MetadataPropertyHandling MetadataPropertyHandling
	{
		get
		{
			return _metadataPropertyHandling;
		}
		set
		{
			if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_metadataPropertyHandling = value;
		}
	}

	public virtual JsonConverterCollection Converters
	{
		get
		{
			if (_converters == null)
			{
				_converters = new JsonConverterCollection();
			}
			return _converters;
		}
	}

	public virtual IContractResolver ContractResolver
	{
		get
		{
			return _contractResolver;
		}
		set
		{
			_contractResolver = value ?? DefaultContractResolver.Instance;
		}
	}

	public virtual StreamingContext Context
	{
		get
		{
			return _context;
		}
		set
		{
			_context = value;
		}
	}

	public virtual Formatting Formatting
	{
		get
		{
			return _formatting.GetValueOrDefault();
		}
		set
		{
			_formatting = value;
		}
	}

	public virtual DateFormatHandling DateFormatHandling
	{
		get
		{
			return _dateFormatHandling.GetValueOrDefault();
		}
		set
		{
			_dateFormatHandling = value;
		}
	}

	public virtual DateTimeZoneHandling DateTimeZoneHandling
	{
		get
		{
			return _dateTimeZoneHandling ?? DateTimeZoneHandling.RoundtripKind;
		}
		set
		{
			_dateTimeZoneHandling = value;
		}
	}

	public virtual DateParseHandling DateParseHandling
	{
		get
		{
			return _dateParseHandling ?? DateParseHandling.DateTime;
		}
		set
		{
			_dateParseHandling = value;
		}
	}

	public virtual FloatParseHandling FloatParseHandling
	{
		get
		{
			return _floatParseHandling.GetValueOrDefault();
		}
		set
		{
			_floatParseHandling = value;
		}
	}

	public virtual FloatFormatHandling FloatFormatHandling
	{
		get
		{
			return _floatFormatHandling.GetValueOrDefault();
		}
		set
		{
			_floatFormatHandling = value;
		}
	}

	public virtual StringEscapeHandling StringEscapeHandling
	{
		get
		{
			return _stringEscapeHandling.GetValueOrDefault();
		}
		set
		{
			_stringEscapeHandling = value;
		}
	}

	public virtual string DateFormatString
	{
		get
		{
			return _dateFormatString ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
		}
		set
		{
			_dateFormatString = value;
			_dateFormatStringSet = true;
		}
	}

	public virtual CultureInfo Culture
	{
		get
		{
			return _culture ?? JsonSerializerSettings.DefaultCulture;
		}
		set
		{
			_culture = value;
		}
	}

	public virtual int? MaxDepth
	{
		get
		{
			return _maxDepth;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Value must be positive.", "value");
			}
			_maxDepth = value;
			_maxDepthSet = true;
		}
	}

	public virtual bool CheckAdditionalContent
	{
		get
		{
			return _checkAdditionalContent == true;
		}
		set
		{
			_checkAdditionalContent = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[method: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public virtual event EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> Error;

	internal bool IsCheckAdditionalContentSet()
	{
		return _checkAdditionalContent.HasValue;
	}

	public JsonSerializer()
	{
		_referenceLoopHandling = ReferenceLoopHandling.Error;
		_missingMemberHandling = MissingMemberHandling.Ignore;
		_nullValueHandling = NullValueHandling.Include;
		_defaultValueHandling = DefaultValueHandling.Include;
		_objectCreationHandling = ObjectCreationHandling.Auto;
		_preserveReferencesHandling = PreserveReferencesHandling.None;
		_constructorHandling = ConstructorHandling.Default;
		_typeNameHandling = TypeNameHandling.None;
		_metadataPropertyHandling = MetadataPropertyHandling.Default;
		_context = JsonSerializerSettings.DefaultContext;
		_serializationBinder = DefaultSerializationBinder.Instance;
		_culture = JsonSerializerSettings.DefaultCulture;
		_contractResolver = DefaultContractResolver.Instance;
	}

	public static JsonSerializer Create()
	{
		return new JsonSerializer();
	}

	public static JsonSerializer Create([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSerializerSettings settings)
	{
		JsonSerializer jsonSerializer = Create();
		if (settings != null)
		{
			ApplySerializerSettings(jsonSerializer, settings);
		}
		return jsonSerializer;
	}

	public static JsonSerializer CreateDefault()
	{
		return Create(JsonConvert.DefaultSettings?.Invoke());
	}

	public static JsonSerializer CreateDefault([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSerializerSettings settings)
	{
		JsonSerializer jsonSerializer = CreateDefault();
		if (settings != null)
		{
			ApplySerializerSettings(jsonSerializer, settings);
		}
		return jsonSerializer;
	}

	private static void ApplySerializerSettings(JsonSerializer serializer, JsonSerializerSettings settings)
	{
		if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
		{
			for (int i = 0; i < settings.Converters.Count; i++)
			{
				serializer.Converters.Insert(i, settings.Converters[i]);
			}
		}
		if (settings._typeNameHandling.HasValue)
		{
			serializer.TypeNameHandling = settings.TypeNameHandling;
		}
		if (settings._metadataPropertyHandling.HasValue)
		{
			serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
		}
		if (settings._typeNameAssemblyFormatHandling.HasValue)
		{
			serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
		}
		if (settings._preserveReferencesHandling.HasValue)
		{
			serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
		}
		if (settings._referenceLoopHandling.HasValue)
		{
			serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
		}
		if (settings._missingMemberHandling.HasValue)
		{
			serializer.MissingMemberHandling = settings.MissingMemberHandling;
		}
		if (settings._objectCreationHandling.HasValue)
		{
			serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
		}
		if (settings._nullValueHandling.HasValue)
		{
			serializer.NullValueHandling = settings.NullValueHandling;
		}
		if (settings._defaultValueHandling.HasValue)
		{
			serializer.DefaultValueHandling = settings.DefaultValueHandling;
		}
		if (settings._constructorHandling.HasValue)
		{
			serializer.ConstructorHandling = settings.ConstructorHandling;
		}
		if (settings._context.HasValue)
		{
			serializer.Context = settings.Context;
		}
		if (settings._checkAdditionalContent.HasValue)
		{
			serializer._checkAdditionalContent = settings._checkAdditionalContent;
		}
		if (settings.Error != null)
		{
			serializer.Error += settings.Error;
		}
		if (settings.ContractResolver != null)
		{
			serializer.ContractResolver = settings.ContractResolver;
		}
		if (settings.ReferenceResolverProvider != null)
		{
			serializer.ReferenceResolver = settings.ReferenceResolverProvider();
		}
		if (settings.TraceWriter != null)
		{
			serializer.TraceWriter = settings.TraceWriter;
		}
		if (settings.EqualityComparer != null)
		{
			serializer.EqualityComparer = settings.EqualityComparer;
		}
		if (settings.SerializationBinder != null)
		{
			serializer.SerializationBinder = settings.SerializationBinder;
		}
		if (settings._formatting.HasValue)
		{
			serializer._formatting = settings._formatting;
		}
		if (settings._dateFormatHandling.HasValue)
		{
			serializer._dateFormatHandling = settings._dateFormatHandling;
		}
		if (settings._dateTimeZoneHandling.HasValue)
		{
			serializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
		}
		if (settings._dateParseHandling.HasValue)
		{
			serializer._dateParseHandling = settings._dateParseHandling;
		}
		if (settings._dateFormatStringSet)
		{
			serializer._dateFormatString = settings._dateFormatString;
			serializer._dateFormatStringSet = settings._dateFormatStringSet;
		}
		if (settings._floatFormatHandling.HasValue)
		{
			serializer._floatFormatHandling = settings._floatFormatHandling;
		}
		if (settings._floatParseHandling.HasValue)
		{
			serializer._floatParseHandling = settings._floatParseHandling;
		}
		if (settings._stringEscapeHandling.HasValue)
		{
			serializer._stringEscapeHandling = settings._stringEscapeHandling;
		}
		if (settings._culture != null)
		{
			serializer._culture = settings._culture;
		}
		if (settings._maxDepthSet)
		{
			serializer._maxDepth = settings._maxDepth;
			serializer._maxDepthSet = settings._maxDepthSet;
		}
	}

	[DebuggerStepThrough]
	public void Populate(TextReader reader, object target)
	{
		Populate(new JsonTextReader(reader), target);
	}

	[DebuggerStepThrough]
	public void Populate(JsonReader reader, object target)
	{
		PopulateInternal(reader, target);
	}

	internal virtual void PopulateInternal(JsonReader reader, object target)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		ValidationUtils.ArgumentNotNull(target, "target");
		SetupReader(reader, out var previousCulture, out var previousDateTimeZoneHandling, out var previousDateParseHandling, out var previousFloatParseHandling, out var previousMaxDepth, out var previousDateFormatString);
		TraceJsonReader traceJsonReader = ((TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose) ? CreateTraceJsonReader(reader) : null);
		new JsonSerializerInternalReader(this).Populate(traceJsonReader ?? reader, target);
		if (traceJsonReader != null)
		{
			TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
		}
		ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public object Deserialize(JsonReader reader)
	{
		return Deserialize(reader, null);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public object Deserialize(TextReader reader, Type objectType)
	{
		return Deserialize(new JsonTextReader(reader), objectType);
	}

	[DebuggerStepThrough]
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public T Deserialize<T>([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonReader reader)
	{
		return (T)Deserialize(reader, typeof(T));
	}

	[DebuggerStepThrough]
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public object Deserialize([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonReader reader, Type objectType)
	{
		return DeserializeInternal(reader, objectType);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal virtual object DeserializeInternal([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonReader reader, Type objectType)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		SetupReader(reader, out var previousCulture, out var previousDateTimeZoneHandling, out var previousDateParseHandling, out var previousFloatParseHandling, out var previousMaxDepth, out var previousDateFormatString);
		TraceJsonReader traceJsonReader = ((TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose) ? CreateTraceJsonReader(reader) : null);
		object result = new JsonSerializerInternalReader(this).Deserialize(traceJsonReader ?? reader, objectType, CheckAdditionalContent);
		if (traceJsonReader != null)
		{
			TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
		}
		ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
		return result;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal void SetupReader([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonReader reader, out CultureInfo previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString)
	{
		if (_culture != null && !_culture.Equals(reader.Culture))
		{
			previousCulture = reader.Culture;
			reader.Culture = _culture;
		}
		else
		{
			previousCulture = null;
		}
		if (_dateTimeZoneHandling.HasValue && reader.DateTimeZoneHandling != _dateTimeZoneHandling)
		{
			previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
			reader.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
		}
		else
		{
			previousDateTimeZoneHandling = null;
		}
		if (_dateParseHandling.HasValue && reader.DateParseHandling != _dateParseHandling)
		{
			previousDateParseHandling = reader.DateParseHandling;
			reader.DateParseHandling = _dateParseHandling.GetValueOrDefault();
		}
		else
		{
			previousDateParseHandling = null;
		}
		if (_floatParseHandling.HasValue && reader.FloatParseHandling != _floatParseHandling)
		{
			previousFloatParseHandling = reader.FloatParseHandling;
			reader.FloatParseHandling = _floatParseHandling.GetValueOrDefault();
		}
		else
		{
			previousFloatParseHandling = null;
		}
		if (_maxDepthSet && reader.MaxDepth != _maxDepth)
		{
			previousMaxDepth = reader.MaxDepth;
			reader.MaxDepth = _maxDepth;
		}
		else
		{
			previousMaxDepth = null;
		}
		if (_dateFormatStringSet && reader.DateFormatString != _dateFormatString)
		{
			previousDateFormatString = reader.DateFormatString;
			reader.DateFormatString = _dateFormatString;
		}
		else
		{
			previousDateFormatString = null;
		}
		if (reader is JsonTextReader { PropertyNameTable: null } jsonTextReader && _contractResolver is DefaultContractResolver defaultContractResolver)
		{
			jsonTextReader.PropertyNameTable = defaultContractResolver.GetNameTable();
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	private void ResetReader([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonReader reader, CultureInfo previousCulture, DateTimeZoneHandling? previousDateTimeZoneHandling, DateParseHandling? previousDateParseHandling, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string previousDateFormatString)
	{
		if (previousCulture != null)
		{
			reader.Culture = previousCulture;
		}
		if (previousDateTimeZoneHandling.HasValue)
		{
			reader.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
		}
		if (previousDateParseHandling.HasValue)
		{
			reader.DateParseHandling = previousDateParseHandling.GetValueOrDefault();
		}
		if (previousFloatParseHandling.HasValue)
		{
			reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
		}
		if (_maxDepthSet)
		{
			reader.MaxDepth = previousMaxDepth;
		}
		if (_dateFormatStringSet)
		{
			reader.DateFormatString = previousDateFormatString;
		}
		if (reader is JsonTextReader { PropertyNameTable: not null } jsonTextReader && _contractResolver is DefaultContractResolver defaultContractResolver && jsonTextReader.PropertyNameTable == defaultContractResolver.GetNameTable())
		{
			jsonTextReader.PropertyNameTable = null;
		}
	}

	public void Serialize(TextWriter textWriter, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		Serialize(new JsonTextWriter(textWriter), value);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public void Serialize([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonWriter jsonWriter, object value, Type objectType)
	{
		SerializeInternal(jsonWriter, value, objectType);
	}

	public void Serialize(TextWriter textWriter, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, Type objectType)
	{
		Serialize(new JsonTextWriter(textWriter), value, objectType);
	}

	public void Serialize(JsonWriter jsonWriter, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		SerializeInternal(jsonWriter, value, null);
	}

	private TraceJsonReader CreateTraceJsonReader(JsonReader reader)
	{
		TraceJsonReader traceJsonReader = new TraceJsonReader(reader);
		if (reader.TokenType != JsonToken.None)
		{
			traceJsonReader.WriteCurrentToken();
		}
		return traceJsonReader;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal virtual void SerializeInternal([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JsonWriter jsonWriter, object value, Type objectType)
	{
		ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");
		Formatting? formatting = null;
		if (_formatting.HasValue && jsonWriter.Formatting != _formatting)
		{
			formatting = jsonWriter.Formatting;
			jsonWriter.Formatting = _formatting.GetValueOrDefault();
		}
		DateFormatHandling? dateFormatHandling = null;
		if (_dateFormatHandling.HasValue && jsonWriter.DateFormatHandling != _dateFormatHandling)
		{
			dateFormatHandling = jsonWriter.DateFormatHandling;
			jsonWriter.DateFormatHandling = _dateFormatHandling.GetValueOrDefault();
		}
		DateTimeZoneHandling? dateTimeZoneHandling = null;
		if (_dateTimeZoneHandling.HasValue && jsonWriter.DateTimeZoneHandling != _dateTimeZoneHandling)
		{
			dateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
			jsonWriter.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
		}
		FloatFormatHandling? floatFormatHandling = null;
		if (_floatFormatHandling.HasValue && jsonWriter.FloatFormatHandling != _floatFormatHandling)
		{
			floatFormatHandling = jsonWriter.FloatFormatHandling;
			jsonWriter.FloatFormatHandling = _floatFormatHandling.GetValueOrDefault();
		}
		StringEscapeHandling? stringEscapeHandling = null;
		if (_stringEscapeHandling.HasValue && jsonWriter.StringEscapeHandling != _stringEscapeHandling)
		{
			stringEscapeHandling = jsonWriter.StringEscapeHandling;
			jsonWriter.StringEscapeHandling = _stringEscapeHandling.GetValueOrDefault();
		}
		CultureInfo cultureInfo = null;
		if (_culture != null && !_culture.Equals(jsonWriter.Culture))
		{
			cultureInfo = jsonWriter.Culture;
			jsonWriter.Culture = _culture;
		}
		string dateFormatString = null;
		if (_dateFormatStringSet && jsonWriter.DateFormatString != _dateFormatString)
		{
			dateFormatString = jsonWriter.DateFormatString;
			jsonWriter.DateFormatString = _dateFormatString;
		}
		TraceJsonWriter traceJsonWriter = ((TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose) ? new TraceJsonWriter(jsonWriter) : null);
		new JsonSerializerInternalWriter(this).Serialize(traceJsonWriter ?? jsonWriter, value, objectType);
		if (traceJsonWriter != null)
		{
			TraceWriter.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
		}
		if (formatting.HasValue)
		{
			jsonWriter.Formatting = formatting.GetValueOrDefault();
		}
		if (dateFormatHandling.HasValue)
		{
			jsonWriter.DateFormatHandling = dateFormatHandling.GetValueOrDefault();
		}
		if (dateTimeZoneHandling.HasValue)
		{
			jsonWriter.DateTimeZoneHandling = dateTimeZoneHandling.GetValueOrDefault();
		}
		if (floatFormatHandling.HasValue)
		{
			jsonWriter.FloatFormatHandling = floatFormatHandling.GetValueOrDefault();
		}
		if (stringEscapeHandling.HasValue)
		{
			jsonWriter.StringEscapeHandling = stringEscapeHandling.GetValueOrDefault();
		}
		if (_dateFormatStringSet)
		{
			jsonWriter.DateFormatString = dateFormatString;
		}
		if (cultureInfo != null)
		{
			jsonWriter.Culture = cultureInfo;
		}
	}

	internal IReferenceResolver GetReferenceResolver()
	{
		if (_referenceResolver == null)
		{
			_referenceResolver = new DefaultReferenceResolver();
		}
		return _referenceResolver;
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal JsonConverter GetMatchingConverter(Type type)
	{
		return GetMatchingConverter(_converters, type);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal static JsonConverter GetMatchingConverter([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })] IList<JsonConverter> converters, Type objectType)
	{
		if (converters != null)
		{
			for (int i = 0; i < converters.Count; i++)
			{
				JsonConverter jsonConverter = converters[i];
				if (jsonConverter.CanConvert(objectType))
				{
					return jsonConverter;
				}
			}
		}
		return null;
	}

	internal void OnError(Newtonsoft.Json.Serialization.ErrorEventArgs e)
	{
		this.Error?.Invoke(this, e);
	}
}
