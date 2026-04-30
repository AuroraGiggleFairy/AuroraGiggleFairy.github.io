using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JConstructor : JContainer
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private string _name;

	private readonly List<JToken> _values = new List<JToken>();

	protected override IList<JToken> ChildrenTokens => _values;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public string Name
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _name;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_name = value;
		}
	}

	public override JTokenType Type => JTokenType.Constructor;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override JToken this[object key]
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
		get
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int index))
			{
				throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			return GetItem(index);
		}
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
		set
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int index))
			{
				throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			SetItem(index, value);
		}
	}

	public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		await writer.WriteStartConstructorAsync(_name ?? string.Empty, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < _values.Count; i++)
		{
			await _values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(continueOnCapturedContext: false);
		}
		await writer.WriteEndConstructorAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public new static Task<JConstructor> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return LoadAsync(reader, null, cancellationToken);
	}

	public new static async Task<JConstructor> LoadAsync(JsonReader reader, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonLoadSettings settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.StartConstructor)
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JConstructor c = new JConstructor((string)reader.Value);
		c.SetLineInfo(reader as IJsonLineInfo, settings);
		await c.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return c;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal override int IndexOfItem(JToken item)
	{
		if (item == null)
		{
			return -1;
		}
		return _values.IndexOfReference(item);
	}

	internal override void MergeItem(object content, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonMergeSettings settings)
	{
		if (content is JConstructor jConstructor)
		{
			if (jConstructor.Name != null)
			{
				Name = jConstructor.Name;
			}
			JContainer.MergeEnumerableContent(this, jConstructor, settings);
		}
	}

	public JConstructor()
	{
	}

	public JConstructor(JConstructor other)
		: base(other, null)
	{
		_name = other.Name;
	}

	internal JConstructor(JConstructor other, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonCloneSettings settings)
		: base(other, settings)
	{
		_name = other.Name;
	}

	public JConstructor(string name, params object[] content)
		: this(name, (object)content)
	{
	}

	public JConstructor(string name, object content)
		: this(name)
	{
		Add(content);
	}

	public JConstructor(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Constructor name cannot be empty.", "name");
		}
		_name = name;
	}

	internal override bool DeepEquals(JToken node)
	{
		if (node is JConstructor jConstructor && _name == jConstructor.Name)
		{
			return ContentsEqual(jConstructor);
		}
		return false;
	}

	internal override JToken CloneToken([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonCloneSettings settings = null)
	{
		return new JConstructor(this, settings);
	}

	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WriteStartConstructor(_name);
		int count = _values.Count;
		for (int i = 0; i < count; i++)
		{
			_values[i].WriteTo(writer, converters);
		}
		writer.WriteEndConstructor();
	}

	internal override int GetDeepHashCode()
	{
		return (_name?.GetHashCode() ?? 0) ^ ContentsHashCode();
	}

	public new static JConstructor Load(JsonReader reader)
	{
		return Load(reader, null);
	}

	public new static JConstructor Load(JsonReader reader, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonLoadSettings settings)
	{
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.StartConstructor)
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JConstructor jConstructor = new JConstructor((string)reader.Value);
		jConstructor.SetLineInfo(reader as IJsonLineInfo, settings);
		jConstructor.ReadTokenFrom(reader, settings);
		return jConstructor;
	}
}
