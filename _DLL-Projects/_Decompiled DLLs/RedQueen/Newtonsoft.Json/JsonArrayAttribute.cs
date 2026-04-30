using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
internal sealed class JsonArrayAttribute : JsonContainerAttribute
{
	private bool _allowNullItems;

	public bool AllowNullItems
	{
		get
		{
			return _allowNullItems;
		}
		set
		{
			_allowNullItems = value;
		}
	}

	public JsonArrayAttribute()
	{
	}

	public JsonArrayAttribute(bool allowNullItems)
	{
		_allowNullItems = allowNullItems;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public JsonArrayAttribute(string id)
		: base(id)
	{
	}
}
